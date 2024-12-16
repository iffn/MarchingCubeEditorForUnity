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

        public VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distance)
        {
            Color32 newColor = Color.Lerp(color, currentValue.Color, curve.Evaluate(distance));

            return currentValue.With(newColor);
        }
    }
}
