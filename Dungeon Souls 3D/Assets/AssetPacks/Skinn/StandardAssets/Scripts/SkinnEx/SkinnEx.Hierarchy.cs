using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
	public static partial class SkinnEx
    {
        public static void ResetLocal(this Transform source)
        {
            if (!source) return;
            source.localPosition = Vector3.zero; source.localRotation = Quaternion.identity; source.localScale = Vector3.one;
        }

        public static void ResetWorld(this Transform source)
        {
            if (!source) return;
            source.position = Vector3.zero; source.rotation = Quaternion.identity; source.localScale = Vector3.one;
        }

        public static Transform[] GetBones(this SkinnedMeshRenderer skinnedMesh)
        {
            if (!skinnedMesh) return null;
            if (!skinnedMesh.sharedMesh) return skinnedMesh.bones;
            if (IsNullOrEmpty(skinnedMesh.sharedMesh.bindposes)) return skinnedMesh.bones;
            int maxBones = IsNullOrEmpty(skinnedMesh.bones) ? 0 : skinnedMesh.bones.Length;
            var fallBackBone = skinnedMesh.rootBone ? skinnedMesh.rootBone : skinnedMesh.transform;
            Transform[] transforms = skinnedMesh.bones;
            Transform[] bones = new Transform[skinnedMesh.sharedMesh.bindposes.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                if (i < maxBones && transforms[i]) bones[i] = transforms[i];
                else bones[i] = fallBackBone;
            }
            return bones;
        }

        public static bool IsInGetComponentsInChildren(this Transform root, Transform child){
            if (root == null) return false;
			Transform[] transforms = root.GetComponentsInChildren<Transform>();
			for (int i = 0; i < transforms.Length; i++) if(child == transforms[i]) return true;
			return false;
		}

        public static bool IsChild(this Transform root, Transform child)
        {
            if (!root || !child) return false;
            var transforms = root.GetAllChildern();
            for (int i = 0; i < transforms.Count; i++) if (child == transforms[i]) return true;
            return false;
        }

        public static int GetHierarchicalOrder(this Transform transform)
        {
            if (!transform || transform.parent == null) return 0;
            Transform parent = transform;
            int count = 0; while (parent) { parent = parent.parent; count++; }
            return count;
        }
        
        public static Transform FindMinHierarchicalOrder(this Transform[] transforms)
        {
            int minHierarchicalOrder = DefualtMath.IntInfinity;
            Transform root = null;
            for (int i = 0; i < transforms.Length; i++)
            {
                var transform = transforms[i];
                int hierarchicalOrder = GetHierarchicalOrder(transform);
                if (hierarchicalOrder < minHierarchicalOrder)
                {
                    minHierarchicalOrder = hierarchicalOrder;
                    root = transform;
                }
            }
            return root;
        }

        public static Transform FindMinHierarchicalOrder(this List<Transform> transforms)
        {
            int minHierarchicalOrder = DefualtMath.IntInfinity;
            Transform root = null;
            for (int i = 0; i < transforms.Count; i++)
            {
                var transform = transforms[i];
                int hierarchicalOrder = GetHierarchicalOrder(transform);
                if (hierarchicalOrder < minHierarchicalOrder)
                {
                    minHierarchicalOrder = hierarchicalOrder;
                    root = transform;
                }
            }
            return root;
        }

        public static bool HasTrueRootBone(this Transform[] transforms)
        {
            if (IsNullOrEmpty(transforms)) return false;
            var root = transforms.FindMinHierarchicalOrder();
            var minHierarchicalOrder = GetHierarchicalOrder(root);
            for (int i = 0; i < transforms.Length; i++)
            {
                var transform = transforms[i]; if (transform == root) continue;
                int hierarchicalOrder = GetHierarchicalOrder(transform);
                if (hierarchicalOrder <= minHierarchicalOrder) return false;
            }
            return true;
        }

        public static bool HasTrueRootBone(this List<Transform> transforms)
        {
            if (IsNullOrEmpty(transforms)) return false;
            var root = transforms.FindMinHierarchicalOrder();
            var minHierarchicalOrder = GetHierarchicalOrder(root);
            for (int i = 0; i < transforms.Count; i++)
            {
                var transform = transforms[i]; if (transform == root) continue;
                int hierarchicalOrder = GetHierarchicalOrder(transform);
                if (hierarchicalOrder <= minHierarchicalOrder) return false;
            }
            return true;
        }

        public static Transform GetRootBone(this Transform[] transforms)
        {
            if (IsNullOrEmpty(transforms)) return null;
            return transforms.HasTrueRootBone() ? transforms.FindMinHierarchicalOrder() : null;
        }

        public static Transform GetRootBone(this List<Transform> transforms)
        {
            if (IsNullOrEmpty(transforms)) return null;
            return transforms.HasTrueRootBone() ? transforms.FindMinHierarchicalOrder() : null;
        }

        public static Transform FirstNonNull(this Transform[] transforms)
        {
            if (IsNullOrEmpty(transforms)) return null;
            for (int i = 0; i < transforms.Length; i++) if (transforms[i]) return transforms[i];
            return null;
        }

        public static Transform FirstNonNull(this List<Transform> transforms)
        {
            if (IsNullOrEmpty(transforms)) return null;
            for (int i = 0; i < transforms.Count; i++) if (transforms[i]) return transforms[i];
            return null;
        }

        public static bool HasAnyNulls(this List<Transform> transforms)
        {
            if (IsNullOrEmpty(transforms)) return true;
            for (int i = 0; i < transforms.Count; i++) if (!transforms[i]) return true;
            return false;
        }

        public static bool HasAnyNulls(this Transform[] transforms)
        {
            if (IsNullOrEmpty(transforms)) return true;
            for (int i = 0; i < transforms.Length; i++) if (!transforms[i]) return true;
            return false;
        }

        public static Dictionary<int, Transform> GetAllChildernKeyPairs(this Transform transform, bool inculdeRoot = false)
        {
            Dictionary<int, Transform> transforms = new Dictionary<int, Transform>();
            List<string> hashes = new List<string>();
            if (!transform) return new Dictionary<int, Transform>();
            if (inculdeRoot) transforms.Add(Animator.StringToHash(GetOrderedName(ref hashes, transform.name)), transform);
            AddChildrenRecursive(transform, ref transforms, ref hashes);
            return transforms;
        }

        private static void AddChildrenRecursive(Transform transform, ref Dictionary<int, Transform> transforms, ref List<string> hashes)
        {

            for (int i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                transforms.Add(Animator.StringToHash(GetOrderedName(ref hashes, child.name)), child);
                AddChildrenRecursive(child, ref transforms, ref hashes);
            }
        }

        public static List<Transform> GetAllChildern(this Transform transform, bool inculdeRoot = false)
        {
            List<Transform> transforms = new List<Transform>();
            if (!transform) return new List<Transform>();
            if (inculdeRoot) transforms.Add(transform);
            AddChildrenRecursive(transform, ref transforms);
            return transforms;
        }

        private static void AddChildrenRecursive(Transform transform, ref List<Transform> transforms)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                transforms.Add(child);
                AddChildrenRecursive(child, ref transforms);
            }
        }

        public static List<Transform> GetAllParents(this Transform transform, bool inculdeChild = false)
        {
            List<Transform> transforms = new List<Transform>();
            if (!transform) return new List<Transform>();
            if (inculdeChild) transforms.Add(transform);
            var parent = transform.parent;
            while (parent != null) { transforms.Add(parent); parent = parent.parent; }
            return transforms;
        }

        public static bool IsParent(this Transform transform, Transform isParent)
        {
            if (!transform || !isParent) return false;
            var parent = transform.parent;
            while (parent != null) { if (parent == isParent) return true; parent = parent.parent; }
            return false;
        }

        public static bool IsParentOfAny(this Transform[] transforms, Transform isParent)
        {
            if (IsNullOrEmpty(transforms) || !isParent) return false;
            foreach (var item in transforms) if (item.IsParent(isParent)) return true;
            return false;
        }

        public static void CopyLocalSpace(this Transform a, Transform b)
        {
            if (!a || !b) return;
            a.localPosition = b.localPosition;
            a.localRotation = b.localRotation;
            a.localScale = b.localScale;
        }

        public static void CopyWorldSpace(this Transform a, Transform b)
        {
            if (!a || !b) return;
            a.position = b.position;
            a.rotation = b.rotation;
            a.localScale = b.lossyScale;
        }
        
        /// <summary>
        /// returns and mirrors a transform if found. uses common naming conventions.
        /// </summary>
        public static Transform MirrorHumanoidGeneric(this Transform transform, bool left,  List<Transform> transforms = null)
        {
            var mirrorTransform = transform.FindMirrorByName(left, transforms);
            if (!mirrorTransform) return mirrorTransform;

            var position = transform.root.InverseTransformPoint(transform.position);
            position.x = -position.x;
            mirrorTransform.position = transform.root.TransformPoint(position);

            var rotation = Quaternion.Inverse(transform.root.rotation) * transform.rotation;
            rotation.y = -rotation.y; rotation.z = -rotation.z;
            rotation = transform.root.rotation * rotation;
            mirrorTransform.rotation = rotation;

            var localScale = transform.localScale;
            mirrorTransform.localScale = localScale;

            return mirrorTransform;
        }

    }
}