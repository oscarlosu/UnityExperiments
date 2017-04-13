// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Water/Foam" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_FoamColor ("Foam Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Amplitude ("Amplitude", float) = 1.0
		_xFrequency ("X Frequency", float) = 1.0
		_yFrequency ("Y Frequency", float) = 1.0
		_useFoam ("Use foam", Range(0,1)) = 1
		_Foam ("Wave Shape", 2D) = "white"{}
		_FoamWiggle ("Foam Wiggle (u freq, u amplitude, v freq, v amplitude)", Vector) = (1.0, 1.0, 1.0, 1.0)

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
			uniform float4 _FoamColor;
			uniform float _Amplitude;
			uniform float _xFrequency;
			uniform float _yFrequency;
			uniform float _foamThreshold;
			uniform sampler2D _Foam;
			uniform float4 _Foam_ST;
			uniform float4 _FoamWiggle;
			uniform bool _useFoam;

			// base input structs
			struct vertexInput {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float4 worldPos : TEXCOORD1;
				float offset : PSIZE;
			};
			// vertex function
			vertexOutput vert(vertexInput i) {
				vertexOutput o;


				o.worldPos = mul(unity_ObjectToWorld, i.vertex);

				o.offset = cos((o.worldPos.x * _xFrequency + _Time.y)) * sin((o.worldPos.z * _yFrequency + _Time.y));
				o.pos = float4(i.vertex.x, i.vertex.y + o.offset * _Amplitude, i.vertex.z, i.vertex.w);

				o.pos = UnityObjectToClipPos(o.pos);

				o.uv = i.texcoord;

				return o;
			}

			// fragment function
			float4 frag(vertexOutput i) : COLOR {
				if(_useFoam < 0.5) {
					return _Color;
				}
				float u = i.uv.x + _FoamWiggle.y * sin(i.worldPos.x * _FoamWiggle.x + _Time.y);
				float v = i.uv.y + _FoamWiggle.w * sin(i.worldPos.z * _FoamWiggle.z + _Time.y);
				fixed4 tex = tex2D(_Foam, float2(u, v));
				float4 color = saturate(_Color + tex);
				color.a = any(tex) ? 1 : _Color.a;
				return color;
			}

			

			ENDCG
		}
	}
	// fallback commented out during developments
	//Fallback "Diffuse"
}