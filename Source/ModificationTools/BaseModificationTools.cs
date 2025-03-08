#if UNITY_EDITOR
using UnityEngine;
using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections.Generic;

public class BaseModificationTools
{
    public interface IVoxelModifier
    {
        VoxelData ModifyVoxel(int x, int y, int z, VoxelData[,,] currentData, float distanceOutsideIsPositive);
    }

    public class AddShapeModifier : IVoxelModifier
    {
        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData[,,] currentData, float distanceOutsideIsPositive)
        {
            VoxelData currentValue = currentData[x, y, z];

            //return currentValue.WithWeightInsideIsPositive(Mathf.Max(currentValue.WeightInsideIsPositive, -distanceOutsideIsPositive));

            float newDistanceOutsideIsPositive = SDFMath.CombinationFunctionsOutsideIsPositive.Add(currentValue.DistanceOutsideIsPositive, distanceOutsideIsPositive);

            return currentValue.WithDistanceOutsideIsPositive(newDistanceOutsideIsPositive);
        }
    }

    public class SubtractShapeModifier : IVoxelModifier
    {
        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData[,,] currentData, float distanceOutsideIsPositive)
        {
            VoxelData currentValue = currentData[x, y, z];

            float newDistanceOutsideIsPositive = SDFMath.CombinationFunctionsOutsideIsPositive.Subtract(currentValue.DistanceOutsideIsPositive, distanceOutsideIsPositive);

            return currentValue.WithDistanceOutsideIsPositive(newDistanceOutsideIsPositive);
        }
    }

    public class ModifyShapeWithMaxHeightModifier : IVoxelModifier
    {
        private readonly float maxHeight;

        BooleanType booleanType;

        public enum BooleanType
        {
            AddOnly,
            SubtractOnly,
            AddAndSubtract
        }

        public ModifyShapeWithMaxHeightModifier(float maxHeight, BooleanType booleanType)
        {
            this.maxHeight = maxHeight;
            this.booleanType = booleanType;
        }

        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData[,,] currentData, float distanceOutsideIsPositive)
        {
            VoxelData currentValue = currentData[x, y, z];

            Vector3 samplePoint = new Vector3(x, y, z); //ToDo: Implement position, rotation and scale

            float currentDistance = currentValue.WeightInsideIsPositive;

            float newDistance;

            switch (booleanType)
            {
                case BooleanType.AddOnly:
                    newDistance = AddOnly(distanceOutsideIsPositive);
                    break;
                case BooleanType.SubtractOnly:
                    newDistance = SubtractOnly(distanceOutsideIsPositive);
                    break;
                case BooleanType.AddAndSubtract:
                    newDistance = AddOnly(distanceOutsideIsPositive);
                    newDistance = SubtractOnly(newDistance);
                    break;
                default:
                    newDistance = currentDistance;
                    break;
            }

            float AddOnly(float distanceToShape)
            {
                float floorDistance = SDFMath.ShapesDistanceOutsideIsPositive.PlaneFloor(samplePoint, maxHeight);

                distanceToShape = SDFMath.CombinationFunctionsOutsideIsPositive.Intersect(distanceToShape, floorDistance);

                return SDFMath.CombinationFunctionsOutsideIsPositive.Add(currentValue.DistanceOutsideIsPositive, distanceToShape);
            }

            float SubtractOnly(float distanceToShape)
            {
                float floorDistance = SDFMath.ShapesDistanceOutsideIsPositive.PlaneCeiling(samplePoint, maxHeight);

                distanceToShape = SDFMath.CombinationFunctionsOutsideIsPositive.Intersect(distanceToShape, floorDistance);

                return SDFMath.CombinationFunctionsOutsideIsPositive.Subtract(currentValue.DistanceOutsideIsPositive, distanceToShape);
            }

            return currentValue.WithDistanceOutsideIsPositive(newDistance);
        }
    }

    public class GaussianSmoothingModifier : IVoxelModifier
    {
        readonly VoxelData[,,] voxelData;
        float[,,] gaussianKernel;
        readonly float weightThreshold;
        readonly int radius;
        readonly float sigma;

        public GaussianSmoothingModifier(VoxelData[,,] voxelData, float weightThreshold, int radius, float sigma)
        {
            this.voxelData = voxelData;
            this.weightThreshold = weightThreshold;
            this.radius = radius;
            this.sigma = sigma;

            GenerateGaussianKernel(radius, sigma);
        }

        public VoxelData ModifyVoxel(int x, int y, int z, VoxelData[,,] currentData, float distanceOutsideIsPositive)
        {
            VoxelData currentValue = currentData[x, y, z];

            if (distanceOutsideIsPositive > 0) return currentValue;

            if (Mathf.Abs(currentValue.WeightInsideIsPositive - weightThreshold) > sigma)
                return currentValue;

            float newWeight = ApplyKernel(x, y, z, voxelData, gaussianKernel, radius);
            return currentValue.WithWeightInsideIsPositive(newWeight);
        }

        private void GenerateGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            gaussianKernel = new float[size, size, size];
            float sigma2 = 2 * sigma * sigma;
            float normalization = 1f / Mathf.Pow(Mathf.PI * sigma2, 1.5f);

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    for (int z = -radius; z <= radius; z++)
                    {
                        float distance2 = x * x + y * y + z * z;
                        gaussianKernel[x + radius, y + radius, z + radius] = normalization * Mathf.Exp(-distance2 / sigma2);
                    }
                }
            }
        }

        private float ApplyKernel(int x, int y, int z, VoxelData[,,] voxelData, float[,,] kernel, int radius)
        {
            float sum = 0f;
            float weightSum = 0f;

            // Get voxel data dimensions
            int maxX = voxelData.GetLength(0);
            int maxY = voxelData.GetLength(1);
            int maxZ = voxelData.GetLength(2);

            // Adjust loop limits to stay within bounds
            int minI = Mathf.Max(-radius, -x);
            int maxI = Mathf.Min(radius, maxX - x - 1);
            int minJ = Mathf.Max(-radius, -y);
            int maxJ = Mathf.Min(radius, maxY - y - 1);
            int minK = Mathf.Max(-radius, -z);
            int maxK = Mathf.Min(radius, maxZ - z - 1);

            for (int i = minI; i <= maxI; i++)
            {
                for (int j = minJ; j <= maxJ; j++)
                {
                    for (int k = minK; k <= maxK; k++)
                    {
                        int nx = x + i;
                        int ny = y + j;
                        int nz = z + k;

                        float weight = kernel[i + radius, j + radius, k + radius];
                        sum += voxelData[nx, ny, nz].WeightInsideIsPositive * weight;
                        weightSum += weight;
                    }
                }
            }

            return sum / weightSum;
        }
    }

    public class WorldSpaceRougheningModifier : IVoxelModifier
    {
        readonly VoxelData[,,] voxelData;
        readonly float weightThreshold;
        readonly int radius;
        readonly float intensity;
        readonly float frequency;
        readonly float falloffSharpness;
        readonly Vector3 voxelOrigin;
        readonly float voxelSize;

        public WorldSpaceRougheningModifier(
            VoxelData[,,] voxelData,
            float weightThreshold,
            int radius,
            float intensity,
            float frequency,
            float falloffSharpness,
            Vector3 voxelOrigin,
            float voxelSize)
        {
            this.voxelData = voxelData;
            this.weightThreshold = weightThreshold;
            this.radius = radius;
            this.intensity = intensity;
            this.frequency = frequency;
            this.falloffSharpness = falloffSharpness;
            this.voxelOrigin = voxelOrigin;
            this.voxelSize = voxelSize;
        }

        public VoxelData ModifyVoxel(int x, int y, int z, VoxelData[,,] currentData, float distanceOutsideIsPositive)
        {
            VoxelData currentValue = currentData[x, y, z];

            //Only modify data inside the shape
            if (distanceOutsideIsPositive > 0) return currentValue;

            // Check 6-connected neighbors
            bool hasDifferentSign = false;
            int[,] offsets = {
                { 1, 0, 0 }, { -1, 0, 0 }, // X neighbors
                { 0, 1, 0 }, { 0, -1, 0 }, // Y neighbors
                { 0, 0, 1 }, { 0, 0, -1 }  // Z neighbors
            };

            List<float> weights = new List<float>();

            for (int i = 0; i < 6; i++)
            {
                int nx = x + offsets[i, 0];
                int ny = y + offsets[i, 1];
                int nz = z + offsets[i, 2];

                // Ensure the neighbor is within bounds
                if (nx >= 0 && ny >= 0 && nz >= 0 &&
                    nx < currentData.GetLength(0) &&
                    ny < currentData.GetLength(1) &&
                    nz < currentData.GetLength(2))
                {
                    float neighborWeight = currentData[nx, ny, nz].WeightInsideIsPositive;

                    weights.Add(neighborWeight);

                    // Bitwise sign check (faster)
                    if ((currentValue.WeightInsideIsPositive * neighborWeight) < 0)
                    {
                        hasDifferentSign = true;
                        break; // No need to check further
                    }
                }
            }

            //Debug.Log($"{currentValue.WeightInsideIsPositive}, {currentData[x+1, y+1, z+1].WeightInsideIsPositive}, {hasDifferentSign}"); 

            // Ignore non-bordering voxels
            if (!hasDifferentSign) return currentValue;

            string output = "";

            foreach(float weight in weights)
            {
                output += weight + " ";
            }

            // Calculate noise
            Vector3 worldPos = voxelOrigin + new Vector3(x, y, z) * voxelSize;

            // Pseudo-3D Perlin Noise
            float noiseXY = Mathf.PerlinNoise(worldPos.x * frequency, worldPos.y * frequency);
            float noiseYZ = Mathf.PerlinNoise(worldPos.y * frequency, worldPos.z * frequency);
            float noiseXZ = Mathf.PerlinNoise(worldPos.x * frequency, worldPos.z * frequency);
            float noiseValue = (noiseXY + noiseYZ + noiseXZ) / 3f;

            noiseValue = (noiseValue * 2f) - 1f; // Normalize to [-1, 1]

            float distanceToCenter = Vector3.Distance(worldPos, voxelOrigin);
            float normalizedDistance = Mathf.Clamp01(distanceToCenter / radius);
            float falloff = 1f - Mathf.Pow(normalizedDistance, falloffSharpness);

            float addition = noiseValue * intensity;// * falloff;
            float modifiedWeight = currentValue.WeightInsideIsPositive + addition;

            return currentValue.WithWeightInsideIsPositive(modifiedWeight);
        }

    }


    public class ChangeColorModifier : IVoxelModifier
    {
        private readonly Color32 color;
        private readonly AnimationCurve curve;  // Not sure if curve makes sense as there is already a shape that 
                                                // defines the how the painting should look like. So either remove 
                                                // the shape or the curve.

        public ChangeColorModifier(Color32 color, AnimationCurve curve) 
        {
            this.color = color;
            this.curve = curve;
        }

        public VoxelData ModifyVoxel(int x, int y, int z, VoxelData[,,] currentData, float distance)
        {
            VoxelData currentValue = currentData[x, y, z];

            Color32 newColor = Color.Lerp(color, currentValue.Color, curve.Evaluate(distance));

            return currentValue.WithColor(newColor);
        }
    }
}
#endif