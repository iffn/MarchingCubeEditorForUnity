using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PlaymodeEditor : MonoBehaviour
{
    [SerializeField] MarchingCubesController LinkedMarchingCubeController;
    [SerializeField] EditShape placeableByClick;

    //Unity functions
    void Start()
    {
        InitializeController();
    }

    void Update()
    {
        RayHitResult result = RaycastToCenter(true);

        if (result != RayHitResult.None)
        {
            placeableByClick.gameObject.SetActive(true);
            placeableByClick.transform.position = result.point;

            if (Input.GetMouseButtonDown(0))
            {
                BaseModificationTools.IVoxelModifier modifier = new BaseModificationTools.AddShapeModifier();
                LinkedMarchingCubeController.ModificationManager.ModifyData(placeableByClick, modifier);
            }
            if (Input.GetMouseButtonDown(1))
            {
                BaseModificationTools.IVoxelModifier modifier = new BaseModificationTools.SubtractShapeModifier();
                LinkedMarchingCubeController.ModificationManager.ModifyData(placeableByClick, modifier);
            }

            float scaleAxis = Input.GetAxis("Mouse ScrollWheel");

            placeableByClick.transform.localScale *= (1 - scaleAxis * 0.03f); 
        }
        else
        {
            placeableByClick.gameObject.SetActive(false);
        }
    }

    //Internal functions
    void ApplyModification(Vector3 position)
    {
        placeableByClick.transform.position = position;

        BaseModificationTools.IVoxelModifier modifier = new BaseModificationTools.AddShapeModifier();

        LinkedMarchingCubeController.ModificationManager.ModifyData(placeableByClick, modifier);
    }

    void InitializeController()
    {
        LinkedMarchingCubeController.Initialize(1, 1, 1, true, false);
        LoadData();
    }

    void LoadData()
    {
        if (LinkedMarchingCubeController.linkedSaveData == null)
            return;

        LinkedMarchingCubeController.SaveAndLoadManager.LoadGridData(LinkedMarchingCubeController.linkedSaveData);
    }

    RayHitResult RaycastToCenter(bool detectBoundingBox = true)
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore)) //~0 = layer mask for all layers
            return new RayHitResult(hitInfo.point, hitInfo.normal);

        if (!detectBoundingBox)
            return RayHitResult.None;

        Vector3 areaPosition = LinkedMarchingCubeController.transform.position;
        Vector3Int areaSize = LinkedMarchingCubeController.MaxGrid;
        Bounds bounds = new Bounds(areaPosition + areaSize / 2, areaSize);

        (Vector3, Vector3)? result = bounds.GetIntersectRayPoints(ray);
        if (result != null)
            return new RayHitResult(result.Value.Item2, bounds.GetNormalToSurface(result.Value.Item2));

        // Both normal Raycast and Bounds intersection did not succeed 
        return RayHitResult.None;
    }
}
