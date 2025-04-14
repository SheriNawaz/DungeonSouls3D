using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        internal static bool MergeCloneIfNeeded(this Transform root, Transform[] b, out Transform[] finalBones, out List<Transform> createdBones)
        {
            createdBones = new List<Transform>();
            finalBones = new Transform[b.Length];
            if (IsNullOrEmpty(b)) return false;
            else if (!b.FirstNonNull()) return false;
            var stack = root.GetAllChildernKeyPairs();
            var rootBone = b.FirstNonNull().root;
            List<Transform> otherStack = new List<Transform>(rootBone.GetAllChildern());
            int[] otherHashes = otherStack.GetNamesToHashes();
            for (int i = 0; i < otherStack.Count; i++)
            {
                Transform bone;
                if (!stack.TryGetValue(otherHashes[i], out bone))
                {
                    if (b.GetIndex(otherStack[i], true) < 0  && !b.IsParentOfAny(otherStack[i])) continue;
                    bone = new GameObject(otherStack[i].name).transform;
                    createdBones.Add(bone);
                    Transform parent;
                    if (stack.TryGetValue(Animator.StringToHash(EnforceObjectName(otherStack[i].parent)), out parent)) bone.SetParent(parent);
                    else bone.SetParent(root);

                    stack.Add(otherHashes[i], bone);
                    bone.CopyLocalSpace(otherStack[i]);
                }
            }
            int[] boneHashes = b.GetNamesToHashes();
            finalBones = new Transform[b.Length];
            for (int i = 0; i < finalBones.Length; i++)
            {
                Transform bone;
                if (stack.TryGetValue(boneHashes[i], out bone))
                    finalBones[i] = bone;
                else
                {
                    finalBones[i] = root;
                    if (b[i]) Debug.LogWarning("Missing Bone " + b[i].name);
                }
            }
            return true;
        }
    }
}
