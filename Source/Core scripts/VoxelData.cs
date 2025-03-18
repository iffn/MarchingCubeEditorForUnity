#if UNITY_EDITOR

using System;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    public struct VoxelData
    {
        public readonly static VoxelData Empty = new VoxelData(-1.0f, new Color32(255, 255, 255, 255));
        public readonly static int Size = 4 + 32;

        public VoxelData(float weight, Color32 color)
        {
            WeightInsideIsPositive = Mathf.Clamp(weight, -1f, 1f);
            Color = color;
        }

        public float WeightInsideIsPositive { get; private set; }
        public float DistanceOutsideIsPositive => -WeightInsideIsPositive;

        public Color32 Color { get; private set; }

        public override string ToString() 
            => $"(w: {WeightInsideIsPositive}, (r: {Color.r}, g: {Color.g}, b: {Color.b}, a: {Color.a}))";

        public VoxelData WithWeightInsideIsPositive(float weightInsideIsPositive) => new VoxelData(weightInsideIsPositive, Color);
        public VoxelData WithDistanceOutsideIsPositive(float distanceOutsideIsPositive) => new VoxelData(-distanceOutsideIsPositive, Color);
        public VoxelData WithColor(Color32 color) => new VoxelData(WeightInsideIsPositive, color);

        public void Serialize(byte[] dst, int dstOffset)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(WeightInsideIsPositive), 0, dst, dstOffset,  4);
            dst[dstOffset + 4] = Color.r;
            dst[dstOffset + 5] = Color.g;
            dst[dstOffset + 6] = Color.b;
            dst[dstOffset + 7] = Color.a;
        }

        public void Deserialize(byte[] src, int srcOffset) 
        {
            WeightInsideIsPositive = BitConverter.ToSingle(src, srcOffset);
            Color = new Color32(src[srcOffset + 4], src[srcOffset + 5], src[srcOffset + 6], src[srcOffset + 7]);
        }
    }
}

#endif