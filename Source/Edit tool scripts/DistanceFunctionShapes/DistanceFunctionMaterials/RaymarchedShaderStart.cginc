#ifndef RAYMARCHED_SHADER_START_INCLUDED
#define RAYMARCHED_SHADER_START_INCLUDED

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

    // Slightly scale the vertex outward from the origin (0,0,0)
    float scaleOffset = 1.01; // 1% outward
    float3 expandedVertex = input.vertex.xyz * scaleOffset;

    // Transform to world space
    float4 worldPos = mul(unity_ObjectToWorld, float4(expandedVertex, 1.0));
    output.pos = UnityObjectToClipPos(float4(expandedVertex, 1.0));
    output.worldPos = worldPos.xyz;

    return output;
}

#endif // RAYMARCHED_SURFACE_INCLUDED
