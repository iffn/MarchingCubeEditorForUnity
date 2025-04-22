Shader "DistanceFunctionShapes/RaymarchedSphere"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "RaymarchedShaderStart.cginc"

            #include "SDFMath.cginc"
            
            float SDF(float3 localPosition)
            {
                return sphereSDF(localPosition, 0.5);
            }
            
            #include "RaymarchedShaderEnd.cginc"
            
            ENDCG
        }
    }
    FallBack "Diffuse"
}
