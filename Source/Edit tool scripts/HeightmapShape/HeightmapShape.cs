using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class HeightmapShape : EditShape
{
    private float[,] heightmapData; // 2D array to store heightmap values
    private int heightmapWidth;
    private int heightmapHeight;
    float heightScaler = 1;
    [SerializeField] List<Texture2D> heightmapTextures; // ToDo: Get from display material
    [SerializeField] Material heightmapDisplayMaterial; // ToDo: Get from display material
    Texture2D heightmapTexture;
    int currentHeightmapIndex = 0;

    public override void Initialize()
    {
        base.Initialize();

        SelectHeightmapClamped(currentHeightmapIndex);
    }

    public override void PrepareParameters(Transform gridTransform)
    {
        base.PrepareParameters(gridTransform);

        heightScaler = transform.localScale.y;

        if (heightmapTexture != null)
            LoadHeightmapData();
    }

    public override void DrawUI()
    {
        GUILayout.Label($"Current heightmap: {heightmapTextures[currentHeightmapIndex].name}");
        base.DrawUI();
    }

    /// <summary>
    /// Load heightmap data from the provided texture.
    /// </summary>
    private void LoadHeightmapData()
    {
        heightmapWidth = heightmapTexture.width;
        heightmapHeight = heightmapTexture.height;

        heightmapData = new float[heightmapWidth, heightmapHeight];

        float max = -1000;
        float min = 1000;

        for (int x = 0; x < heightmapWidth; x++)
        {
            for (int y = 0; y < heightmapHeight; y++)
            {
                // Read pixel grayscale value and store as height
                heightmapData[x, y] = heightmapTexture.GetPixel(x, y).r;

                if(max < heightmapData[x, y]) max = heightmapData[x, y];
                if(min > heightmapData[x, y]) min = heightmapData[x, y];
            }
        }
    }

    /// <summary>
    /// Override the distance function for the terrain heightmap.
    /// </summary>
    protected override float DistanceOutsideIsPositive(Vector3 localPoint)
    {
        if(localPoint.y  < 0)
            return -localPoint.y; //Don't fill area below terrain

        if (heightmapData == null)
            return -1; // Return large value if heightmap is not loaded

        // Convert localPoint.x and localPoint.z to heightmap coordinates
        int xIndex = Mathf.Clamp(Mathf.FloorToInt((localPoint.x + 0.5f) * heightmapWidth), 0, heightmapWidth - 1);
        int zIndex = Mathf.Clamp(Mathf.FloorToInt((localPoint.z + 0.5f) * heightmapHeight), 0, heightmapHeight - 1);

        // Get the height at this (x, z) position
        float heightAtPoint = heightmapData[xIndex, zIndex];

        if (heightAtPoint == 0) return heightScaler;

        // Compare the heightmap's height to the localPoint's y position
        float distance = localPoint.y - heightAtPoint;

        return distance * heightScaler; // Positive if above the terrain, negative if below
    }

    /// <summary>
    /// Return the bounding box based on heightmap dimensions.
    /// </summary>
    public override (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox()
    {
        // Assuming the heightmap covers a unit box (scale: -0.5 to +0.5 in x and z)
        return (-0.5f * Vector3.one, new Vector3(0.5f, 1.0f, 0.5f));
    }

    protected override void SetupShortcutHandlers()
    {
        shortcutHandlers.Add(new HandleHorizontalScaleByHoldingSAndScrolling(transform));
        shortcutHandlers.Add(new HandleVerticallyScaleByHoldingAAndScrolling(transform));
        shortcutHandlers.Add(new HandleHorizontalRotateByHoldingDAndScrolling(transform));
        shortcutHandlers.Add(new SwitchHeightmapByHoldingRAndScrolling(this));
    }

    public void SelectNextHeightmap(int offset)
    {

        currentHeightmapIndex += offset;

        SelectHeightmapClamped(currentHeightmapIndex);
    }

    void SelectHeightmapClamped(int index)
    {
        currentHeightmapIndex = ((index % heightmapTextures.Count) + heightmapTextures.Count) % heightmapTextures.Count;

        heightmapTexture = heightmapTextures[currentHeightmapIndex];

        heightmapDisplayMaterial.SetTexture("_MainTex", heightmapTexture);
    }

    class SwitchHeightmapByHoldingRAndScrolling : ShortcutHandler
    {
        KeyCode switchKey = KeyCode.R;
        bool scaleActive = false;
        readonly HeightmapShape linkedHeightmapShape;

        public SwitchHeightmapByHoldingRAndScrolling(HeightmapShape linkedHeightmapShape)
        {
            this.linkedHeightmapShape = linkedHeightmapShape;
        }

        public override string ShortcutText { get { return $"Hold {switchKey} and scroll to change the heightmap"; } }

        public override void HandleShortcut(Event e)
        {
            if (e.keyCode == switchKey)
            {
                if (e.type == EventType.KeyDown) scaleActive = true;
                else if (e.type == EventType.KeyUp) scaleActive = false;
            }

            if (scaleActive && e.type == EventType.ScrollWheel)
            {
                int offset = Mathf.RoundToInt(Mathf.Sign(e.delta.y));

                linkedHeightmapShape.SelectNextHeightmap(offset);

                e.Use(); // Mark event as handled
            }
        }
    }
}
