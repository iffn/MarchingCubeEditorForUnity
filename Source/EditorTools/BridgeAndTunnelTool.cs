using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BridgeAndTunnelTool : BaseTool
{
    Vector3 startPoint;
    Vector3 endPoint;

    bool startPointSet = false;
    bool endPointSet = false;

    public override string DisplayName => "Bridge and tunnel tool";

    BridgeOrTunnelShape bridgeOrTunnelShape;

    public override void HandleSceneUpdate(Event e)
    {
        if (startPointSet)
        {
            RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

            endPointSet = (result != RayHitResult.None);

            if (endPointSet)
            {
                endPoint = result.point;
            }

            if (EscapeDownEvent(e))
            {
                startPointSet = false;
            }
        }

        if (LeftClickDownEvent(e)) // Left-click event
        {
            LeftClickAction(e);
        }
    }

    void LeftClickAction(Event e)
    {
        if (!startPointSet)
        {
            RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

            if (result == RayHitResult.None) return;

            startPoint = result.point;
            startPointSet = true;
            e.Use();
        }
        else if (endPointSet)
        {
            CreateBridge(startPoint, endPoint);

            startPoint = endPoint;
            e.Use();
        }
    }

    public override void DrawUI()
    {
        base.DrawUI();

        bridgeOrTunnelShape = EditorGUILayout.ObjectField(
            bridgeOrTunnelShape,
            typeof(BridgeOrTunnelShape),
            true) as BridgeOrTunnelShape;

        if(bridgeOrTunnelShape != null)
        {
            bridgeOrTunnelShape.radius = EditorGUILayout.FloatField("Radius:", bridgeOrTunnelShape.radius);

            GUILayout.Label($"Start point : {bridgeOrTunnelShape.StartPoint}");
            GUILayout.Label($"End point : {bridgeOrTunnelShape.EndPoint}");
        }
    }

    public override void DrawGizmos()
    {
        if(startPointSet && endPointSet)
        {
            Gizmos.DrawLine(startPoint, endPoint);
        }
    }

    void CreateBridge(Vector3 startPoint, Vector3 endPoint)
    {
        if (bridgeOrTunnelShape == null) return;

        bridgeOrTunnelShape.transform.position = LinkedMarchingCubeController.transform.position;

        bridgeOrTunnelShape.StartPoint = startPoint;
        bridgeOrTunnelShape.EndPoint = endPoint;

        LinkedMarchingCubeController.ModificationManager.ModifyData(bridgeOrTunnelShape, new BaseModificationTools.AddShapeModifier());
    }
}
