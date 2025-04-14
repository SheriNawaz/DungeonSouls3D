Shader "Hidden/Skinn/VertexColor/Unlit/Overlay"
{
	Properties
	{
	}

	SubShader
	{
		Tags
	{
			"RenderType" = "Opaque"
		}
		Pass
	{
			ZTest Always

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
				i.normalDir = normalize(i.normalDir);
				fixed4 finalRGBA = i.vertexColor.rgba;
				return finalRGBA;
			}
			ENDCG
		}

	}
		FallBack "Diffuse"
}
