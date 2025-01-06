using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SDFMath
{
    public static class ShapesDistanceOutsideIsPositive
    {
        public static float Sphere(Vector3 samplePoint, float radius)
        {
            return (samplePoint).magnitude - radius;
        }

        public static float Box(Vector3 samplePoint, Vector3 sideLengths)
        {
            Vector3 absPoint = new Vector3(
                Mathf.Abs(samplePoint.x),
                Mathf.Abs(samplePoint.y),
                Mathf.Abs(samplePoint.z)
            );

            Vector3 halfExtents = 0.5f * sideLengths;
            Vector3 delta = absPoint - halfExtents;

            return Mathf.Max(delta.x, delta.y, delta.z);
        }

        public static float PlaneFloor(Vector3 samplePoint)
        {
            return samplePoint.y;
        }

        public static float PlaneFloor(Vector3 samplePoint, float floorHeight)
        {
            return samplePoint.y - floorHeight;
        }

        public static float PlaneCeiling(Vector3 samplePoint)
        {
            return -PlaneFloor(samplePoint);
        }

        public static float PlaneCeiling(Vector3 samplePoint, float ceilingHeight)
        {
            return -PlaneFloor(samplePoint, ceilingHeight);
        }

        public static float DistanceToRoundedTube(Vector3 point, Vector3 lineStart, Vector3 lineEnd, float radius)
        {
            return DistanceToLineSegment(point, lineStart, lineEnd) - radius;
        }

        public static float DistanceToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 ab = lineEnd - lineStart;
            Vector3 ap = point - lineStart;

            float abLengthSquared = ab.sqrMagnitude; // Squared length of AB
            if (abLengthSquared == 0) return ap.magnitude; // A and B are the same point

            float t = Vector3.Dot(ap, ab) / abLengthSquared; // Projection factor
            t = Mathf.Clamp01(t); // Clamp to [0, 1] to stay on the segment

            Vector3 closestPoint = lineStart + t * ab; // Closest point on the segment
            return (point - closestPoint).magnitude; // Distance to the segment
        }

        public static float DistanceToLevelPlaneFilledBelow(Vector3 point, Vector3 pointA, Vector3 pointB)
        {
            // Direction vector of the line AB
            Vector3 ab = pointB - pointA;
            float abLength = Mathf.Sqrt(ab.x * ab.x + ab.z * ab.z); // Only XZ length

            // Projection of point onto the line AB (XZ-plane only)
            float t = ((point.x - pointA.x) * (pointB.x - pointA.x) + (point.z - pointA.z) * (pointB.z - pointA.z)) / (abLength * abLength);

            // Clamp t to the line segment [0, 1]
            t = Mathf.Clamp01(t);

            // Interpolated height at the projected point
            float heightAtPoint = Mathf.Lerp(pointA.y, pointB.y, t);

            // Signed distance to the plane
            return point.y - heightAtPoint;
        }
    }

    public static class CombinationFunctionsOutsideIsPositive
    {
        /*
        Boolean logic:
            A     ->  A
            !A    ->  -A
            A & B ->  Max(A,B)
            A | B ->  Min(A,B)
        */

        public static float Add(float a, float b) // Same as (A | B)
        {
            return Mathf.Min(a, b);
        }

        public static float Subtract(float a, float b) // Same as (A & !B)
        {
            return Mathf.Max(a, -b);
        }

        public static float Intersect(float a, float b) // Same as (A & B)
        {
            return Mathf.Max(a, b);
        }
    }
}
