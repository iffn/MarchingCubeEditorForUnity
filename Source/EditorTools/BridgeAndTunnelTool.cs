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
    bool previewingTunnel = false;

    public override string DisplayName => "Bridge and tunnel tool";

    BridgeOrTunnelShape bridgeOrTunnelShape;

    // Editor properties
    bool showPreviewBeforeApplying = true;
    bool confirmToApply = true;

    public override void OnEnable()
    {
        base.OnEnable();

        if(bridgeOrTunnelShape == null)
        {
            List<EditShape> shapes = LinkedMarchingCubeController.ShapeList;

            foreach(EditShape shape in shapes)
            {
                if(shape is BridgeOrTunnelShape bridgeShape)
                {
                    bridgeOrTunnelShape = bridgeShape;
                    break;
                }
            }
        }
    }

    public override void HandleSceneUpdate(Event e)
    {
        if (startPointSet)
        {
            RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

            endPointSet = (result != RayHitResult.None);

            if (endPointSet)
            {
                endPoint = result.point;

                if (showPreviewBeforeApplying)
                {
                    if (ControlIsHeld(e))
                        PreviewTunnel(startPoint, endPoint);
                    else
                        PreviewBridge(startPoint, endPoint);
                }
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
            if (!confirmToApply)
            {
                if (showPreviewBeforeApplying)
                {
                    ApplyPreviewChanges();
                }
                else
                {
                    if (ControlIsHeld(e))
                    {
                        CreateTunnel(startPoint, endPoint);
                    }
                    else
                    {
                        CreateBridge(startPoint, endPoint);
                    }
                }
                
            }

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
        }

        showPreviewBeforeApplying = EditorGUILayout.Toggle("Show preview before applying", showPreviewBeforeApplying);
        confirmToApply = EditorGUILayout.Toggle("Confirm to apply", confirmToApply);

        if(confirmToApply && startPointSet && endPointSet)
        {
            if (showPreviewBeforeApplying)
            {
                if (previewingTunnel)
                {
                    if (GUILayout.Button($"Switch to bridge"))
                    {
                        PreviewBridge(startPoint, endPoint);
                        previewingTunnel = false;
                    }

                    if (GUILayout.Button($"Apply tunnel"))
                        ApplyPreviewChanges();
                }
                else
                {
                    if (GUILayout.Button($"Apply bridge"))
                        ApplyPreviewChanges();

                    if (GUILayout.Button($"Switch to tunnel"))
                    {
                        PreviewTunnel(startPoint, endPoint);
                        previewingTunnel = true;
                    }
                }
            }
            else
            {
                if (GUILayout.Button($"Create bridge"))
                    CreateBridge(startPoint, endPoint);
                if (GUILayout.Button($"Create tunnel"))
                    CreateTunnel(startPoint, endPoint);
            }
        }
    }

    public override void DrawGizmos()
    {
        if(startPointSet && endPointSet)
        {
            Gizmos.DrawLine(startPoint, endPoint);
        }
    }

    void PrepareBridge(Vector3 startPoint, Vector3 endPoint)
    {
        bridgeOrTunnelShape.transform.position = LinkedMarchingCubeController.transform.position;

        bridgeOrTunnelShape.SetParameters(startPoint, endPoint, BridgeOrTunnelShape.shapeTypes.flatTop);
    }

    void PrepareTunnel(Vector3 startPoint, Vector3 endPoint)
    {
        bridgeOrTunnelShape.transform.position = LinkedMarchingCubeController.transform.position;

        bridgeOrTunnelShape.SetParameters(startPoint, endPoint, BridgeOrTunnelShape.shapeTypes.flatBottom);
    }

    void PreviewBridge(Vector3 startPoint, Vector3 endPoint)
    {
        PrepareBridge(startPoint, endPoint);

        LinkedMarchingCubeController.ModificationManager.SetPreviewDisplayState(MarchingCubesPreview.PreviewDisplayStates.addition);
        LinkedMarchingCubeController.ModificationManager.ShowPreviewData(bridgeOrTunnelShape, new BaseModificationTools.AddShapeModifier());
        
        previewingTunnel = false;
    }

    void PreviewTunnel(Vector3 startPoint, Vector3 endPoint)
    {
        PrepareTunnel(startPoint, endPoint);

        LinkedMarchingCubeController.ModificationManager.SetPreviewDisplayState(MarchingCubesPreview.PreviewDisplayStates.subtraction);
        LinkedMarchingCubeController.ModificationManager.ShowPreviewData(bridgeOrTunnelShape, new BaseModificationTools.SubtractShapeModifier());

        previewingTunnel = true;
    }

    void ApplyPreviewChanges()
    {
        LinkedMarchingCubeController.ApplyPreviewChanges();
    }

    void CreateBridge(Vector3 startPoint, Vector3 endPoint)
    {
        PrepareBridge(startPoint, endPoint);

        LinkedMarchingCubeController.ModificationManager.ModifyData(bridgeOrTunnelShape, new BaseModificationTools.AddShapeModifier());
    }

    void CreateTunnel(Vector3 startPoint, Vector3 endPoint)
    {
        PrepareTunnel(startPoint, endPoint);

        LinkedMarchingCubeController.ModificationManager.ModifyData(bridgeOrTunnelShape, new BaseModificationTools.SubtractShapeModifier());
    }
}
