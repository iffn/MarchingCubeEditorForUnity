#if UNITY_EDITOR
using UnityEngine;
using iffnsStuff.MarchingCubeEditor.Core;
using static BaseModificationTools.ModifyShapeWithMaxHeightModifier;

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

        public virtual VoxelData ModifyVoxel(int x, int y, int z, VoxelData currentValue, float distance)
        {
            distance = Mathf.Max(distance, y - maxHeight);

            switch (booleanType)
            {
                case BooleanType.AddOnly:
                    return currentValue.With(Mathf.Max(currentValue.Weight, -distance));
                case BooleanType.SubtractOnly:
                    return currentValue.With(Mathf.Min(currentValue.Weight, distance));
                case BooleanType.AddAndSubtract:
                    return currentValue.With(-distance);
            }

            return currentValue;
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
#endif