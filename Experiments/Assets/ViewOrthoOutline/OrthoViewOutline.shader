// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/OrthoViewOutline" {
	Properties {		
		_Color ("Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_OutlineColor ("Outline Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Thickness ("Thickness", float) = 0.1
	}
	SubShader {
		Pass {
			CGPROGRAM
			// pragmas
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			// user defined variables
			uniform float4 _Color;
			uniform float4 _OutlineColor;
			uniform float _Thickness;
			// base input structs
			struct vertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float3 viewDir : TEXCOORD0;
			};
			// vertex function
			vertexOutput vert(vertexInput i) {
				vertexOutput o;
				o.pos = UnityObjectToClipPos(i.vertex);
				o.normal = i.normal;
				o.viewDir = mul((float3x3)unity_ObjectToWorld, i.vertex) - _WorldSpaceCameraPos;
				return o;
			}
			// fragment function
			float4 frag(vertexOutput i) : COLOR {
				float d = dot(i.normal, i.viewDir);
				//return abs(d) * _OutlineColor;
				if(abs(d) < _Thickness) {
					return _OutlineColor;
				}

				return _Color;
			}
			ENDCG
		}
	}
	// fallback commented out during developments
	//Fallback "Diffuse"
}