#ifndef SDF_MATH_INCLUDED
#define SDF_MATH_INCLUDED

float sphereSDF(float3 p, float radius)
{
    return length(p) - radius;
}

#endif // RAYMARCHED_SURFACE_INCLUDED
