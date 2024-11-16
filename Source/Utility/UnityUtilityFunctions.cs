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
}
