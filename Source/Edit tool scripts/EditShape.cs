#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.EditTools
{
    [ExecuteInEditMode]
    public abstract class EditShape : MonoBehaviour
    {
        private Matrix4x4 worldToLocalMatrix;

        /// <summary>
        /// Precompute the world-to-local transformation matrix for optimized distance calculations.
        /// </summary>
        public virtual void PrepareParameters(Transform gridTransform)
        {
            worldToLocalMatrix = transform.worldToLocalMatrix * gridTransform.localToWorldMatrix;
        }

        /// <summary>
        /// Calculate the distance to the shape surface in world space.
        /// </summary>
        public float OptimizedDistance(Vector3 worldPoint)
        {
            Vector3 localPoint = worldToLocalMatrix.MultiplyPoint3x4(worldPoint);
            return DistanceOutsideIsPositive(localPoint);
        }

        /// <summary>
        /// Abstract method for distance calculation in local space (protected).
        /// </summary>
        protected abstract float DistanceOutsideIsPositive(Vector3 localPoint);

        /// <summary>
        /// The shape's position in world space.
        /// </summary>
        public Vector3 Position => transform.position;

        /// <summary>
        /// The shape's scale in local space.
        /// </summary>
        public Vector3 Scale => transform.localScale;

        /// <summary>
        /// Material linked to the shape for visualization.
        /// </summary>
        [SerializeField] private Material linkedMaterial;

        readonly List<ShortcutHandler> shortcutHandlers = new List<ShortcutHandler>();

        void OnEnable()
        {
            // ToDo: Find better way to set it up once
            shortcutHandlers.Clear();
            SetupShortcutHanlders();
        }

        protected virtual void SetupShortcutHanlders()
        {
            shortcutHandlers.Add(new HandleScaleByHoldingSAndScrolling(transform));
        }

        public void DrawUI()
        {
            string helpText = "Controls:\n" +
                    "Click to add\n" +
                    "Ctrl Click to subtract";

            foreach(ShortcutHandler handler in shortcutHandlers)
            {
                helpText += "\n" + handler.ShortcutText;
            }

            EditorGUILayout.HelpBox(helpText, MessageType.None);
        }

        public virtual void HandleSceneUpdate(Event e)
        {
            foreach (ShortcutHandler handler in shortcutHandlers)
            {
                handler.HandleShortcut(e);
            }
        }

        public Color Color
        {
            set
            {
                //ToDo: Make better
                return;

                if (linkedMaterial == null)
                {
                    Renderer renderer = GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        linkedMaterial = renderer.material;
                    }
                }

                if (linkedMaterial != null)
                {
                    linkedMaterial.SetColor("_Color", value);
                }
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
}

#endif