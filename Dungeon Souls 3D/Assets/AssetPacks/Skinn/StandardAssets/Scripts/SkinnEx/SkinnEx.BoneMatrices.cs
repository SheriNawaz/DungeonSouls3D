using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
	public static partial class SkinnEx
    {
        public static Matrix4x4[] GetMatrices(this BakeOptions options, Matrix4x4[] bindposes, Transform[] bones)
        {
            var bakeOptions = BakeOptions.FallbackToDefualtValue(options);
            if (IsNullOrEmpty(bindposes) || IsNullOrEmpty(bones)) return null;
            var rootTransform = bones.FirstNonNull().root;
            var poses = new Matrix4x4[bones.Length];
            var worldMatrix = Matrix4x4.identity;
            if (!bakeOptions.world) worldMatrix = rootTransform.localToWorldMatrix.inverse;
            int minIndex = Mathf.Min(bindposes.Length, bones.Length);
            for (int i = 0; i < minIndex; i++)
            {
                var pose = worldMatrix;
                pose *= bones[i] ? bones[i].localToWorldMatrix : Matrix4x4.identity;
                pose *= bindposes[i];
                poses[i] = pose;
            }
            return poses;
        }

        public static Matrix4x4[] GetBindposes(this Transform[] bones, bool worldSpace = false)
        {
            if (IsNullOrEmpty(bones)) return new Matrix4x4[0];
            var firstNonNull = bones.FirstNonNull(); if (!firstNonNull) return new Matrix4x4[bones.Length];
            var root = firstNonNull.root;
            var position = root.position; var rotation = root.rotation; var localScale = root.localScale;
            if (!worldSpace) { root.position = Vector3.zero; root.rotation = Quaternion.identity; root.localScale = Vector3.one; }
            var boneCount = bones.Length;
            var poses = new Matrix4x4[boneCount];
            var trueRootBone = bones.HasTrueRootBone() ? bones.FindMinHierarchicalOrder() : null;

            for (int i = 0; i < boneCount; i++)
            {
                if (trueRootBone && bones[i] == trueRootBone) poses[i] = trueRootBone.worldToLocalMatrix * trueRootBone.root.localToWorldMatrix;
                else if (bones[i]) poses[i] = bones[i].worldToLocalMatrix;
                else poses[i] = Matrix4x4.identity;
            }
            root.position = position; root.rotation = rotation; root.localScale = localScale;
            return poses;
        }

        internal static Matrix4x4 GetLocalMatrix(Transform transform)
        {
            SkinnGizmos.ResetTransforms();
            var transformA = SkinnGizmos.Transform0;
            transformA.position = transform.root.position;
            transformA.rotation = transform.root.rotation;
            transformA.localScale = transform.root.localScale;
            var transformB = SkinnGizmos.Transform1;
            transformB.position = transform.position;
            transformB.rotation = transform.rotation;
            transformB.localScale = transform.lossyScale;
            transformB.SetParent(transformA, true);
            Matrix4x4 localMatrix = transformB.localToWorldMatrix;
            SkinnGizmos.ResetTransforms();
            return localMatrix;
        }

        internal static Matrix4x4[] ToAnimationSpacePoses(Transform[] bones)
        {
            if (IsNullOrEmpty(bones)) return null;
            var boneCount = bones.Length;
            var poses = new Matrix4x4[boneCount];
            for (int i = 0; i < boneCount; i++) { if (bones[i]) poses[i] = GetLocalMatrix(bones[i]); else poses[i] = Matrix4x4.identity; }
            return poses;
        }

        internal static Matrix4x4[] Bindposes(List<BoneElement> skeleton)
        {
            var weights = new Matrix4x4[skeleton.Count - 1];
            for (int i = 1; i < skeleton.Count; i++) weights[i - 1] = skeleton[i].bindpose;
            return weights;
        }

    }
}