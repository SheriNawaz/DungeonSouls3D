using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
        {

        /// <summary>
        /// uses Animator.StringToHash() to convert strings to hashes. 
        /// duplicates are named xxx_#
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int[] GetOrderedHashes(this string[] array)
        {
            List<string> names = new List<string>();
            int[] hashId = new int[array.Length];
            for (int i = 0; i < array.Length; i++) hashId[i] = Animator.StringToHash(GetOrderedName(ref names, array[i]));
            return hashId;
        }

        /// <summary>
        /// uses Animator.StringToHash() to convert strings to hashes. 
        /// duplicates are named xxx_#
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int[] GetOrderedHashes(this Transform[] array)
        {
            List<string> names = new List<string>();
            int[] hashId = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
                if (!array[i]) hashId[i] = Animator.StringToHash(GetOrderedName(ref names, "Null"));
                else hashId[i] = Animator.StringToHash(GetOrderedName(ref names, array[i].name));
            return hashId;
        }

        public static int[] GetOrderedHashes(this List <Transform> array)
        {
            List<string> names = new List<string>();
            int[] hashId = new int[array.Count];
            for (int i = 0; i < array.Count; i++)
                if (!array[i]) hashId[i] = Animator.StringToHash(GetOrderedName(ref names, "Null"));
                else hashId[i] = Animator.StringToHash(GetOrderedName(ref names, array[i].name));
            return hashId;
        }

        public static int[] GetNamesToHashes(this List<Transform> array)
        {
            int[] hashId = new int[array.Count];
            for (int i = 0; i < array.Count; i++)
                if (!array[i]) hashId[i] = Animator.StringToHash("Null");
                else hashId[i] = Animator.StringToHash(array[i].name);
            return hashId;
        }

        public static int[] GetNamesToHashes(this Transform[] array)
        {
            int[] hashId = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
                if (!array[i]) hashId[i] = Animator.StringToHash("Null");
                else hashId[i] = Animator.StringToHash(array[i].name);
            return hashId;
        }

        /// <summary>
        /// uses Animator.StringToHash() to convert strings to hashes. 
        /// duplicates are named xxx_#
        /// </summary>
        /// <param name="names"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetOrderedName(ref List<string> names, string name)
        {
            string orderedName = string.IsNullOrEmpty(name) ? "NoName" : name;
            int count = 0;
            string validName = orderedName;
            while (names.Contains(validName))
            {
                count++;
                validName = string.Format("{0}_{1}", orderedName, count);
            }
            names.Add(validName);
            return validName;
        }

        /// <summary>
        /// creates an array of unique ids convertible to int.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string[] GetUniqueIDs(int count)
        {
            List<string> ids = new List<string>();
            for (int i = 0; i < count; i++) ids.Add(GetUniqueID(ref ids));
            return ids.ToArray();
        }

        public static string GetUniqueID(ref List<string> ids)
        {
            string id = GetUniqueID32();
            int maxAtempts = 3 * ids.Count;
            int count = 0;
            while (count < maxAtempts && ids.Contains(id))
            {
                Debug.LogWarningFormat("id failed, retrying {0} of {1}", count, maxAtempts);
                id = GetUniqueID32(); count++;
            }
            return id;
        }

        public static string GetUniqueID() { return GetUniqueID32(); }

        public static string GetUniqueID32() { return System.BitConverter.ToInt32(System.Guid.NewGuid().ToByteArray(), 0).ToString(); }

        public static string GetUniqueID64() { return System.BitConverter.ToInt64(System.Guid.NewGuid().ToByteArray(), 0).ToString(); }
    }
}