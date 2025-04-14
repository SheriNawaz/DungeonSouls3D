using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnContext
    {
        [InitializeOnLoad]
        private class InitializeContext
        {
            static InitializeContext()
            {
                OnEnable();
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;

#if UNITY_2018_3_OR_NEWER
                EditorApplication.projectChanged -= EditorUpdate;
                EditorApplication.projectChanged += EditorUpdate;
#endif


#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui -= DuringSceneGui;
                SceneView.duringSceneGui += DuringSceneGui;
#else
                SceneView.onSceneGUIDelegate -= DuringSceneGui;
                SceneView.onSceneGUIDelegate += DuringSceneGui;
#endif

            }
            private static bool StartedComplie = false;

            private static void EditorUpdate()
            {
                var isCompiling = EditorApplication.isCompiling || (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode);

                if (isCompiling)
                {
                    if (!StartedComplie)
                    {
                        if (SkinnGizmos.OnEditorRelease != null) SkinnGizmos.OnEditorRelease();
                        StartedComplie = true;
                        SkinnGizmos.Release();
                        SkinnInternalAsset.ClearTempData();
                    }
                }
                else StartedComplie = false;
            }

            private static void DuringSceneGui(SceneView sceneView)
            {
                if (SceneView.currentDrawingSceneView != SceneView.lastActiveSceneView) return;
                if (SkinnGizmos.OnSceneGUI != null) SkinnGizmos.OnSceneGUI();
            }

            private static void OnEnable()
            {
                if (!SkinnInternalAsset.Asset || !SkinnInternalAsset.Asset.HasShaders())
                { }
            }
        }

        [MenuItem("CONTEXT/SkinnedMeshRenderer/Skinn VM: Save Mesh", true)]
        public static bool CanSkinnedMeshRendererSaveMesh(MenuCommand menuCommand)
        {
            var context = menuCommand.context as SkinnedMeshRenderer;
            if (SkinnEx.IsNullOrNotInAScene(context) || context.sharedMesh == null) return false;
            return true;
        }

        [MenuItem("CONTEXT/SkinnedMeshRenderer/Skinn VM: Save Mesh")]
        public static void SkinnedMeshRendererSaveMesh(MenuCommand menuCommand)
        {
            var context = menuCommand.context as SkinnedMeshRenderer;
            var path = EditorUtility.SaveFilePanelInProject("Save Mesh", context.name, "asset", "Mesh Asset");
            if (string.IsNullOrEmpty(path)) return;
            AssetDatabase.CreateAsset(context.sharedMesh.Clone() as Mesh, path);
            AssetDatabase.SaveAssets();
            var savedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(path, typeof(Mesh));
            if (!savedMesh) return;
            context.sharedMesh = savedMesh;
        }
    }
}
