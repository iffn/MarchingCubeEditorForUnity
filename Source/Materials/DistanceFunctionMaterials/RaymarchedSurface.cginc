#ifndef RAYMARCHED_SURFACE_INCLUDED
#define RAYMARCHED_SURFACE_INCLUDED

// Estimate normals using the SDF
float3 estimateNormal(float3 worldPosition)
{
    float epsilon = 0.001;
    float distance = SDF(worldPosition);

    float dx = SDF(worldPosition + float3(epsilon, 0, 0)) - distance;
    float dy = SDF(worldPosition + float3(0, epsilon, 0)) - distance;
    float dz = SDF(worldPosition + float3(0, 0, epsilon)) - distance;

    return normalize(float3(dx, dy, dz));
}

// Generic raymarching routine — expects `map(float3)` to be defined elsewhere
float raymarch(float3 rayOrigin, float3 rayDirection, out float3 hitPoint)
{
    float t = 0.0;

    for (int i = 0; i < 640; i++)
    {
        hitPoint = rayOrigin + rayDirection * t;

        float dist = SDF(hitPoint);

        if (dist < t * 0.01)
            return t;

        if (t > 2000.0)
            return -1.0;

        t += dist;
    }

    return -1.0;
}

struct ShadingResult
{
    float shading;
    float depth;
    bool hit;
};

ShadingResult ComputeShading(float3 worldPos, float3 cameraPos)
{
    ShadingResult result;
    result.shading = 0.0;
    result.depth = 0.0;
    result.hit = false;

    float3 rayOrigin = worldPos;
    float3 rayDirection = normalize(worldPos - cameraPos);

    float3 hitPoint;
    float t = raymarch(rayOrigin, rayDirection, hitPoint);

    if (t > 0.0)
    {
        float3 normal = estimateNormal(hitPoint);
        float ambient = 0.2;
        float viewFactor = max(dot(normal, normalize(-rayDirection)), 0.0);
        result.shading = ambient + viewFactor * 0.8;

        float4 clipPos = UnityWorldToClipPos(hitPoint);
        result.depth = clipPos.z / clipPos.w;
        result.hit = true;
    }

    return result;
}

#endif // RAYMARCHED_SURFACE_INCLUDED
