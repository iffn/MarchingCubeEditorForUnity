using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseModificationTools
{
    public interface IVoxelModifier
    {
        float ModifyVoxel(int x, int y, int z, float currentValue, float distance);
    }

    public class AddShapeModifier : IVoxelModifier
    {
        public float ModifyVoxel(int x, int y, int z, float currentValue, float distance)
        {
            return Mathf.Max(currentValue, -distance);
        }
    }

    public class SubtractShapeModifier : IVoxelModifier
    {
        public float ModifyVoxel(int x, int y, int z, float currentValue, float distance)
        {
            return Mathf.Min(currentValue, distance);
        }
    }

    public class AddShapeWithMaxHeightModifier : IVoxelModifier
    {
        private int maxHeight;

        public AddShapeWithMaxHeightModifier(int maxHeight)
        {
            this.maxHeight = maxHeight;
        }

        public float ModifyVoxel(int x, int y, int z, float currentValue, float distance)
        {
            // Only modify voxels below or at the maximum height
            if (y > maxHeight)
                return currentValue;

            return Mathf.Max(currentValue, -distance);
        }
    }
}
