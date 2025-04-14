Shader "Skinn/VertexColor/Texture"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Copy Color", Color) = (1, 1, 1, 1)
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Pass
	{
			ZTest On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma target 3.0
			
			uniform sampler2D _MainTex; uniform float4 _MainTex_ST;

			sampler2D _BumpMap;

			struct VertexInput {
				float4 vertex : POSITION;
				float2 texcoord0 : TEXCOORD0;
				float3 normal : NORMAL;
				float4 vertexColor : COLOR;
			};
			struct VertexOutput {
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				float3 normalDir : TEXCOORD2;
				float4 vertexColor : COLOR;
			};
			VertexOutput vert(VertexInput v) {
				VertexOutput o = (VertexOutput)0;
				o.vertexColor = v.vertexColor;
				o.uv0 = v.texcoord0;
				o.normalDir = UnityObjectToWorldNormal(v.normal);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			float4 frag(VertexOutput i) : COLOR 
			{
				float3 color = (i.vertexColor.rgb);
				color.r = clamp(color.r, 0.001, 0.8999);
				color.g = clamp(color.g, 0.001, 0.8999);
				color.b = clamp(color.b, 0.001, 0.8999);

				float dark = 1.0 - ((i.normalDir * 3));
				float light = 1.0 - (1.0 - (-i.normalDir * 0.4));

				color = lerp(color, color * dark, 0.1);
				float3 shaded = lerp(color, float3(1.0, 1.0, 1.0), light);
				shaded = lerp(color, shaded, 0.5);

				shaded *= tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex));

				fixed4 finalRGBA = fixed4(shaded, 1);
				return finalRGBA;
			}
			ENDCG
		}

	}
		FallBack "Diffuse"
}
