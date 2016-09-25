// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "UnityCookie/Beginner/2_Lambert" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	SubShader {
		Pass {
			Tags {
				"LightMode" = "ForwardBase"
			}
			CGPROGRAM
			// pragmas
			#pragma vertex vert
			#pragma fragment frag

			// user defined variables
			uniform float4 _Color;

			// Unity defined variables
			uniform float4 _LightColor0;

			// base input structs
			struct vertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 col : COLOR;
			};

			// vertex function
			vertexOutput vert(vertexInput i) {
				vertexOutput o;
				o.pos = mul(UNITY_MATRIX_MVP, i.vertex);

				// Lambert light
				float3 normalDirection = normalize(mul(float4(i.normal, 0.0), unity_WorldToObject).xyz);
				float atten = 1.0;
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 diffuseReflection = atten * _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection));


				o.col = float4(diffuseReflection, 1.0);
				return o;
			}

			// fragment function
			float4 frag(vertexOutput i) : COLOR {
				return i.col;
			}
			ENDCG
		}
	}
	// fallback commented out during developments
	//Fallback "Diffuse"
}