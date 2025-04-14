Shader "Hidden/Skinn/Null/Unlit" {
    Properties {
    }
    SubShader {
        Tags {
            "Queue"="Overlay"
            "RenderType"="Opaque"
        }
        Pass {
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma target 3.0
            struct VertexInput 
			{
                float4 vertex : POSITION;
            };
            struct VertexOutput
			{
                float4 pos : SV_POSITION;
            };

            VertexOutput vert (VertexInput v) 
			{
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR
			{
                return fixed4(float3(1, 0, 1),1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
