// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "UnityCookie/Beginner/8b_EmitMap" {
	Properties {
		_Color ("Color Tint", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex ("Diffuse Texture gloss(A)", 2D) = "white"{}

		_BumpMap ("Normal Texture", 2D) = "bump"{}
		_BumpDepth ("Bump Depth", Range(-2.0, 2.0)) = 1.0

		_EmitMap ("Emission Texture", 2D) = "black"{}
		_EmitStrength ("Emission Strength", Range(0.0, 2.0)) = 0.0

		_SpecColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Shininess ("Shininess", Float) = 10

		_RimColor ("Rim Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_RimPower ("Rim Power", Range(0.1, 10.0)) = 3.0


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
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			uniform sampler2D _BumpMap;
			uniform float4 _BumpMap_ST;
			uniform float _BumpDepth;

			uniform sampler2D _EmitMap;
			uniform float4 _EmitMap_ST;
			uniform float _EmitStrength;


			uniform float4 _Color;
			uniform float4 _SpecColor;
			uniform float _Shininess;
			uniform float4 _RimColor;
			uniform float _RimPower;

			// Unity defined variables
			uniform float4 _LightColor0;

			// base input structs
			struct vertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				float4 tangent : TANGENT;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				float3 normalWorld : TEXCOORD2;

				float3 tangentWorld: TEXCOORD3;
				float3 binormalWorld : TEXTCOORD4;
			};

			// vertex function
			vertexOutput vert(vertexInput i) {
				vertexOutput o;
				// Position
				o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
				o.posWorld = mul(unity_ObjectToWorld, i.vertex);
				o.tex = i.texcoord;

				o.normalWorld = normalize(mul(float4(i.normal, 0.0), unity_WorldToObject).xyz);
				o.tangentWorld = normalize(mul(unity_ObjectToWorld, i.tangent).xyz);
				o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * i.tangent.w);



				return o;
			}

			// fragment function
			float4 frag(vertexOutput i) : COLOR {
				

				// Texture maps
				float4 tex = tex2D(_MainTex, i.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw);
				float4 texN = tex2D(_BumpMap, i.tex.xy * _BumpMap_ST.xy + _BumpMap_ST.zw);
				float4 texE = tex2D(_EmitMap, i.tex.xy * _EmitMap_ST.xy + _EmitMap_ST.zw);

				// Unpacking normal function
				float3 localCoords = float3(2.0 * texN.ag - float2(1.0, 1.0), 0.0);
				// Standard
				//localCoords.z = 1.0 - 0.5 * dot(localCoords, localCoords);
				// Custom
				localCoords.z = _BumpDepth;
				// normal transpose matrix
				float3x3 local2WorldTranspose = float3x3(
					i.tangentWorld,
					i.binormalWorld,
					i.normalWorld
				);
				// calculate normal direction
				float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));


				// Lambert light
				float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);
				// Directional / point light
				float atten;
				float3 lightDirection;
				if(_WorldSpaceLightPos0.w == 0.0) {
					atten = 1.0;
					lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				} else {
					float3 fragmentToLightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
					float distance = length(fragmentToLightSource);
					atten = 1.0 / distance;
					lightDirection = normalize(fragmentToLightSource);
				}
				// Diffuse and specular
				float3 diffuseReflection = atten * _LightColor0.rgb * saturate(dot(normalDirection, lightDirection));
				float3 specularReflection = diffuseReflection * _SpecColor.rgb * pow(saturate(dot(reflect(- lightDirection, normalDirection), viewDirection)), _Shininess);
				// Rim lighting
				float rim = 1 - saturate(dot(normalize(viewDirection), normalDirection));
				float3 rimLighting = atten * _LightColor0.rgb * _RimColor.rgb * saturate(dot(normalDirection, lightDirection)) * pow(rim , _RimPower);

				float3 lightFinal = diffuseReflection + (specularReflection * tex.a) + rimLighting + UNITY_LIGHTMODEL_AMBIENT.xyz + (texE.rgb * _EmitStrength);

				return float4(tex.rgb * lightFinal * _Color.rgb, 1.0);
			}
			ENDCG
		}

			Pass {
			Tags {
			// Necessary to have directional lights work with the shader
				"LightMode" = "ForwardAdd"
			}
			Blend One One
			CGPROGRAM
			// pragmas
			#pragma vertex vert
			#pragma fragment frag

			// user defined variables
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			uniform sampler2D _BumpMap;
			uniform float4 _BumpMap_ST;
			uniform float _BumpDepth;
			uniform float4 _Color;
			uniform float4 _SpecColor;
			uniform float _Shininess;
			uniform float4 _RimColor;
			uniform float _RimPower;

			// Unity defined variables
			uniform float4 _LightColor0;

			// base input structs
			struct vertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				float4 tangent : TANGENT;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				float3 normalWorld : TEXCOORD2;

				float3 tangentWorld: TEXCOORD3;
				float3 binormalWorld : TEXTCOORD4;
			};

			// vertex function
			vertexOutput vert(vertexInput i) {
				vertexOutput o;
				// Position
				o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
				o.posWorld = mul(unity_ObjectToWorld, i.vertex);
				o.tex = i.texcoord;

				o.normalWorld = normalize(mul(float4(i.normal, 0.0), unity_WorldToObject).xyz);
				o.tangentWorld = normalize(mul(unity_ObjectToWorld, i.tangent).xyz);
				o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * i.tangent.w);



				return o;
			}

			// fragment function
			float4 frag(vertexOutput i) : COLOR {
				

				// Texture maps
				float4 tex = tex2D(_MainTex, i.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw);
				float4 texN = tex2D(_BumpMap, i.tex.xy * _BumpMap_ST.xy + _BumpMap_ST.zw);

				// Unpacking normal function
				float3 localCoords = float3(2.0 * texN.ag - float2(1.0, 1.0), 0.0);
				// Standard
				//localCoords.z = 1.0 - 0.5 * dot(localCoords, localCoords);
				// Custom
				localCoords.z = _BumpDepth;
				// normal transpose matrix
				float3x3 local2WorldTranspose = float3x3(
					i.tangentWorld,
					i.binormalWorld,
					i.normalWorld
				);
				// calculate normal direction
				float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));


				// Lambert light
				float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);
				// Directional / point light
				float atten;
				float3 lightDirection;
				if(_WorldSpaceLightPos0.w == 0.0) {
					atten = 1.0;
					lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				} else {
					float3 fragmentToLightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
					float distance = length(fragmentToLightSource);
					atten = 1.0 / distance;
					lightDirection = normalize(fragmentToLightSource);
				}
				// Diffuse and specular
				float3 diffuseReflection = atten * _LightColor0.rgb * saturate(dot(normalDirection, lightDirection));
				float3 specularReflection = diffuseReflection * _SpecColor.rgb * pow(saturate(dot(reflect(- lightDirection, normalDirection), viewDirection)), _Shininess);
				// Rim lighting
				float rim = 1 - saturate(dot(normalize(viewDirection), normalDirection));
				float3 rimLighting = atten * _LightColor0.rgb * _RimColor.rgb * saturate(dot(normalDirection, lightDirection)) * pow(rim , _RimPower);

				float3 lightFinal = diffuseReflection + (specularReflection * tex.a) + rimLighting;

				return float4(lightFinal * _Color.rgb, 1.0);
			}
			ENDCG
		}

	// fallback commented out during developments
	//Fallback "Diffuse"
	}
}