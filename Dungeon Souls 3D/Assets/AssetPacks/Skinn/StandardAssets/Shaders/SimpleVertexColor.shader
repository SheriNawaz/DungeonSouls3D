Shader "Skinn/VertexColor/CopyColor"
{
	Properties
	{
	  _Color("Copy Color", Color) = (1, 1, 1, 1)
	}

	SubShader
	{
		Tags
	{
			"RenderType" = "Opaque"
		}
		Pass
	{
			ZTest On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma target 3.0
			struct VertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 vertexColor : COLOR;
			};
			struct VertexOutput {
				float4 pos : SV_POSITION;
				float4 posWorld : TEXCOORD0;
				float3 normalDir : TEXCOORD1;
				float4 vertexColor : COLOR;
			};
			VertexOutput vert(VertexInput v) {
				VertexOutput o = (VertexOutput)0;
				o.vertexColor = v.vertexColor;
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

				fixed4 finalRGBA = fixed4(shaded, 1);
				return finalRGBA;
			}
			ENDCG
		}

	}
		FallBack "Diffuse"
}
