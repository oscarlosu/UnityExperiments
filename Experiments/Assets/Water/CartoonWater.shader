Shader "Water/CartoonWater" {
     Properties {
         _Color ("Color", Color) = (1,1,1,1)
         _Glossiness("Smoothness", Range(0,1)) = 0.5
         _ShoreFade("Shoreline Distance", Float) = 1
     }
     SubShader {
             Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
         LOD 200
         
         CGPROGRAM
         // Physically based Standard lighting model, and enable shadows on all light types
         #pragma surface surf StandardSpecular     
 
         // Use shader model 3.0 target, to get nicer looking lighting
         #pragma target 3.0
 
         sampler2D _CameraDepthTexture;
         
         struct Input {
             float3 worldPos;
             float4 screenPos;
         };
 
         half _Glossiness;
         fixed4 _Color;
         float _ShoreFade;
 
         void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
 
             o.Albedo = _Color.rgb;
             o.Smoothness = _Glossiness;
         
             fixed4 depth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
                 
             o.Albedo = _Color;
             if (distance(LinearEyeDepth(depth), distance(_WorldSpaceCameraPos, IN.worldPos)) < _ShoreFade) {
                 o.Albedo = fixed4(1, 1, 1, 1);
             }
     
         }
         ENDCG
     }
     FallBack "Diffuse"
 }