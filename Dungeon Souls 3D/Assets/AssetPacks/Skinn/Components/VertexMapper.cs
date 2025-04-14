using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CWM.Skinn
{
    [ExecuteInEditMode]
    [HelpURL(SkinnEx.AssetURLS.UASForum)]
    public class VertexMapper : MonoBehaviour
    {
        [ComponentHeader] public string header = "";

        [HighlightNull(false, true)]
        [Tooltip("the root gameObject of new skinned meshes." +
            "")]
        public GameObject root = null;

        [Space]
        [Tooltip("source of the vertex information." +
            "")]
        public ModelRefrence sourceAsset = new ModelRefrence();

        [Space]
        [Tooltip("target of the vertex information." +
            "")]
        public ModelRefrence targetAsset = new ModelRefrence();

        [Tooltip("how to transform the sources vertices before mapping the indices.")]
        [ContextMenuItem("Reset", "ResetSourceBaking")]
        public BakeOptions sourceBaking = new BakeOptions();

        [Tooltip("how to transform the targets vertices before mapping the indices.")]
        [ContextMenuItem("Reset", "ResetTargetBaking")]
        public BakeOptions targetBaking = new BakeOptions();

        [Tooltip("how to create a guide between the source and target.")]
        [ContextMenuItem("Reset", "ResetSearchOptions")]
        public SearchOptions search = new SearchOptions();

        [Tooltip("bone-weight projection options")]
        [ContextMenuItem("Reset", "ResetWeightProjection")]
        public WeightProjection weightProjection = new WeightProjection();

        [Tooltip("blend-shape projection options")]
        [ContextMenuItem("Reset", "ResetShapeProjection")]
        public ShapeProjection shapeProjection = new ShapeProjection();

        [Tooltip("preview source, target and mapping in skinning space.")]
        [ContextMenuItem("Reset", "ResetGizmos")]
        public PreviewSkinner gizmos = new PreviewSkinner();

        [HideInInspector] public ElementFilter boneFilter;
        [HideInInspector] public ElementFilter shapeFilter;
        [HideInInspector] public SkinnedMeshRenderer preveiwAsset;

        public SkinnedMeshRenderer Source { get { if (!sourceAsset) return null; return sourceAsset.GetSkinnedMeshRenderer; } }

        public Renderer Target { get { if (!targetAsset) return null; return targetAsset.GetRenderer; } }

        [NonSerialized] private MeshBinding Mapping = null;

        private void ResetSourceBaking() { sourceBaking.Reset(); }
        private void ResetTargetBaking() { targetBaking.Reset(); }
        private void ResetSearchOptions() { search.Reset(); }
        private void ResetWeightProjection() { weightProjection.Reset();}
        private void ResetShapeProjection() { shapeProjection.Reset(); }
        private void ResetGizmos() { gizmos.Reset(); }

        [ContextMenu("Reset Settings")]
        private void ResetSettings()
        {
            ResetSourceBaking();
            ResetTargetBaking();
            ResetSearchOptions();
            ResetWeightProjection();
            ResetShapeProjection();
            ResetGizmos();
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            gizmos.Enable();
            SkinnGizmos.OnEditorRelease -= gizmos.Release;
            SkinnGizmos.OnEditorRelease += gizmos.Release;
#endif
        }

        private void OnDrawGizmos() { if (gizmos.displayGizmos == GizmosDisplayMode.Always) DrawGizmos(); }
        private void OnDrawGizmosSelected() { if (gizmos.displayGizmos == GizmosDisplayMode.Selected) DrawGizmos(); }

        private void DrawGizmos()
        {
#if UNITY_EDITOR
            gizmos.Draw(Source, Target, Mapping, sourceBaking, targetBaking, search);
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            SkinnGizmos.OnEditorRelease -= gizmos.Release;
            gizmos.Release();
#endif
        }

        public bool CanCreateBinding
        {
            get
            {
                if (SkinnEx.IsNullOrNotVailid(Target) || SkinnEx.IsNullOrNotVailid(Source)) return false;
                else return true;
            }
        }

        public bool HasPreviewAsset
        {
            get
            {
                var mapping = Mapping;
                var source = Source;
                return preveiwAsset && mapping && mapping.indices.Length == preveiwAsset.sharedMesh.vertexCount &&
                    source && source.sharedMesh && mapping.otherVertexCount == source.sharedMesh.vertexCount;
            }
        }

        [ContextMenu("Additional Weight Smoothing")]
        public void AdditionalWeightSmoothing()
        {
            if (!HasPreviewAsset) return;

            BoneWeight[] boneWeights;
            preveiwAsset.SmoothWeights(out boneWeights, new SmoothingOption() { Multiplier = Mapping.distance}, 16);
            preveiwAsset.sharedMesh = preveiwAsset.sharedMesh.Clone() as Mesh;
            preveiwAsset.sharedMesh.boneWeights = boneWeights;
        }


        [ContextMenu("Create")]
        public void Create()
        {
            if (!CanCreateBinding) return;
            if (preveiwAsset && (preveiwAsset == Source || (Renderer)preveiwAsset == Target)) preveiwAsset = null;
            if (preveiwAsset) SkinnEx.ReleaseWithUndo(preveiwAsset.gameObject);
            SkinnedMeshRenderer smr;
            if(Create(Source, Target, out smr))
            {
#if UNITY_EDITOR
                var undoLabel = "VM Create" + SkinnEx.EnforceObjectName(smr);
                Undo.RecordObject(this, undoLabel);
                if (Target && SkinnEx.IsNullOrNotInAScene(Target)) { Undo.RecordObject(Target.gameObject, undoLabel); Target.gameObject.SetActive(false); }
#endif
                preveiwAsset = smr;
                return;
            }
            return;
        }

        public bool Create(Renderer Target, out SkinnedMeshRenderer smr)
        {
            smr = null;
            SkinnedMeshRenderer asset;
            if (Create(Source, Target, out asset)) { smr = asset; return true; }
            return false;
        }

        public bool Create(SkinnedMeshRenderer Source, Renderer Target, out SkinnedMeshRenderer smr)
        {
            smr = null;
            if (SkinnEx.IsNullOrNotVailid(Target) || SkinnEx.IsNullOrNotVailid(Source, true)) return false;
            if (SkinnEx.IsNullOrNotVailid(Target as SkinnedMeshRenderer)) weightProjection.enabled = true;
            if (this.root && (this.root == Source.gameObject || this.root == Target.gameObject)) this.root = null;

            var source = Source;
            var sourceMesh = source.sharedMesh;
            var target = Target;
            bool maskSet = search.sourceBoneMask != null;
            if (!maskSet) search.sourceBoneMask = boneFilter.GetBoneFilter(Source.bones);

            ShapeFilter filter;
            if (!ShapeFilter.Read(out filter, shapeFilter.elements)) { filter = null; }
            shapeProjection.ShapeMask = filter;

            int[] mapping;
            float[] distance;
            Vector3[] vertices;
            Vector3[] normals;
            Vector4[] tangents;
            BoneWeight[] boneWeights;
            Blendshape[] blendshapes;

            if (!source.VertexMap(target, out mapping,out distance, out vertices, out normals, out tangents, out boneWeights, out blendshapes,
                sourceBaking, targetBaking, search, weightProjection, shapeProjection))
            {
                if (Mapping)
                {
                    Mapping.indices = mapping;
                    Mapping.distance = distance;
                    Mapping.otherVertexCount = sourceMesh.vertexCount;
                }

                Debug.LogError("Mapping Failed.");
                return false;
            }

            if (!maskSet) search.sourceBoneMask = null;

            smr = target.CloneAsSkinnedMeshRenderer(true) as SkinnedMeshRenderer;
            var mesh = target.CloneSharedMesh();

            mesh.vertices = vertices;
            mesh.normals = SkinnEx.IsNullOrEmpty(mesh.normals) ? null : normals;
            mesh.tangents = SkinnEx.IsNullOrEmpty(mesh.tangents) ? null : tangents;

            Transform[] mergeBones;
            if (weightProjection.enabled)
            {
                mergeBones = source.bones;
                mesh.boneWeights = boneWeights;
                mesh.bindposes = source.bones.GetBindposes();
            }
            else
            {
                var targetSkinnedMesh = (target as SkinnedMeshRenderer);
                mergeBones = targetSkinnedMesh.bones;
                mesh.bindposes = targetSkinnedMesh.bones.GetBindposes();
            }

            var emptyDeltas = new Vector3[mesh.vertexCount];

            for (int i = 0; i < blendshapes.Length; i++)
            {
                var shape = blendshapes[i];
                for (int ii = 0; ii < shape.frames.Count; ii++)
                {
                    var keyframe = shape.frames[ii];
                    var deltaVertices = shapeProjection.vertex ? keyframe.shapeDeltas : emptyDeltas;
                    var deltaNormals = shapeProjection.normal ? keyframe.shapeNormals : null;
                    var deltaTangents = shapeProjection.tangent ? keyframe.shapeTangents : null;

                    mesh.AddBlendShapeFrame(shape.name, keyframe.frameWeight, deltaVertices , deltaNormals, deltaTangents);
                }
            }

            var root = this.root ? this.root : target.gameObject.Clone(false) as GameObject;
            var position = root.transform.position; var rotation = root.transform.rotation; var localScale = root.transform.localScale;
            Transform[] bones;
            List<Transform> newTransforms;
            root.transform.MergeCloneIfNeeded(mergeBones, out bones, out newTransforms);
            root.transform.position = position; root.transform.rotation = rotation; root.transform.localScale = localScale;

            smr.sharedMesh = mesh;
            smr.bones = bones;
            smr.rootBone = bones.HasTrueRootBone() ? bones.FindMinHierarchicalOrder() : null;

            smr.transform.SetParent(root.transform);
            smr.SetBounds();
            smr.quality = SkinQuality.Bone4;

            smr.SyncShapesFromRoot(root.transform);

#if UNITY_EDITOR
            if (root != this.root) newTransforms.Add(root.transform);
            newTransforms.Add(smr.transform);
            var undoLabel = "VM Create" + SkinnEx.EnforceObjectName(smr);
            foreach (var item in newTransforms) Undo.RegisterCreatedObjectUndo(item.gameObject, undoLabel);
            if (!Mapping) Mapping = new MeshBinding();
            Mapping.indices = mapping;
            Mapping.distance = distance;
            Mapping.otherVertexCount = sourceMesh.vertexCount;
#endif

            return true;
        }

        public void AppendToNew()
        {
            if (!HasPreviewAsset) return;
            var source = Source;
            GameObject newGameObject = new GameObject(preveiwAsset.name);
            Transform[] bones;
            List<Transform> newTransforms;
            newGameObject.transform.MergeCloneIfNeeded(source.bones, out bones, out newTransforms);
            newTransforms.Add(newGameObject.transform);

#if UNITY_EDITOR
            var undoLabel = "VM Append" + SkinnEx.EnforceObjectName(preveiwAsset);
            foreach (var item in newTransforms) Undo.RegisterCreatedObjectUndo(item.gameObject, undoLabel);
            Undo.RecordObject(preveiwAsset.gameObject, undoLabel);
            Undo.RecordObject(preveiwAsset, undoLabel);
            Undo.RecordObject(this, undoLabel);
#endif
            preveiwAsset.transform.SetParent(newGameObject.transform, false);
            preveiwAsset.transform.ResetLocal();
            preveiwAsset.bones = bones;
            preveiwAsset.rootBone = bones.HasTrueRootBone() ? bones.FindMinHierarchicalOrder() : null;
            preveiwAsset = null;
        }

    }
}