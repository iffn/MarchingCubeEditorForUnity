#if UNITY_EDITOR
using UnityEngine;
using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections.Generic;

public class BaseModificationTools
{
    public interface IVoxelModifier
    {
        VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distanceOutsideIsPositive);
    }

    public class CopyModifier : IVoxelModifier
    {
        VoxelData[,,] currentData;
        private Matrix4x4 originalTransformWTL;
        private Matrix4x4 newTransformWTL;
        private Matrix4x4 controllerTransformWTL;

        public CopyModifier(VoxelData[,,] currentData, Matrix4x4 originalTransformWTL, Matrix4x4 newTransformWTL, Matrix4x4 controllerTransformWTL)
        {
            this.currentData = currentData;
            this.originalTransformWTL = originalTransformWTL;
            this.newTransformWTL = newTransformWTL;
            this.controllerTransformWTL = controllerTransformWTL;
        }

        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distanceOutsideIsPositive)
        {
            if (distanceOutsideIsPositive > 0) return currentValue;

            // Convert voxel position to Vector3
            Vector3 originalPosition = new Vector3(x, y, z);
            originalPosition = controllerTransformWTL.inverse.MultiplyPoint3x4(originalPosition);

            // Apply transformation matrix
            Vector3 transformedPosition = TransformBetweenLocalSpaces(originalPosition, originalTransformWTL, newTransformWTL);

            transformedPosition = controllerTransformWTL.MultiplyPoint3x4(transformedPosition);

            // Get the integer floor and ceiling of the transformed position
            int x0 = Mathf.FloorToInt(transformedPosition.x);
            int x1 = Mathf.CeilToInt(transformedPosition.x);
            int y0 = Mathf.FloorToInt(transformedPosition.y);
            int y1 = Mathf.CeilToInt(transformedPosition.y);
            int z0 = Mathf.FloorToInt(transformedPosition.z);
            int z1 = Mathf.CeilToInt(transformedPosition.z);

            // Clamp indices to stay within bounds
            x0 = Mathf.Clamp(x0, 0, currentData.GetLength(0) - 1);
            x1 = Mathf.Clamp(x1, 0, currentData.GetLength(0) - 1);
            y0 = Mathf.Clamp(y0, 0, currentData.GetLength(1) - 1);
            y1 = Mathf.Clamp(y1, 0, currentData.GetLength(1) - 1);
            z0 = Mathf.Clamp(z0, 0, currentData.GetLength(2) - 1);
            z1 = Mathf.Clamp(z1, 0, currentData.GetLength(2) - 1);

            // Get fractional parts for interpolation
            float xd = Mathf.Clamp01(transformedPosition.x - x0);
            float yd = Mathf.Clamp01(transformedPosition.y - y0);
            float zd = Mathf.Clamp01(transformedPosition.z - z0);

            // Retrieve voxel values at 8 surrounding points
            float c000 = currentData[x0, y0, z0].WeightInsideIsPositive;
            float c100 = currentData[x1, y0, z0].WeightInsideIsPositive;
            float c010 = currentData[x0, y1, z0].WeightInsideIsPositive;
            float c110 = currentData[x1, y1, z0].WeightInsideIsPositive;
            float c001 = currentData[x0, y0, z1].WeightInsideIsPositive;
            float c101 = currentData[x1, y0, z1].WeightInsideIsPositive;
            float c011 = currentData[x0, y1, z1].WeightInsideIsPositive;
            float c111 = currentData[x1, y1, z1].WeightInsideIsPositive;

            // Perform trilinear interpolation
            float c00 = Mathf.Lerp(c000, c100, xd);
            float c01 = Mathf.Lerp(c001, c101, xd);
            float c10 = Mathf.Lerp(c010, c110, xd);
            float c11 = Mathf.Lerp(c011, c111, xd);

            float c0 = Mathf.Lerp(c00, c10, yd);
            float c1 = Mathf.Lerp(c01, c11, yd);

            float interpolatedWeight = Mathf.Lerp(c0, c1, zd);

            // Return the interpolated voxel value
            return currentValue.WithWeightInsideIsPositive(interpolatedWeight);
        }

        Vector3 TransformBetweenLocalSpaces(Vector3 worldPosition, Matrix4x4 A_old, Matrix4x4 A_new)
        {
            // Convert world position to local space of A_new
            Vector3 localPositionInAnew = A_new.MultiplyPoint3x4(worldPosition);

            // Convert local position (A_new) back to world position using A_old
            Vector3 transformedWorldPosition = A_old.inverse.MultiplyPoint3x4(localPositionInAnew);

            return transformedWorldPosition;
        }

    }

    public class AddShapeModifier : IVoxelModifier
    {
        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distanceOutsideIsPositive)
        {
            float newDistanceOutsideIsPositive = SDFMath.CombinationFunctionsOutsideIsPositive.Add(currentValue.DistanceOutsideIsPositive, distanceOutsideIsPositive);

            return currentValue.WithDistanceOutsideIsPositive(newDistanceOutsideIsPositive);
        }
    }

    public class SubtractShapeModifier : IVoxelModifier
    {
        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distanceOutsideIsPositive)
        {
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

        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distanceOutsideIsPositive)
        {
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
        VoxelData[,,] currentData;
        float[,,] gaussianKernel;
        readonly float weightThreshold;
        readonly int radius;
        readonly float sigma;

        public GaussianSmoothingModifier(VoxelData[,,] currentData, float weightThreshold, int radius, float sigma)
        {
            this.currentData = currentData;
            this.weightThreshold = weightThreshold;
            this.radius = radius;
            this.sigma = sigma;

            GenerateGaussianKernel(radius, sigma);
        }

        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distanceOutsideIsPositive)
        {
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
        VoxelData[,,] currentData;
        readonly int radius;
        readonly float intensity;
        readonly float frequency;
        readonly float falloffSharpness;
        readonly Vector3 voxelOrigin;
        readonly float voxelSize;

        public WorldSpaceRougheningModifier(
            VoxelData[,,] currentData,
            int radius,
            float intensity,
            float frequency,
            float falloffSharpness,
            Vector3 voxelOrigin,
            float voxelSize)
        {
            this.currentData = currentData;
            this.radius = radius;
            this.intensity = intensity;
            this.frequency = frequency;
            this.falloffSharpness = falloffSharpness;
            this.voxelOrigin = voxelOrigin;
            this.voxelSize = voxelSize;
        }

        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distanceOutsideIsPositive)
        {
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

        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distanceOutsideIsPositive)
        {
            if (distanceOutsideIsPositive > 0) return currentValue;

            Color32 newColor = Color.Lerp(color, currentValue.Color, curve.Evaluate(distanceOutsideIsPositive));

            return currentValue.WithColor(newColor);
        }
    }
}
#endif