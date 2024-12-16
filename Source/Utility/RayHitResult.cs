#if UNITY_EDITOR
using UnityEngine;

public readonly struct RayHitResult
{
    public static readonly RayHitResult None = new RayHitResult(Vector3.zero, Vector3.zero);

    public readonly Vector3 point;
    public readonly Vector3 normal;

    public RayHitResult(Vector3 point, Vector3 normal)
    {
        this.point = point;
        this.normal = normal;
    }

    public override bool Equals(object other) => other is RayHitResult hit && hit.normal == normal && hit.point == point;

    public override int GetHashCode() => point.GetHashCode();

    public static bool operator == (RayHitResult a, RayHitResult b) => a.Equals(b);

    public static bool operator != (RayHitResult a, RayHitResult b) => !a.Equals(b);
}
#endif