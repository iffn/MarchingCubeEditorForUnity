#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtilityFunctions;

public struct PostProcessingOptions
{
    public bool postProcessWhileEditing;
    public bool createOneChunk;
    public bool smoothNormals;
    public float smoothNormalsDistanceFactorBias;
    public bool mergeTriangles;
    public float angleThresholdDeg;
    public float areaThreshold;
    public double maxProcessingTimeSeconds;

    public static List<FieldMetadata> FieldDefinitions = new List<FieldMetadata>
    {
        new FieldMetadata
        {
            Name = "Post Process While Editing",
            FieldType = typeof(bool),
            GetValue = opts => opts.postProcessWhileEditing,
            SetValue = (opts, val) =>
            {
                opts.postProcessWhileEditing = (bool)val;
                return opts; // Return the updated struct
            },
            IsVisible = opts => true // Always visible
        },
        new FieldMetadata
        {
            Name = "Create One Chunk",
            FieldType = typeof(bool),
            GetValue = opts => opts.createOneChunk,
            SetValue = (opts, val) =>
            {
                opts.createOneChunk = (bool)val;
                return opts; // Return the updated struct
            },
            IsVisible = opts => true // Always visible
        },
        new FieldMetadata
        {
            Name = "Smooth Normals",
            FieldType = typeof(bool),
            GetValue = opts => opts.smoothNormals,
            SetValue = (opts, val) =>
            {
                opts.smoothNormals = (bool)val;
                return opts; // Return the updated struct
            },
            IsVisible = opts => true // Always visible
        },
        new FieldMetadata
        {
            Name = "Distance factor bias",
            FieldType = typeof(float),
            GetValue = opts => opts.smoothNormalsDistanceFactorBias,
            SetValue = (opts, val) =>
            {
                opts.smoothNormalsDistanceFactorBias = (float)val;
                return opts; // Return the updated struct
            },
            IsVisible = opts => opts.smoothNormals // Visible only if mergeTriangles is true
        },
        new FieldMetadata
        {
            Name = "Merge Triangles",
            FieldType = typeof(bool),
            GetValue = opts => opts.mergeTriangles,
            SetValue = (opts, val) =>
            {
                opts.mergeTriangles = (bool)val;
                return opts; // Return the updated struct
            },
            IsVisible = opts => true // Always visible
        },
        new FieldMetadata
        {
            Name = "Angle Threshold (Deg)",
            FieldType = typeof(float),
            GetValue = opts => opts.angleThresholdDeg,
            SetValue = (opts, val) =>
            {
                opts.angleThresholdDeg = (float)val;
                return opts; // Return the updated struct
            },
            IsVisible = opts => opts.mergeTriangles // Visible only if mergeTriangles is true
        },
        new FieldMetadata
        {
            Name = "Area Threshold",
            FieldType = typeof(float),
            GetValue = opts => opts.areaThreshold,
            SetValue = (opts, val) =>
            {
                opts.areaThreshold = (float)val;
                return opts; // Return the updated struct
            },
            IsVisible = opts => opts.mergeTriangles // Visible only if mergeTriangles is true
        },
        new FieldMetadata
        {
            Name = "Max Processing Time [s]",
            FieldType = typeof(double),
            GetValue = opts => opts.maxProcessingTimeSeconds,
            SetValue = (opts, val) =>
            {
                opts.maxProcessingTimeSeconds = (double)val;
                return opts; // Return the updated struct
            },
            IsVisible = opts => true // Always visible
        }
    };

    public static PostProcessingOptions Default => new PostProcessingOptions
    {
        postProcessWhileEditing = false,
        createOneChunk = true,
        smoothNormals = false,
        smoothNormalsDistanceFactorBias = 2f,
        mergeTriangles = false,
        angleThresholdDeg = 5f,
        areaThreshold = 0.01f,
        maxProcessingTimeSeconds = 10.0
    };
}
#endif