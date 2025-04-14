using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public static void SyncShapesToThis(this SkinnedMeshRenderer source, Transform root = null,  string undoLabel = "")
        {
            if (IsNullOrNotVailid(source)) return;
            if (!root) root = source.transform.root;

             var mesh = source.GetSharedMesh();
            var blendShapeCount = mesh.blendShapeCount;
            var smrs = root.gameObject.GetValidSkinnedMeshes(true).ToArray();
            bool record = string.IsNullOrEmpty(undoLabel);
            for (int i = 0; i < blendShapeCount; i++)
            {
                var shapeName = source.sharedMesh.GetBlendShapeName(i);
                var shapeWeight = source.GetBlendShapeWeight(i);

                for (int ii = 0; ii < smrs.Length; ii++)
                {
                    if (smrs[ii] == source) continue;
                    smrs[ii].ResetBlendshapes(undoLabel);

                    if (record)
                    {
#if UNITY_EDITOR
                        Undo.RecordObject(smrs[i], undoLabel);
#endif
                    }

                    for (int iii = 0; iii < smrs[ii].sharedMesh.blendShapeCount; iii++)
                    {
                        if (smrs[ii].sharedMesh.GetBlendShapeName(iii) == shapeName) smrs[ii].SetBlendShapeWeight(iii, shapeWeight);
                    }
                }
            }
        }

        public static void SyncShapesFromRoot(this SkinnedMeshRenderer source, Transform root = null, string undoLabel = "")
        {
            if (IsNullOrNotVailid(source)) return;

#if UNITY_EDITOR
            if (string.IsNullOrEmpty(undoLabel)) Undo.RecordObject(source, undoLabel);
#endif
            if (!root) root = source.transform.root;
            var smrs = root.gameObject.GetValidSkinnedMeshes(true).ToArray();

            source.ResetBlendshapes(undoLabel);
            for (int i = 0; i < smrs.Length; i++)
            {
                if (smrs[i] == source) continue;
                for (int ii = 0; ii < smrs[i].sharedMesh.blendShapeCount; ii++)
                {
                    var shapeName = smrs[i].sharedMesh.GetBlendShapeName(ii);
                    var shapeWeight = smrs[i].GetBlendShapeWeight(ii);

                    for (int iii = 0; iii < source.sharedMesh.blendShapeCount; iii++)
                    {
                        if (source.sharedMesh.GetBlendShapeName(iii) == shapeName) source.SetBlendShapeWeight(iii, shapeWeight);
                    }
                }
            }
        }


        public static void ResetBlendshapes(this SkinnedMeshRenderer source, string undoLabel = "")
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(undoLabel)) Undo.RecordObject(source, undoLabel);
#endif
            if (IsNullOrNotVailid(source)) return;
            var blendShapeCount = source.sharedMesh.blendShapeCount;
            for (int i = 0; i < blendShapeCount; i++) source.SetBlendShapeWeight(i, 0f);
        }
    }
}