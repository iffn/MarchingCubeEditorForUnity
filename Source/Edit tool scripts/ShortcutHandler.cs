using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
public abstract class ShortcutHandler
{
    public abstract string ShortcutText { get; }
    public abstract void HandleShortcut(Event e);

    protected void ForceSceneFocus()
    {
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.Focus(); // Make sure that the scene is the focus
        }
    }
}

public class HandleScaleByHoldingSAndScrolling : ShortcutHandler
{
    readonly Transform referenceTransform;
    KeyCode scaleKey = KeyCode.S;
    bool scaleActive = false;

    public HandleScaleByHoldingSAndScrolling(Transform referenceTransform)
    {
        this.referenceTransform = referenceTransform;
    }

    public override string ShortcutText { get { return $"Hold {scaleKey} and scroll to change the size"; } }

    public override void HandleShortcut(Event e)
    {
        ForceSceneFocus();

        if (e.keyCode == scaleKey)
        {
            if (e.type == EventType.KeyDown) scaleActive = true;
            else if (e.type == EventType.KeyUp) scaleActive = false;
        }

        if (scaleActive && e.type == EventType.ScrollWheel)
        {
            float scaleDelta = e.delta.y * 0.03f; // Scale factor; reverse direction if needed

            referenceTransform.localScale *= (scaleDelta + 1);

            e.Use(); // Mark event as handled
        }
    }
}

public class HandleHorizontalScaleByHoldingSAndScrolling : ShortcutHandler
{
    readonly Transform referenceTransform;
    KeyCode scaleKey = KeyCode.S;
    bool scaleActive = false;

    public HandleHorizontalScaleByHoldingSAndScrolling(Transform referenceTransform)
    {
        this.referenceTransform = referenceTransform;
    }

    public override string ShortcutText { get { return $"Hold {scaleKey} and scroll to change the size"; } }

    public override void HandleShortcut(Event e)
    {
        ForceSceneFocus();

        if (e.keyCode == scaleKey)
        {
            if (e.type == EventType.KeyDown) scaleActive = true;
            else if (e.type == EventType.KeyUp) scaleActive = false;
        }

        if (scaleActive && e.type == EventType.ScrollWheel)
        {
            Debug.Log($"Scaling with factor {e.delta}");

            float scaleDelta = e.delta.x * 0.03f; // Scale factor; reverse direction if needed

            float scaleFactor = scaleDelta + 1;

            referenceTransform.localScale = new Vector3(
                referenceTransform.localScale.x * scaleFactor,
                referenceTransform.localScale.y,
                referenceTransform.localScale.z * scaleFactor
                );

            e.Use(); // Mark event as handled
        }
    }
}
#endif