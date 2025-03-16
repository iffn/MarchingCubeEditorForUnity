#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BridgeAndTunnelTool : BaseTool
{
    //Vector3 startPointWorld;
    Vector3 startPointWorld;
    Vector3 startPointLocal;
    Vector3 endPointWorld;
    Vector3 endPointLocal;
    bool startPointSet = false;
    bool endPointSet = false;
    bool previewingTunnel = false;

    public override string DisplayName => "Bridge and tunnel tool";

    BridgeOrTunnelShape bridgeOrTunnelShape;

    // Editor properties
    bool showPreviewBeforeApplying = true;
    bool confirmToApply = false;
    bool continueWithEndPoint = true;

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

        LinkedMarchingCubeController.EnableAllColliders = true;
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void HandleSceneUpdate(Event e)
    {
        if (startPointSet)
        {
            if (confirmToApply)
            {
                if (LeftClickDownEvent(e))
                {
                    RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

                    endPointSet = (result != RayHitResult.None);
                    endPointWorld = result.point;
                    endPointLocal = LinkedMarchingCubeController.transform.InverseTransformPoint(endPointWorld);

                    if (showPreviewBeforeApplying)
                    {
                        if (ControlIsHeld(e))
                            PreviewTunnel(startPointLocal, endPointLocal);
                        else
                            PreviewBridge(startPointLocal, endPointLocal);
                    }

                    ShowPreviewCheck();

                    e.Use();
                }
            }
            else
            {
                RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

                endPointSet = (result != RayHitResult.None);
                endPointWorld = result.point;
                endPointLocal = LinkedMarchingCubeController.transform.InverseTransformPoint(endPointWorld);

                if (endPointSet)
                {
                    if (showPreviewBeforeApplying)
                    {
                        if (ControlIsHeld(e))
                            PreviewTunnel(startPointLocal, endPointLocal);
                        else
                            PreviewBridge(startPointLocal, endPointLocal);
                    }

                    if (LeftClickDownEvent(e))
                    {
                        if (showPreviewBeforeApplying)
                        {
                            ApplyPreviewChanges();
                            if (continueWithEndPoint)
                            {
                                startPointWorld = endPointWorld;
                                startPointLocal = endPointLocal;
                            }
                        }
                        else
                        {
                            if (ControlIsHeld(e))
                                CreateTunnel(startPointLocal, endPointLocal);
                            else
                                CreateBridge(startPointLocal, endPointLocal);
                        }

                        e.Use();
                    }
                }

                ShowPreviewCheck();
            }

            if (EscapeDownEvent(e))
            {
                startPointSet = false;
                endPointSet = false;
                if (showPreviewBeforeApplying) LinkedMarchingCubeController.DisplayPreviewShape = false;
            }
        }
        else
        {
            if (LeftClickDownEvent(e)) // Left-click event
            {
                RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

                if (result == RayHitResult.None) return;

                startPointWorld = result.point;
                startPointLocal = LinkedMarchingCubeController.transform.InverseTransformPoint(startPointWorld);
                startPointSet = true;
                e.Use();
            }
        }
    }

    void ShowPreviewCheck()
    {
        bool showPreview = showPreviewBeforeApplying && startPointSet && endPointSet;

        LinkedMarchingCubeController.DisplayPreviewShape = showPreview;
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
            float newRadius = EditorGUILayout.FloatField("Radius:", bridgeOrTunnelShape.radius);

            if(newRadius != bridgeOrTunnelShape.radius)
            {
                bridgeOrTunnelShape.radius = newRadius;

                if(showPreviewBeforeApplying && startPointSet && endPointSet)
                {
                    Debug.Log("Update");

                    if(previewingTunnel)
                        PreviewTunnel(startPointLocal, endPointLocal);
                    else
                        PreviewBridge(startPointLocal, endPointLocal);
                }
            }
        }

        bool newShowPreviewBeforeApplying = EditorGUILayout.Toggle("Show preview before applying", showPreviewBeforeApplying);
        if(newShowPreviewBeforeApplying != showPreviewBeforeApplying)
        {
            showPreviewBeforeApplying = newShowPreviewBeforeApplying;

            ShowPreviewCheck();
        }
        
        confirmToApply = EditorGUILayout.Toggle("Confirm to apply", confirmToApply);

        continueWithEndPoint = EditorGUILayout.Toggle("Continue with end point", continueWithEndPoint);

        if (confirmToApply && startPointSet && endPointSet)
        {
            if (showPreviewBeforeApplying)
            {
                if (previewingTunnel)
                {
                    if (GUILayout.Button($"Switch to bridge"))
                    {
                        PreviewBridge(startPointLocal, endPointLocal);
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
                        PreviewTunnel(startPointLocal, endPointLocal);
                        previewingTunnel = true;
                    }
                }
            }
            else
            {
                if (GUILayout.Button($"Create bridge"))
                    CreateBridge(startPointLocal, endPointLocal);
                if (GUILayout.Button($"Create tunnel"))
                    CreateTunnel(startPointLocal, endPointLocal);
            }
        }
    }

    public override void DrawGizmos()
    {
        if(startPointSet && endPointSet)
        {
            Gizmos.DrawLine(startPointWorld, endPointWorld);

            LinkedMarchingCubeController.VisualisationManager.DrawCircle(endPointWorld, bridgeOrTunnelShape.radius, 12, startPointWorld - endPointWorld);
        }

        if (startPointSet)
        {
            if (endPointSet)
                LinkedMarchingCubeController.VisualisationManager.DrawCircle(startPointWorld, bridgeOrTunnelShape.radius, 12, startPointWorld - endPointWorld);
            else
                LinkedMarchingCubeController.VisualisationManager.DrawCircle(startPointWorld, bridgeOrTunnelShape.radius, 12, Vector3.up);
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
        if ((startPoint - endPoint).magnitude == 0) return;

        PrepareBridge(startPoint, endPoint);

        LinkedMarchingCubeController.ModificationManager.SetPreviewDisplayState(MarchingCubesPreview.PreviewDisplayStates.addition);
        LinkedMarchingCubeController.ModificationManager.ShowPreviewData(bridgeOrTunnelShape, new BaseModificationTools.AddShapeModifier());
        
        previewingTunnel = false;
    }

    void PreviewTunnel(Vector3 startPoint, Vector3 endPoint)
    {
        if ((startPoint - endPoint).magnitude == 0) return;
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

#endif