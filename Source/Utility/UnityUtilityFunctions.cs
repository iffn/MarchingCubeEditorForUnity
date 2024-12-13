using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityUtilityFunctions
{
    public static Vector2 ComponentwiseMultiply(this Vector2 a, Vector2 b)
    {
        return new Vector2(a.x * b.x, a.y * b.y);
    }

    public static Vector3 ComponentwiseMultiply(this Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector2 InverseComponents(Vector2 vector)
    {
        return new Vector2(1f / vector.x, 1f / vector.y);
    }

    public static Vector3 InverseComponents(Vector3 vector)
    {
        return new Vector3(1f/vector.x, 1f/vector.y, 1f/vector.z);
    }

    /// <summary>
    ///  This function calculates the intersection points of a given ray with this box.
    /// </summary>
    /// ChatGPT: https://chatgpt.com/share/675ac236-2840-800e-b128-9d570ca5b6d8
    public static (Vector3, Vector3)? GetIntersectRayPoints(this Bounds bounds, Ray ray)
    {
        // Ray does not intersect with bounds, therefore no points to return.
        if (!bounds.IntersectRay(ray))
        {
            return null;
        }
        
        // Calculate intersection points (entry and exit)
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        float tMin = float.MinValue, tMax = float.MaxValue;

        for (int i = 0; i < 3; i++)
        {
            if (ray.direction[i] != 0)
            {
                float t1 = (min[i] - ray.origin[i]) / ray.direction[i];
                float t2 = (max[i] - ray.origin[i]) / ray.direction[i];

                if (t1 > t2)
                {
                    (t1, t2) = (t2, t1); // Swap if t1 > t2
                }

                tMin = Mathf.Max(tMin, t1);
                tMax = Mathf.Min(tMax, t2);
            }
            else if (ray.origin[i] < min[i] || ray.origin[i] > max[i])
            {
                return null; // Ray is parallel and outside the bounds
            }
        }

        if (tMin > tMax || tMax < 0)
        {
            return null; // No valid intersection
        }

        // Calculate intersection points
        Vector3 entryPoint = ray.origin + tMin * ray.direction;
        Vector3 exitPoint = ray.origin + tMax * ray.direction;

        return (entryPoint, exitPoint);
    }
}
