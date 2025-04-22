#ifndef RAYMARCHED_SHADER_END_INCLUDED
#define RAYMARCHED_SHADER_END_INCLUDED

#include "RaymarchedSurface.cginc"
            
fixed4 frag(v2f input, out float depth : SV_Depth) : SV_Target
{
    ShadingResult result = ComputeShading(input.worldPos, _WorldSpaceCameraPos);

    if (result.hit)
    {
        depth = result.depth;
        float alpha = _Color.a; // Or compute your own transparency
        return fixed4(_Color.rgb * result.shading, alpha);
    }

    clip(-1.0);
    return fixed4(0, 0, 0, 0);
}

#endif // RAYMARCHED_SURFACE_INCLUDED
