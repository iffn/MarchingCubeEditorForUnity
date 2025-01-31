#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UIElements;

namespace iffnsStuff.MarchingCubeEditor.EditTools
{
    public class BoxEditShape : EditShape, IPlaceableByClick
    {
        public EditShape AsEditShape => this;

        public override OffsetTypes offsetType => OffsetTypes.towardsNormal;

        protected override float DistanceOutsideIsPositive(Vector3 localPoint)
        {
            return SDFMath.ShapesDistanceOutsideIsPositive.Box(localPoint, Vector3.one);
        }

        public override (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox()
        {
            return (-0.5f * Vector3.one, 0.5f * Vector3.one);
        }
    }
}

#endif