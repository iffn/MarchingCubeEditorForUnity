using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BridgeAndTunnelTool : BaseTool
{
    Vector3 startPoint;
    Vector3 endPoint;
    float radius = 1;

    bool startPointSet = false;
    bool endPointSet = false;

    public override string DisplayName => "Bridge and tunnel tool";

    BridgeOrTunnelLogic bridgeOrTunnelLogic;

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
        else if (!endPointSet)
        {
            CreateBridge(startPoint, endPoint);

            startPoint = endPoint;
            e.Use();
        }
    }

    public override void DrawUI()
    {
        base.DrawUI();

        bridgeOrTunnelLogic = EditorGUILayout.ObjectField(
            bridgeOrTunnelLogic,
            typeof(BridgeOrTunnelLogic),
            true) as BridgeOrTunnelLogic;

        GUILayout.Label($"Start point : {startPoint}");
        GUILayout.Label($"End point : {endPoint}");
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
        if (bridgeOrTunnelLogic == null) return;

        bridgeOrTunnelLogic.StartPoint = startPoint;
        bridgeOrTunnelLogic.StartPoint = endPoint;

        LinkedMarchingCubeController.ModificationManager.ModifyData(bridgeOrTunnelLogic, new BaseModificationTools.AddShapeModifier());
    }
}
