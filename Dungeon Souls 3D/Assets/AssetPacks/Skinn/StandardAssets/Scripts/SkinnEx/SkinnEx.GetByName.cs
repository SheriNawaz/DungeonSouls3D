using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
	public static partial class SkinnEx
    {
        public static Transform FindTransformByName(this Transform[] array, string name, bool returnNull = false)
        {
			for (int i = 0; i < array.Length; i++) if(name == array[i].name) return array [i];
			if (returnNull) return null;
			return array [0];
		}
        
        /// <summary>
        /// returns a mirrored transform if found. uses common naming conventions.
        /// </summary>
        public static Transform FindMirrorByName(this Transform transform, bool left, List<Transform> transforms = null)
        {
            string commonName;
            return FindMirrorByName(transform, left, transforms, out commonName);
        }

        /// <summary>
        /// returns a mirrored transform if found. uses common naming conventions.
        /// </summary>
        public static Transform FindMirrorByName(this Transform transform, bool left, List<Transform> transforms, out string commonName)
        {
            commonName = "";
            if (!transform || IsNullOrLessThan(transform.name, 2)) return null;

            var name = transform.name.ToLower();
            commonName = name;

            var mirrorName = "";
            if (left)
            {
                if (name.Contains("left"))
                {
                    mirrorName = name.Replace("left", "right");
                    commonName = commonName.Replace("left", "");
                }
                else if (name.EndsWith("_l"))
                {
                    mirrorName = name.TrimEnd('l'); mirrorName += "r";
                    commonName = commonName.TrimEnd('l');
                }
                else if (name.StartsWith("l") && !char.IsLetter(name[1]) || (char.IsLetter(name[1]) && char.IsUpper(name[1])))
                {
                    mirrorName = name.TrimStart('l');
                    mirrorName = "r" + mirrorName;
                    commonName = commonName.TrimStart('l');
                }
            }
            else
            {
                if (name.Contains("right"))
                {
                    mirrorName = name.Replace("right", "left");
                    commonName = commonName.Replace("right", "");
                }
                else if (name.EndsWith("_r"))
                {
                    mirrorName = name.TrimEnd('r');
                    mirrorName += "l";
                    commonName = commonName.TrimEnd('r');
                }
                else if (name.StartsWith("l") && !char.IsLetter(name[1]) || (char.IsLetter(name[1]) && char.IsUpper(name[1])))
                {
                    mirrorName = name.TrimStart('l');
                    mirrorName = "r" + mirrorName;
                    commonName = commonName.TrimStart('r');
                }
            }

            if (mirrorName.Length < 1) return null;
            if (IsNullOrEmpty(transforms)) transforms = transform.root.GetAllChildern(true);
            var mirrorTransform = transforms.Find((x) => { return x.name.ToLower() == mirrorName; });
            return mirrorTransform;
        }
    }
}