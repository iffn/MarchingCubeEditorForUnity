#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoyRockShape : EditShape, IPlaceableByClick
{
    public EditShape AsEditShape => this;

    public override OffsetTypes offsetType => OffsetTypes.towardsNormal;

    // Parameters for the SDF
    float baseFrequency = 10.0f;
    float baseAmplitude = 0.05f;
    float baseScale = 1.0f;

    float voronoiScale = 1.0f;
    float voronoiStrength = 0.02f;

    Vector3 scale;

    protected override float DistanceOutsideIsPositive(Vector3 localPoint)
    {
        // Get parameters from material
        

        return RockSDF(localPoint);
    }

    public override void PrepareParameters(Transform gridTransform)
    {
        base.PrepareParameters(gridTransform);

        scale = transform.lossyScale;

        Material linkedMaterial = transform.GetComponent<MeshRenderer>().sharedMaterial;
        baseAmplitude = linkedMaterial.GetFloat("_BaseAmplitude");
        baseFrequency = linkedMaterial.GetFloat("_BaseFrequency");
        baseScale = linkedMaterial.GetFloat("_BaseScale");
        voronoiScale = linkedMaterial.GetFloat("_VoronoiScale");
        voronoiStrength = linkedMaterial.GetFloat("_VoronoiStrength");
    }

    public override (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox()
    {
        return (-0.5f * Vector3.one, 0.5f * Vector3.one);
    }

    // Sphere SDF
    private float SphereSDF(Vector3 p, float radius)
    {
        return p.magnitude - radius;
    }

    // 3D Voronoi Function
    private float Voronoi(Vector3 p)
    {
        Vector3 cell = new Vector3(Mathf.Floor(p.x), Mathf.Floor(p.y), Mathf.Floor(p.z));
        float minDistance = 1.0f;

        for (int z = -1; z <= 1; z++) // Loop over neighboring cells
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    Vector3 neighbor = cell + new Vector3(x, y, z);

                    // Generate a random offset for the cell point
                    float hash = Mathf.Sin(Vector3.Dot(neighbor, new Vector3(12.9898f, 78.233f, 37.719f))) * 43758.5453f;
                    Vector3 cellPoint = neighbor + new Vector3(
                        ShaderFrac(hash * 0.3f),
                        ShaderFrac(hash * 0.5f),
                        ShaderFrac(hash * 0.7f)
                    );

                    // Compute the distance to the current cell point
                    float dist = (p - cellPoint).magnitude;
                    minDistance = Mathf.Min(minDistance, dist);
                }
            }
        }

        return minDistance;
    }

    static float ShaderFrac(float x)
    {
        return x%1;
    }

    // Combined Noise Function
    private float CombinedNoise(Vector3 localPos)
    {
        float returnValue = 0.0f;

        // Base noise layer
        Vector3 basePosition = localPos * baseFrequency * baseScale;
        returnValue += Mathf.Sin(basePosition.x) * Mathf.Sin(basePosition.y) * Mathf.Sin(basePosition.z) * baseAmplitude;

        // Add Voronoi cracks
        returnValue -= Voronoi(localPos * voronoiScale) * voronoiStrength;

        return returnValue;
    }

    // Rock SDF Function
    public float RockSDF(Vector3 localPos)
    {
        // Normalize local position by scale ratios
        Vector3 normalizedPos = Vector3.Scale(localPos, scale / (scale.x + scale.y + scale.z) * 3.0f);

        // Base sphere shape
        float sphereBase = SphereSDF(localPos, 0.4f);

        // Add noise to normalized position
        float noise = CombinedNoise(normalizedPos);

        // Combine base shape and noise
        float rock = sphereBase + noise;

        return rock;
    }
}

#endif