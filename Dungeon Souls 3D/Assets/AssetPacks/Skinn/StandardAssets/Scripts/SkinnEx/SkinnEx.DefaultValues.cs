#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using System;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public static partial class DefaultColors
        {
            public static readonly Color WhiteLightOverlay = new Color(1f, 1f, 1f, Alpha0);
            public static readonly Color WhiteDarkOverlay = new Color(1f, 1f, 1f, Alpha1);

            public static readonly Color BlueOverlayLight = new Color(0f, 0f, 1f, Alpha0);
            public static readonly Color BlueOverlayDark = new Color(0f, 0f, 1f, Alpha1);

            public const float Alpha0 = 0.125f;
            public const float Alpha1 = 0.325f;
            public const float Alpha2 = 0.425f;
            public const float Alpha3 = 0.625f;
            public const float Alpha4 = 0.825f;

            public static Color OrderedColor(int index, float alpha = 1f)
            {
                Color color;
                int colorIndex = (int)Mathf.Repeat(index, 12);
                switch (colorIndex)
                {
                    case 0: color = Color.yellow * 0.5f + Color.magenta * 0.5f; break;
                    case 1: color = Color.red * 0.5f + Color.blue * 0.5f; break;
                    case 2: color = Color.white * 0.5f + Color.green * 0.5f; break;
                    case 3: color = Color.green; break;
                    case 4: color = Color.cyan; break;
                    case 5: color = Color.gray * 0.5f + Color.blue * 0.5f; break;
                    case 6: color = Color.yellow * 0.5f + Color.red * 0.5f; break;
                    case 7: color = Color.magenta; break;
                    case 8: color = Color.black * 0.5f + Color.red * 0.5f; break;
                    case 9: color = Color.blue; break;
                    case 10: color = Color.white * 0.5f + Color.cyan; break;
                    case 11: color = Color.yellow; break;
                    case 12: color = Color.red; break;
                    default: color = Color.white; break;
                }
                color.a = alpha;
                return color;
            }

            public static Color ErrorColor { get { return new Color(1f, 0f, 0f, 0.3f); } }
            public static Color WarningColor { get { return new Color(1f, 1f, 0f, 0.3f); } }

        }

        public static class AssetURLS
        {
            public const string UAS = "https://www.assetstore.unity3d.com/en/#!/content/86532";
            public const string UASForum = "https://forum.unity.com/threads/skinn-vertex-mapper-released.456494/";
            public const string YouTube = "https://www.youtube.com/channel/UCBSInjON0ldeK4XzSZcMPTg";
            public const string Itch = "https://cwmanley.itch.io/skinn-vertex-mapper";
        }

        public static partial class DefualtMath
        {
            /// <summary>
            /// the default rotation of the models used in the uma examples.
            /// </summary>
            public static Vector3 UMARotation { get { return new Vector3(-89.98f, 0.0f, -180.0f); } }

            public const int IntInfinity = 2147483647;

            public const int IntNegativeInfinity = -2147483647;

            public static float FormatFloat(float source, int decimals = 2)
            {
                if (source == 0f) return source;
                switch (decimals)
                {
                    case 0: return System.Convert.ToSingle(source.ToString("0"));
                    case 1: return System.Convert.ToSingle(source.ToString("0.0"));
                    case 2: return System.Convert.ToSingle(source.ToString("0.00"));
                    case 3: return System.Convert.ToSingle(source.ToString("0.000"));
                    case 4: return System.Convert.ToSingle(source.ToString("0.0000"));
                    case 5: return System.Convert.ToSingle(source.ToString("0.00000"));
                    default: return source;
                }
            }
        }

        public static float FormatFloat(this float source, int decimals = 2)
        {
            return DefualtMath.FormatFloat(source, decimals);
        }

        public static bool IsNullOrNotInAScene(GameObject source)
        {
            if (!source || !source.gameObject.scene.IsValid()) return true; else return false;
        }

        public static bool IsNullOrNotInAScene(Component source)
        {
            if (!source || !source.gameObject || !source.gameObject.scene.IsValid()) return true; else return false;
        }

        public static bool IsNullOrEmpty<T>(T[] source)
        {
            if (source == null || source.Length == 0) return true; else return false;
        }

        public static bool IsNullOrEmpty<T>(List<T> source)
        {
            if (source == null || source.Count == 0) return true; else return false;
        }

        public static bool IsNullOrEmpty<T>(T[] source, int count)
        {
            if (source == null || source.Length == 0) return true;
            else if (source.Length != count) return true;
            else return false;
        }

        public static bool IsNullOrEmpty<T>(List<T> source, int count)
        {
            if (source == null || source.Count == 0) return true;
            else if (source.Count < count) return true;
            else return false;
        }

        public static bool IsNullOrLessThan<T>(T[] source, int count)
        {
            if (source == null || source.Length < count) return true;
            else if (source.Length < count) return true;
            else return false;
        }

        public static bool IsNullOrLessThan<T>(List<T> source, int count)
        {
            if (source == null || source.Count < count) return true;
            else if (source.Count < count) return true;
            else return false;
        }

        public static bool IsNullOrLessThan(string source, int count)
        {
            if (string.IsNullOrEmpty(source)) return true;
            else if (source.Length < count) return true;
            else return false;
        }

        public static void Release(UnityEngine.Object target)
        {
            if (target == null) return;
            var transform = target as Transform;
            if (transform)
            { if (Application.isEditor) UnityEngine.Object.DestroyImmediate(transform.gameObject); else UnityEngine.Object.Destroy(transform.gameObject); return; }
            if (Application.isEditor) UnityEngine.Object.DestroyImmediate(target); else UnityEngine.Object.Destroy(target);
        }

        public static void ReleaseWithUndo(UnityEngine.Object target)
        {
            if (target == null) return;
            var transform = target as Transform;

#if UNITY_EDITOR
            if (transform)
            { if (Application.isEditor) Undo.DestroyObjectImmediate(transform.gameObject); else UnityEngine.Object.Destroy(transform.gameObject); return; }
            if (Application.isEditor) Undo.DestroyObjectImmediate(target); else UnityEngine.Object.Destroy(target);
#endif

            if (transform)
            { if (Application.isEditor) UnityEngine.Object.DestroyImmediate(transform.gameObject); else UnityEngine.Object.Destroy(transform.gameObject); return; }
            if (Application.isEditor) UnityEngine.Object.DestroyImmediate(target); else UnityEngine.Object.Destroy(target);
        }


        public static void Release(ComputeBuffer target)
        {
            if (target == null) return;
            target.Release();
        }

        public static GameObject FindGameObject(int instanceID)
        {
            var contextRoot = Array.Find(UnityEngine.Object.FindObjectsOfType<GameObject>(),
                            (x) => { return x.GetInstanceID() == instanceID; });
            if (!contextRoot)
                contextRoot = Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                    (x) => { return x.GetInstanceID() == instanceID; });
            return contextRoot;
        }

    }
}