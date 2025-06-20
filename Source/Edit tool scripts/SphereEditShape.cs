#if UNITY_EDITOR

using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.EditTools
{
    public class SphereEditShape : EditShape, IPlaceableByClick
    {
        public EditShape AsEditShape => this;

        public override OffsetTypes offsetType => OffsetTypes.towardsNormal;
        protected override float DistanceOutsideIsPositive(Vector3 localPoint)
        {
            // Transform the point into the shape's local space
            return SDFMath.ShapesDistanceOutsideIsPositive.Sphere(localPoint, 0.5f);
        }

        public override (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox()
        {
            return (-0.5f * Vector3.one, 0.5f * Vector3.one);
        }

        protected override void SetupShortcutHandlers()
        {
            base.SetupShortcutHandlers();
            shortcutHandlers.Add(new HandleScaleByHoldingSAndScrolling(transform));
        }
    }
}

#endif