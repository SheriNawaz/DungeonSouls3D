using System;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnGizmos
    {
        public static Action OnSceneGUI;
        public static Action OnEditorRelease;

        public const string FallBackShader = "Hidden/Internal-Colored";

        private static Material gizmoMaterial;
        public static Material GizmoMaterial
        {
            get
            {
                if (gizmoMaterial != null) return gizmoMaterial;
                Shader shader = Shader.Find("Hidden/Skinn/VertexColor/Unlit/Overlay");
                if (shader) gizmoMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                else
                {
                    Debug.LogError("Missing InternalData!");
                    shader = Shader.Find(FallBackShader);
                    if (shader) gizmoMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                }
                return gizmoMaterial;
            }
        }

        private static Material overlayMaterial;
        public static Material OverlayMaterial
        {
            get
            {
                if (overlayMaterial != null) return overlayMaterial;
                Shader shader = SkinnInternalAsset.Asset ? SkinnInternalAsset.Asset.debugVertexOverlay : null;
                if (shader) overlayMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                else
                {
                    Debug.LogError("Missing InternalData!");
                    shader = Shader.Find(FallBackShader);
                    if (shader) overlayMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                }
                return overlayMaterial;
            }
        }

        private static Transform transform0;
        public static Transform Transform0
        {
            get
            {
                if (transform0 == null)
                {
                    transform0 = new GameObject("SkinTransform0").transform;
                    transform0.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }
                return transform0;
            }
        }

        private static Transform transform1;
        public static Transform Transform1
        {
            get
            {
                if (transform1 == null)
                {
                    transform1 = new GameObject("SkinTransform1").transform;
                    transform1.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }
                return transform1;
            }
        }

        public static void ResetTransforms(Transform parent = null)
        {
            if (transform0)
            {
                transform0.SetParent(parent, false);
                transform0.localPosition = Vector3.zero; transform0.localRotation = Quaternion.identity; transform0.localScale = Vector3.one;
            }
            if (transform1)
            {
                transform1.SetParent(parent, false);
                transform1.localPosition = Vector3.zero; transform1.localRotation = Quaternion.identity; transform1.localScale = Vector3.one;
            }
        }

        private static Mesh dynamicCube = null;
        public static Mesh DynamicCube(Color color, Vector3 scale, Vector3 parentScale, float size = 1)
        {
            var id = SkinnEx.BoxPrimativeID(color, scale, parentScale, size);
            if (!dynamicCube || id != dynamicCube.name) dynamicCube = SkinnEx.BoxPrimative(color, scale, parentScale, size, dynamicCube, true);
            return dynamicCube;
        }

        public static void Release()
        {
            SkinnEx.Release(gizmoMaterial);
            SkinnEx.Release(overlayMaterial);
            SkinnEx.Release(transform0);
            SkinnEx.Release(transform1);
            SkinnEx.Release(dynamicCube);
        }
    }
}
