using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DesktopEditor : PlaymodeEditor
{
    [SerializeField] MarchingCubesController linkedMarchingCubeController;
    protected override MarchingCubesController LinkedMarchingCubeController => linkedMarchingCubeController;
    [SerializeField] float scaleSpeed = 1f;

    //Unity functions
    void Start()
    {
        InitializeController();
    }

    void Update()
    {
        HandleEditing();

        HandleSaving();
    }


    void HandleEditing()
    {
        RayHitResult result = RaycastToCenter(true);

        if (result != RayHitResult.None)
        {
            placeableByClick.gameObject.SetActive(true);
            placeableByClick.transform.position = result.point;

            if (Input.GetMouseButtonDown(0))
            {
                BaseModificationTools.IVoxelModifier modifier = new BaseModificationTools.AddShapeModifier();
                linkedMarchingCubeController.ModificationManager.ModifyData(placeableByClick, modifier);
            }
            if (Input.GetMouseButtonDown(1))
            {
                BaseModificationTools.IVoxelModifier modifier = new BaseModificationTools.SubtractShapeModifier();
                linkedMarchingCubeController.ModificationManager.ModifyData(placeableByClick, modifier);
            }

            float scaleAxis = Input.GetAxis("Mouse ScrollWheel");

            placeableByClick.transform.localScale *= (1 - scaleAxis * scaleSpeed);
        }
        else
        {
            placeableByClick.gameObject.SetActive(false);
        }
    }

    void HandleSaving()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
        {
            SaveData();
        }
    }

    //Internal functions
    void ApplyModification(Vector3 position)
    {
        placeableByClick.transform.position = position;

        BaseModificationTools.IVoxelModifier modifier = new BaseModificationTools.AddShapeModifier();

        linkedMarchingCubeController.ModificationManager.ModifyData(placeableByClick, modifier);
    }

    RayHitResult RaycastToCenter(bool detectBoundingBox = true)
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore)) //~0 = layer mask for all layers
            return new RayHitResult(hitInfo.point, hitInfo.normal);

        if (!detectBoundingBox)
            return RayHitResult.None;

        Vector3 areaPosition = linkedMarchingCubeController.transform.position;
        Vector3Int areaSize = linkedMarchingCubeController.MaxGrid;
        Bounds bounds = new Bounds(areaPosition + areaSize / 2, areaSize);

        (Vector3, Vector3)? result = bounds.GetIntersectRayPoints(ray);
        if (result != null)
            return new RayHitResult(result.Value.Item2, bounds.GetNormalToSurface(result.Value.Item2));

        // Both normal Raycast and Bounds intersection did not succeed 
        return RayHitResult.None;
    }
}
