using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
	public static partial class SkinnEx
    {
        public static int GetIndex(this Transform[] array, int instanceID, bool allowNegative = true)
        {
            return System.Array.FindIndex(array, (a) => { return a.GetInstanceID() == instanceID; }).ClampToZero(allowNegative);
        }

        public static int GetIndex(this Transform[] array, Transform value, bool allowNegative = true)
        {
            return System.Array.FindIndex(array, (a) => { return a == value; }).ClampToZero(allowNegative);
		}

        public static int GetIndex(this Transform[] array, string value, bool allowNegative = true)
        {
            return System.Array.FindIndex(array, (a) => { return a.name == value; }).ClampToZero(allowNegative);
        }

        public static int GetIndex(this string[] array, string value, bool allowNegative = true)
        {
            return System.Array.FindIndex(array, (a) => { return a == value; }).ClampToZero(allowNegative);
        }

        public static int GetIndex(this int[] array, int value, bool allowNegative = true)
        {
            return System.Array.FindIndex(array, (a) => { return a == value; }).ClampToZero(allowNegative);
        }

        public static int GetIndex(this List<int> array, int value, bool allowNegative = true)
        {
            return array.FindIndex((a) => { return a == value; }).ClampToZero(allowNegative);
        }

        private static int ClampToZero(this int value, bool allowNegative)
        {
            int v = value;
            if (!allowNegative && v < 0) v = 0;
            return v;
        }

        public static int GetClosestIndex(this Vector3[] source, Vector3 vector)
        {

            int index = 0;
            float minDistance = Mathf.Infinity;
            for (int i = 0; i < source.Length; i++)
            {
                float sqrMagnitude = (vector - source[i]).sqrMagnitude;
                if (sqrMagnitude < minDistance & source[i] != vector)
                {
                    minDistance = sqrMagnitude;
                    index = i;
                }
            }
            return index;
        }

        public static int GetClosestIndex(this Vector3[] source, Vector3 vector, int skipIndex)
        {
            int index = 0;
            float minDistance = Mathf.Infinity;
            for (int i = 0; i < source.Length; i++)
            {
                if (i == skipIndex) continue;
                float sqrMagnitude = (vector - source[i]).sqrMagnitude;
                if (sqrMagnitude < minDistance)
                {
                    minDistance = sqrMagnitude;
                    index = i;
                }
            }
            return index;
        }

        public static int GetClosestNonMatchingIndex(this Vector3[] source, Vector3 vector)
        {
            int index = 0;
            float minDistance = Mathf.Infinity;
            for (int i = 0; i < source.Length; i++)
            {
                float sqrMagnitude = (vector - source[i]).sqrMagnitude;
                if (sqrMagnitude < minDistance & source[i] != vector)
                {
                    minDistance = sqrMagnitude;
                    index = i;
                }
            }
            return index;
        }

        public static int[] GetClosestIndexInRange(this Vector3[] source, int index, float range, bool allowIndex = false, int max = System.Int32.MaxValue)
        {

            List<int> v1 = new List<int>(0);
            for (int i = 0; i < source.Length; i++)
            {
                if (v1.Count >= max) return v1.ToArray();
                if (i == index) continue;
                if (!allowIndex && source[i] == source[index]) continue;
                float test = Vector3.Distance(source[index], source[i]);
                if (test < range) v1.Add(i);
            }
            return v1.ToArray();
        }
    }
}