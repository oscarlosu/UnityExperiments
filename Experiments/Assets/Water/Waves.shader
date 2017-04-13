// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Water/Waves" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Amplitude ("Amplitude", float) = 1.0
		_xFrequency ("X Frequency", float) = 1.0
		_yFrequency ("Y Frequency", float) = 1.0
	}
	SubShader {
		Pass {
			Tags {		
				"RenderQueue" = "Transparent"
			}
			Blend SrcAlpha OneMinusSrcAlpha

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

			// base input structs
			struct vertexInput {
				float4 vertex : POSITION;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
			};
			// vertex function
			vertexOutput vert(vertexInput i) {
				vertexOutput o;

				float3 worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;

				float offset = cos((worldPos.x * _xFrequency + _Time.w)) * sin((worldPos.z * _yFrequency + _Time.w));
				offset = offset * _Amplitude;
				o.pos = float4(i.vertex.x, i.vertex.y + offset, i.vertex.z, i.vertex.w);

				o.pos = UnityObjectToClipPos(o.pos);

				return o;
			}
			// fragment function
			float4 frag(vertexOutput i) : COLOR {
				return _Color;
			}
			ENDCG
		}
	}
	// fallback commented out during developments
	//Fallback "Diffuse"
}