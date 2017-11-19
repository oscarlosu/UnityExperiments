// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "test/MyShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Length ("Length", float) = 1
		_Width ("Width", float) = 1
		_Gravity ("Gravity", float) = 1
		_Steps ("Step", int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
           
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float3 worldPosition : TEXCOORD1;
            };

			// Base properties
			float _Length;
			float _Width;
			float _Gravity;
			float _Steps;
 
			// This struct is the input data from the geometry shader.
			// Simply convert the data from the vertex shader to world position
			struct geomInput
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
			};
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
           
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
 
            [maxvertexcount(80)]
            void geom(triangle geomInput input[3], inout TriangleStream<geomInput> OutputStream)
            {
                // Because access the input data directly tend to make the code a mess, I usually repack everything in clean variables 
				float4 P1 = input[0].vertex;
				float4 P2 = input[1].vertex;
				float4 P3 = input[2].vertex;
 
				float4 N1 = input[0].normal;
				float4 N2 = input[1].normal;
				float4 N3 = input[2].normal;

				// I compute the point at the center of the face, its normal, and choose the lateral direction of the strand of grass.
 
				float4 P = (P1+P2+P3)/3.0f;
				float4 N = (N1+N2+N3)/3.0f;
				float4 T = float4(normalize((P2-P1).xyz), 0.0f);

				for( int i = 0; i < _Steps; i++ )
				{
					// Retrieve the normalized time along the strand.
					// It will be used to interpolate all the data from the bottom to the top of the strand.
					// t0 is the current step of the strand we are drawing.
					// t1 is the next step to be drawn. It will be t0 at the next iteration.
					// You could store its value for optimization.
 
					float t0 = (float)i / _Steps;
					float t1 = (float)(i+1) / _Steps;
 
					// Make our normal bend down with gravity.
					// The further we are on the strand, and the longer it is, the more it bends.
					// We then normalize this new direction, and scale it by the length at the current iteration of the loop.
 
					float4 c0 = normalize(N - (float4(0, _Length * t0, 0, 0) * _Gravity * t0)) * (_Length * t0);
					float4 c1 = normalize(N - (float4(0, _Length * t1, 0, 0) * _Gravity * t1)) * (_Length * t1);
 
					// Interpolate the width, and scale the lateral direction vector with it
 
					float4 w0 = T * lerp(_Width, 0, t0);
					float4 w1 = T * lerp(_Width, 0, t1);

					

					float4 p0 = c0 - w0;					
					float4 p1 = c0 + w0;
					float4 p2 = c1 - w1;
					float4 p3 = c1 + w1;
					float n = normalize(cross(p2 - p0, p3 - p1));


					geomInput newPoint;
					newPoint.vertex = p0;
					newPoint.normal = n;
					OutputStream.Append(newPoint);

					newPoint.vertex = p1;
					newPoint.normal = n;
					OutputStream.Append(newPoint);

					newPoint.vertex = p2;
					newPoint.normal = n;
					OutputStream.Append(newPoint);

					newPoint.vertex = p3;
					newPoint.normal = n;
					OutputStream.Append(newPoint);

				}
            }
           
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
 
                float3 lightDir = float3(1, 1, 0);
                float ndotl = dot(i.normal, normalize(lightDir));
 
                return col * ndotl;
            }
            ENDCG
        }
    }
}