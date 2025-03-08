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

    public class CopyModifier : IVoxelModifier
    {
        private Matrix4x4 transformationMatrix;

        public CopyModifier(Matrix4x4 transformationMatrix)
        {
            this.transformationMatrix = transformationMatrix;
        }

        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData[,,] currentData, float distanceOutsideIsPositive)
        {
            VoxelData currentValue = currentData[x, y, z];

            if (distanceOutsideIsPositive > 0) return currentValue;

            // Convert voxel position to Vector3
            Vector3 originalPosition = new Vector3(x, y, z);

            // Apply transformation matrix
            Vector3 transformedPosition = transformationMatrix.MultiplyPoint3x4(originalPosition);

            // Convert back to voxel grid space
            int newX = Mathf.RoundToInt(transformedPosition.x);
            int newY = Mathf.RoundToInt(transformedPosition.y);
            int newZ = Mathf.RoundToInt(transformedPosition.z);

            // Ensure new voxel coordinates are within bounds
            if (newX < 0 || newX >= currentData.GetLength(0)) return currentValue;
            if (newY < 0 || newY >= currentData.GetLength(1)) return currentValue;
            if (newZ < 0 || newZ >= currentData.GetLength(2)) return currentValue;

            return currentData[newX, newY, newZ];
        }
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
        float[,,] gaussianKernel;
        readonly float weightThreshold;
        readonly int radius;
        readonly float sigma;

        public GaussianSmoothingModifier(float weightThreshold, int radius, float sigma)
        {
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

            float newWeight = ApplyKernel(x, y, z, currentData, gaussianKernel, radius);
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
        readonly int radius;
        readonly float intensity;
        readonly float frequency;
        readonly float falloffSharpness;
        readonly Vector3 voxelOrigin;
        readonly float voxelSize;

        public WorldSpaceRougheningModifier(
            int radius,
            float intensity,
            float frequency,
            float falloffSharpness,
            Vector3 voxelOrigin,
            float voxelSize)
        {
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

            int offset = 1;

            int xMin = Mathf.Max(x - offset, 0);
            int xMax = Mathf.Min(x + offset, currentData.GetLength(0) - 1);
            int yMin = Mathf.Max(y - offset, 0);
            int yMax = Mathf.Min(y + offset, currentData.GetLength(1) - 1);
            int zMin = Mathf.Max(z - offset, 0);
            int zMax = Mathf.Min(z + offset, currentData.GetLength(2) - 1);

            for(int nx = xMin; nx <= xMax; nx++)
            {
                for (int ny = yMin; ny <= yMax; ny++)
                {
                    for (int nz = zMin; nz <= zMax; nz++)
                    {
                        if (nx == x && ny == y && nz == z) continue; // Skip the center voxel

                        float neighborWeight = currentData[nx, ny, nz].WeightInsideIsPositive;

                        // Bitwise sign check for performance (avoids branching)
                        if ((currentValue.WeightInsideIsPositive * neighborWeight) < 0)
                        {
                            hasDifferentSign = true;
                            break; // Exit early if a sign difference is found
                        }
                    }
                }
            }

            // Ignore non-bordering voxels
            if (!hasDifferentSign) return currentValue;

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