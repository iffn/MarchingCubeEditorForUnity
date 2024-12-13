#if UNITY_EDITOR
using iffnsStuff.MarchingCubeEditor.Core;
using UnityEditor;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.SceneEditor
{
    [CustomEditor(typeof(MarchingCubesController))]
    public class MarchingCubesControllerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MarchingCubesController controller = (MarchingCubesController)target;

            if (GUILayout.Button("Open In Editor"))
            {
                MarchingCubeEditor editor = (MarchingCubeEditor)EditorWindow.GetWindow(typeof(MarchingCubeEditor));
                editor.UpdateLinkedCubesController(controller);
            }
        }
    }
}
#endif