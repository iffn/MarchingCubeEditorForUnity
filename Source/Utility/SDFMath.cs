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
