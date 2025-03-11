#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.EditTools
{
    [ExecuteInEditMode]
    public abstract class EditShape : MonoBehaviour
    {
        public enum OffsetTypes
        {
            vertical,
            towardsNormal
        }

        public abstract OffsetTypes offsetType { get; }

        private Matrix4x4 worldToLocalMatrix;

        /// <summary>
        /// Precompute the world-to-local transformation matrix for optimized distance calculations.
        /// </summary>
        public virtual void PrepareParameters(Transform gridTransform)
        {
            worldToLocalMatrix = transform.worldToLocalMatrix * gridTransform.localToWorldMatrix;
        }

        public Vector3 ConcertWorldToOptimizedLocalPoint(Vector3 worldPoint)
        {
            return worldToLocalMatrix.MultiplyPoint3x4(worldPoint);
        }

        /// <summary>
        /// Calculate the distance to the shape surface in world space.
        /// </summary>
        public float OptimizedDistanceOutsideIsPositive(Vector3 worldPoint)
        {
            Vector3 localPoint = ConcertWorldToOptimizedLocalPoint(worldPoint);
            return DistanceOutsideIsPositive(localPoint);
        }

        /// <summary>
        /// Abstract method for distance calculation in local space (protected).
        /// </summary>
        protected abstract float DistanceOutsideIsPositive(Vector3 localPoint);

        /// <summary>
        /// The shape's position in world space.
        /// </summary>
        public Vector3 WorldPosition => transform.position;

        /// <summary>
        /// The shape's scale in local space.
        /// </summary>
        public Vector3 LocalScale => transform.localScale;

        readonly protected List<ShortcutHandler> shortcutHandlers = new List<ShortcutHandler>();

        void OnEnable()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            shortcutHandlers.Clear();
            SetupShortcutHandlers();
            gameObject.SetActive(true);
        }

        protected virtual void SetupShortcutHandlers()
        {
            shortcutHandlers.Add(new HandleScaleByHoldingSAndScrolling(transform));
        }

        public virtual string HelpText
        {
            get
            {
                string helpText = "";

                foreach (ShortcutHandler handler in shortcutHandlers)
                {
                    helpText += "\n• " + handler.ShortcutText;
                }

                return helpText;
            }
        }

        public virtual void DrawUI()
        {
            
        }

        public virtual void HandleSceneUpdate(Event e)
        {
            foreach (ShortcutHandler handler in shortcutHandlers)
            {
                handler.HandleShortcut(e);
            }
        }

        /// <summary>
        /// Defines the bounding box of the shape in local space.
        /// </summary>
        public abstract (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox();

        /// <summary>
        /// Transforms the shape's local bounding box to world space.
        /// </summary>
        public (Vector3 worldMin, Vector3 worldMax) GetWorldBoundingBox()
        {
            (Vector3 localMin, Vector3 localMax) = GetLocalBoundingBox();

            // Define the 8 corners of the local bounding box
            Vector3[] corners = new Vector3[8]
            {
                new Vector3(localMin.x, localMin.y, localMin.z),
                new Vector3(localMax.x, localMin.y, localMin.z),
                new Vector3(localMin.x, localMax.y, localMin.z),
                new Vector3(localMax.x, localMax.y, localMin.z),
                new Vector3(localMin.x, localMin.y, localMax.z),
                new Vector3(localMax.x, localMin.y, localMax.z),
                new Vector3(localMin.x, localMax.y, localMax.z),
                new Vector3(localMax.x, localMax.y, localMax.z)
            };
    
            // Transform the corners to world space
            Vector3 worldMin = Vector3.positiveInfinity;
            Vector3 worldMax = Vector3.negativeInfinity;

            foreach (Vector3 corner in corners)
            {
                Vector3 worldCorner = transform.TransformPoint(corner);
                worldMin = Vector3.Min(worldMin, worldCorner);
                worldMax = Vector3.Max(worldMax, worldCorner);
            }

            return (worldMin, worldMax);
        }
    }

    public interface IPlaceableByClick
    {
        EditShape AsEditShape { get; }
    }

    public class PlaceableByClickHandler
    {
        public IPlaceableByClick SelectedShape { get; private set; }
        public EditShape SelectedEditShape => SelectedShape.AsEditShape;
        public List<IPlaceableByClick> EditShapes { get; } = new List<IPlaceableByClick>();
        string[] EditShapeNames { get; }
        int selectedIndex;

        public PlaceableByClickHandler(MarchingCubesController linkedController)
        {
            List<EditShape> shapes = linkedController.ShapeList;

            EditShapes.Clear();


            foreach (EditShape shape in shapes)
            {
                if (shape is IPlaceableByClick clickableShape)
                {
                    EditShapes.Add(clickableShape);
                    shape.gameObject.SetActive(false);
                }
            }

            EditShapeNames = new string[EditShapes.Count];

            for (int i = 0; i < EditShapes.Count; i++)
            {
                EditShapeNames[i] = EditShapes[i].AsEditShape.transform.name;
            }

            selectedIndex = Math.Clamp(selectedIndex, 0, EditShapes.Count);
            SelectedShape = EditShapes[selectedIndex];
            SelectedEditShape.Initialize();
        }

        public void DrawEditorUI()
        {
            int newSelectedIndex = EditorGUILayout.Popup("Select Option", selectedIndex, EditShapeNames);

            if(newSelectedIndex != selectedIndex)
            {
                SelectedEditShape.gameObject.SetActive(false);
                selectedIndex = newSelectedIndex;
                SelectedShape = EditShapes[selectedIndex];
                SelectedEditShape.Initialize();
            }
        }
    }
}

#endif