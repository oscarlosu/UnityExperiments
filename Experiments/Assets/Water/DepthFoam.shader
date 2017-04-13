// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Water/DepthFoam" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_DepthFactor ("_DepthFactor", float) = 1.0
	}
	SubShader {
		Pass {
			Tags {		
				"RenderQueue" = "Transparent"
			}
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			CGPROGRAM
			// pragmas
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			// user defined variables
			uniform float4 _Color;
			uniform float _Amplitude;
			uniform float _xFrequency;
			uniform float _yFrequency;
			uniform sampler2D _CameraDepthTexture;
			uniform float _DepthFactor;

			// base input structs
			struct vertexInput {
				float4 vertex : POSITION;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 projPos : TEXCOORD0;
				float depth : TEXCOORD1;
			};
			// vertex function
			vertexOutput vert(vertexInput i) {
				vertexOutput o;
				/*
				float3 worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;

				float offset = cos((worldPos.x * _xFrequency + _Time.w)) * sin((worldPos.z * _yFrequency + _Time.w));
				offset = offset * _Amplitude;
				o.pos = float4(i.vertex.x, i.vertex.y + offset, i.vertex.z, i.vertex.w);

				o.pos = UnityObjectToClipPos(o.pos);
				*/

				o.pos = UnityObjectToClipPos(i.vertex);
				o.projPos = ComputeScreenPos (o.pos);
				o.depth = mul(UNITY_MATRIX_MV, i.vertex).z;

				return o;
			}
			// fragment function
			float4 frag(vertexOutput i) : COLOR {
				float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture ,UNITY_PROJ_COORD(i.projPos)));
				float partZ = i.projPos.z;
				float fade = saturate (_DepthFactor * (sceneZ-partZ));
				//myWaterIntersectionTexture.a *= abs(1 - fade);
				return float4( i.projPos.z, 0, 0, 1);
			}
			ENDCG
		}
	}
	// fallback commented out during developments
	//Fallback "Diffuse"
}