using UnityEngine;
using iffnsStuff.MarchingCubeEditor.Core;

public class BaseModificationTools
{
    public interface IVoxelModifier
    {
        VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distance);
    }

    public class AddShapeModifier : IVoxelModifier
    {
        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distance)
        {
            return currentValue.With(Mathf.Max(currentValue.Weight, -distance));
        }
    }

    public class SubtractShapeModifier : IVoxelModifier
    {
        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distance)
        {
            return currentValue.With(Mathf.Min(currentValue.Weight, distance));
        }
    }

    public class AddShapeWithMaxHeightModifier : AddShapeModifier
    {
        private readonly int maxHeight;

        public AddShapeWithMaxHeightModifier(int maxHeight)
        {
            this.maxHeight = maxHeight;
        }

        public override VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distance)
        {
            // Only modify voxels below or at the maximum height
            if (y > maxHeight)
                return currentValue;

            return base.ModifyVoxel(x, y, z, currentValue, distance);
        }
    }
}
