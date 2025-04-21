Shader "DistanceFunctionShapes/RaymarchedSphere"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZWrite On
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };
            
            float4 _Color;
            
            v2f vert(appdata_t input)
            {
                v2f output;
                output.pos = UnityObjectToClipPos(input.vertex);
                output.worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
                return output;
            }
            
            float SDF(float3 worldPosition)
            {
                float3 objectLocal = mul(unity_WorldToObject, float4(worldPosition, 1)).xyz;
                return length(objectLocal) - 0.5;
            }

            #include "RaymarchedSurface.cginc"
            
            fixed4 frag(v2f input, out float depth : SV_Depth) : SV_Target
            {
                ShadingResult result = ComputeShading(input.worldPos, _WorldSpaceCameraPos);

                if (result.hit)
                {
                    depth = result.depth;
                    return fixed4(_Color.rgb * result.shading, 1.0);
                }

                clip(-1.0);
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
