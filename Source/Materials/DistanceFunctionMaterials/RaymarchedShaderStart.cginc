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
    output.pos = UnityObjectToClipPos(input.vertex);
    output.worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
    return output;
}

#endif // RAYMARCHED_SURFACE_INCLUDED
