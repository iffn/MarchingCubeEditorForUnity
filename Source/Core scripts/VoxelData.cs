using System;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    public struct VoxelData
    {
        public readonly static VoxelData Empty = new(-1.0f, new Color32(255, 255, 255, 255));
        public readonly static int Size = 4 + 32;

        public VoxelData(float weight, Color32 color)
        {
            Weight = weight;
            Color = color;
        }

        public float Weight { get; private set; }
        public Color32 Color { get; private set; }

        public override readonly string ToString() 
            => $"(w: {Weight}, (r: {Color.r}, g: {Color.g}, b: {Color.b}, a: {Color.a}))";

        public readonly VoxelData With(float weight) => new(weight, Color);
        public readonly VoxelData With(Color32 color) => new(Weight, color);

        public readonly void Serialize(Array dst, int dstOffset)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(Weight),  0, dst, dstOffset + 0,  4);
            Buffer.BlockCopy(BitConverter.GetBytes(Color.r), 0, dst, dstOffset + 4,  4);
            Buffer.BlockCopy(BitConverter.GetBytes(Color.g), 0, dst, dstOffset + 8,  4);
            Buffer.BlockCopy(BitConverter.GetBytes(Color.b), 0, dst, dstOffset + 16, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Color.a), 0, dst, dstOffset + 24, 4);
        }

        public void Deserialize(byte[] src, int srcOffset) 
        {
            Weight = BitConverter.ToSingle(src, srcOffset);
            Color = new Color32
            (
                (byte)BitConverter.ToChar(src, srcOffset + 4),
                (byte)BitConverter.ToChar(src, srcOffset + 8),
                (byte)BitConverter.ToChar(src, srcOffset + 16),
                (byte)BitConverter.ToChar(src, srcOffset + 24)
            );
        }
    }
}