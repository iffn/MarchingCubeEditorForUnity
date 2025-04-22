#ifndef SDF_MATH_INCLUDED
#define SDF_MATH_INCLUDED

float sphereSDF(float3 samplePoint, float radius)
{
    return length(samplePoint) - radius;
}

float boxSDF(float3 samplePoint, float3 sideLengths)
{
    float3 absPoint = abs(samplePoint);

    float3 halfExtends = 0.5 * sideLengths;
    float3 distanceToSurface = absPoint - halfExtends;

    float outsideDistance = length(max(distanceToSurface, 0.0));
    float insideDistance = min(max(distanceToSurface.x, max(distanceToSurface.y, distanceToSurface.z)), 0.0);

    return outsideDistance + insideDistance;
}

float planeFloor(float3 samplePoint)
{
    return samplePoint.y;
}

/*
Boolean logic:
    A     ->  A
    !A    ->  -A
    A & B ->  Max(A,B)
    A | B ->  Min(A,B)
*/

float CombineAddSDF(float a, float b) // Same as (A | B)
{
    return min(a, b);
}

float CombineSubtractSDF(float a, float b) // Same as (A & !B)
{
    return max(a, -b);
}

float CombineIntersectSDF(float a, float b) // Same as (A & B)
{
    return max(a, b);
}

#endif // RAYMARCHED_SURFACE_INCLUDED
