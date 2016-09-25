Shader "Custom/PaletteGradient" {
	Properties {
		_bias ("Offset", Vector) = (0.5, 0.5, 0.5)
		_scale ("Magnitude", Vector) = (0.5, 0.5, 0.5)
		_frequency ("Frequency", Vector) = (1.0, 1.0, 1.0)
		_phase ("Phase", Vector) = (0.0, 0.33, 0.67)
	}
	SubShader {
		Pass {
			CGPROGRAM
			// pragmas
			#pragma vertex vert
			#pragma fragment frag

			// user defined variables
			uniform float4 _bias;
			uniform float4 _scale;
			uniform float4 _frequency;
			uniform float4 _phase;

			
			// base input structs
			struct vertexInput {
				float4 vertex : POSITION;
			};
			struct vertexOutput {
				float4 hiddenPos : SV_POSITION;
				float4 pos : TEXCOORD0;
			};
			// vertex function
			vertexOutput vert(vertexInput i) {
				vertexOutput o;
				o.hiddenPos = mul(UNITY_MATRIX_MVP, i.vertex);
				o.pos = i.vertex;
				return o;
			}
			// fragment function
			float4 frag(vertexOutput i) : COLOR {
				const float PI = 3.14159f;
				return _bias + _scale * cos(PI * 2.0f * (_frequency * (i.pos.x + 1.0f) / 2.0f + _phase));
			}
			ENDCG
		}
	}
	// fallback commented out during developments
	//Fallback "Diffuse"
}