#ifndef RAYMARCHED_SURFACE_INCLUDED
#define RAYMARCHED_SURFACE_INCLUDED

float3 worldToLocalPosition(float3 worldPosition)
{
    return mul(unity_WorldToObject, float4(worldPosition, 1)).xyz;
}

float3 localToWorldPosition(float3 localPosition)
{
    return mul(unity_ObjectToWorld, float4(localPosition, 1)).xyz;
}

float3 worldToLocalDirection(float3 worldDirection)
{
    return normalize(mul((float3x3)unity_WorldToObject, worldDirection));
}

float3 localToWorldDirection(float3 localDirection)
{
    return normalize(mul((float3x3)unity_ObjectToWorld, localDirection));
}

// Generic raymarching routine — expects 'SDF(float3)' to be defined elsewhere
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

// Estimate normals using the SDF
float3 estimateNormal(float3 localPosition)
{
    float epsilon = 0.001;
    float distance = SDF(localPosition);

    float dx = SDF(localPosition + float3(epsilon, 0, 0)) - distance;
    float dy = SDF(localPosition + float3(0, epsilon, 0)) - distance;
    float dz = SDF(localPosition + float3(0, 0, epsilon)) - distance;

    return normalize(float3(dx, dy, dz));
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

    float3 rayOriginLocal = worldToLocalPosition(worldPos);
    float3 rayDirectionWorld = normalize(worldPos - cameraPos);
    float3 rayDirectionLocal = worldToLocalDirection(rayDirectionWorld);

    float3 hitPointLocal;
    float t = raymarch(rayOriginLocal, rayDirectionLocal, hitPointLocal);
    
    float3 hitPointWorld = localToWorldPosition(hitPointLocal);

    if (t > 0.0)
    {
        float3 normalLocal = estimateNormal(hitPointLocal);
        float3 normalWorld = localToWorldDirection(normalLocal);
        float ambient = 0.2;
        float viewFactor = max(dot(normalWorld, normalize(-rayDirectionWorld)), 0.0);
        result.shading = ambient + viewFactor * 0.8;

        float4 clipPos = UnityWorldToClipPos(hitPointWorld);
        result.depth = clipPos.z / clipPos.w;
        result.hit = true;
    }

    return result;
}

#endif // RAYMARCHED_SURFACE_INCLUDED
