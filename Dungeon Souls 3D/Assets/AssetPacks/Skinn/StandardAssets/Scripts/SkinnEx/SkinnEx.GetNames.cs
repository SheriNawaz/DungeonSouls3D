using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        /// <summary>
        /// returns a string array containing transform names.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string[] GetTransformNames(this Transform[] array)
        {
			var names = new string[array.Length];
			for(int i = 0; i < array.Length; i++) names [i] = EnforceString(array[i].name);
			return names;
		}

        public static string[] GetBlendshapeNames(this Mesh mesh)
        {
            if (!mesh) return new string[0];
            var names = new string[mesh.blendShapeCount];
            for (int i = 0; i < names.Length; i++) names[i] = EnforceString(mesh.GetBlendShapeName(i));
            return names;
        }

        public static string[] GetBlendshapeNames(this SkinnedMeshRenderer source)
        {
            if (!source) return new string[0];
            return GetBlendshapeNames(source.sharedMesh);
        }

        public static List<string> GetBlendshapeNames(this Mesh[] meshes)
        {
            if (IsNullOrEmpty(meshes)) return new List<string>();
            var names = new  List<string>();
            var nameHashes = new List<int>();
            for (int i = 0; i < meshes.Length; i++)
            {
                var iNames = GetBlendshapeNames(meshes[i]);
                var iHashes = iNames.GetOrderedHashes();
                for (int ii = 0; ii < iNames.Length; ii++)
                {
                    if (nameHashes.Contains(iHashes[ii])) continue;
                    nameHashes.Add(iHashes[ii]);
                    names.Add(iNames[ii]);
                }
            }
            return names;
        }

        /// <summary>
        /// returns "No Name" instead of null, "", or " ".
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EnforceString(string value)
        {
            if (string.IsNullOrEmpty(value)) return "No Name";
            //|| string.IsNullOrWhiteSpace(value)) return "No Name";
            return value;
        }

        public static string EnforceString(string value, string nullValue)
        {
            if (string.IsNullOrEmpty(value)) return nullValue;
            return value;
        }

        public static string EnforceObjectName(Object value)
        {
            if (value == null || string.IsNullOrEmpty(value.name)) return "No Name";
            return value.name;
        }

        public static string EnforceObjectName(Object value, string nullValue)
        {
            if (value == null || string.IsNullOrEmpty(value.name)) return nullValue;
            return value.name;
        }

        public static string ClampEllipsis(this string source, int max)
        {
            string displayName = EnforceString(source, "Null");
            int maxLength = Mathf.Max(7, max);
            if (displayName.Length >= maxLength) displayName = displayName.Remove(maxLength - 4) + "...";
            return displayName;
        }
    }
}