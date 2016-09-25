// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "UnityCookie/Beginner/3b_Specular" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_SpecColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Shininess ("Shininess", float) = 10
	}
	SubShader {
		Pass {
			Tags {
			// Necessary to have directional lights work with the shader
				"LightMode" = "ForwardBase"
			}
			CGPROGRAM
			// pragmas
			#pragma vertex vert
			#pragma fragment frag

			// user defined variables
			uniform float4 _Color;
			uniform float4 _SpecColor;
			uniform float _Shininess;

			// Unity defined variables
			uniform float4 _LightColor0;

			// base input structs
			struct vertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 posWorld : TEXCOORD0;
				float3 normalDir : TEXCOORD1;
			};

			// vertex function
			vertexOutput vert(vertexInput i) {
				vertexOutput o;
				// Position
				o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
				o.posWorld = mul(unity_ObjectToWorld, i.vertex);
				o.normalDir = normalize(mul(float4(i.normal, 0.0), unity_WorldToObject).xyz);

				return o;
			}

			// fragment function
			float4 frag(vertexOutput i) : COLOR {
				// Lambert light
				float3 normalDirection = i.normalDir;
				float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);
				float atten = 1.0;
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 diffuseReflection = atten * _LightColor0.rgb * max(0.0, dot(normalDirection, lightDirection));
				float3 specularReflection = atten * _SpecColor.rgb * max(0.0, dot(normalDirection, lightDirection)) * pow(max(0.0, dot(reflect(- lightDirection, normalDirection), viewDirection)), _Shininess);
				float3 lightFinal = diffuseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;
				return float4(lightFinal * _Color.rgb, 1.0);
			}
			ENDCG
		}
	}
	// fallback commented out during developments
	//Fallback "Diffuse"
}