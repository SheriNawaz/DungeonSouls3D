using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CWM.Skinn
{
    [System.Serializable]
    public class PreviewSkinner
    {
        public GizmosDisplayMode displayGizmos = GizmosDisplayMode.Selected;

        public bool drawSource, drawTarget, drawMapping, drawRange;
            
        public bool showMaterials;

        internal LinearSkinner sourceSkinner;
        internal LinearSkinner targetSkinner;

        public void Reset() { displayGizmos = 0; drawSource = false; drawTarget = false; drawMapping = false; drawRange = false; showMaterials = false; }

        public override string ToString()
        {
            return string.Format("Gizmos: source = {1}, target = {2}, map = {3}, range = {4}, mat = {4}"
                , drawSource, drawTarget, drawMapping, drawRange, showMaterials);
        }

        public void Enable()
        {
            if (!sourceSkinner) sourceSkinner = new LinearSkinner();
            if (!targetSkinner) targetSkinner = new LinearSkinner();
        }

        public void Release()
        {
            if (sourceSkinner) sourceSkinner.Release();
            if (targetSkinner) targetSkinner.Release();
            SkinnEx.Release(sourcePreview);
            SkinnEx.Release(targetPreview);
        }

        private Mesh sourcePreview;
        private Mesh GetSourcePreview(Mesh mesh)
        {
            if (sourcePreview && sourcePreview.CanApplyVertices(mesh)) return sourcePreview;
            SkinnEx.Release(sourcePreview);
            sourcePreview = mesh.Clone(HideFlags.HideAndDontSave) as Mesh;
            sourcePreview.MarkDynamic();
            return sourcePreview;
        }

        private Mesh targetPreview;
        private Mesh GetTargetPreview(Mesh mesh)
        {
            if (targetPreview && targetPreview.CanApplyVertices(mesh)) return targetPreview;
            if(!targetSkinner )
            SkinnEx.Release(targetPreview);
            targetPreview = mesh.Clone(HideFlags.HideAndDontSave) as Mesh;
            targetPreview.MarkDynamic();
            return targetPreview;
        }

        public void Draw(SkinnedMeshRenderer source, Renderer target, MeshBinding binding,
            BakeOptions bakeSource, BakeOptions bakeTarget, SearchOptions search)
        {
            Enable();

#if UNITY_EDITOR
            if (EditorApplication.isCompiling || (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)) return;
#endif
            if (!drawSource && !drawTarget && !drawMapping && !drawMapping && !drawRange) return;

            if (!SkinnEx.HasMinimumRequirements(source)) return;

            Vector3[] sourceVertices;
            Vector3[] sourceNormals;
            Vector4[] sourceTangents;

            BakeOptions sourceBaking = BakeOptions.FallbackToDefualtValue(bakeSource);
            if (!sourceSkinner.GetSkinning(source, out sourceVertices, out sourceNormals, out sourceTangents, sourceBaking, false)) return;
            if (drawSource)
            {
                var sourcePMesh = GetSourcePreview(source.sharedMesh);
                if (sourceBaking.vertex) sourcePMesh.vertices = sourceVertices;
                else sourcePMesh.vertices = source.sharedMesh.vertices;
                if (sourceBaking.normal) sourcePMesh.normals = sourceNormals;
                else sourcePMesh.normals = source.sharedMesh.normals;

                Bounds bounds = sourcePMesh.bounds;
                sourcePMesh.bounds = bounds;

                GL.PushMatrix();
                GL.MultMatrix(Matrix4x4.identity);
                var materials = source.GetSharedMaterialsFromSubMeshes();
                for (int i = 0; i < sourcePMesh.subMeshCount; i++)
                {
                    var mat = materials[i];
                    if (showMaterials && mat && mat.shader && mat.passCount > 0) mat.SetPass(0);
                    else SkinnGizmos.GizmoMaterial.SetPass(0);
                    Graphics.DrawMeshNow(sourcePMesh, Matrix4x4.identity, i);
                }
                GL.PopMatrix();
            }

            if (!SkinnEx.HasMinimumRequirements(target)) return;

            BakeOptions targetOptions = BakeOptions.FallbackToDefualtValue(bakeTarget);

            var targetSharedMesh = target.GetSharedMesh();

            Vector3[] targetVertices = targetSharedMesh.vertices;
            Vector3[] targetNormals = targetSharedMesh.normals;
            Vector4[] targetTangents = targetSharedMesh.tangents;

            var targetSkinnedMesh = target as SkinnedMeshRenderer;
            if (targetSkinnedMesh && SkinnEx.HasMinimumRequirements(targetSkinnedMesh))
            {
                if (!targetSkinner.GetSkinning(targetSkinnedMesh, out targetVertices, out targetNormals, out targetTangents, targetOptions, false)){ return; }
            }
            else if(targetOptions.world)
            {
                if (targetOptions.vertex && targetSharedMesh.vertices.Transformed(out targetVertices, target.transform.localToWorldMatrix, false)) { }
                if (targetOptions.normal && !SkinnEx.IsNullOrEmpty(targetSharedMesh.normals)
                    && targetSharedMesh.normals.Transformed(out targetNormals, target.transform.localToWorldMatrix, true)) { }
            }

            if (drawTarget)
            {
                var targetPMesh = GetTargetPreview(targetSharedMesh);

                if (targetOptions.vertex) targetPMesh.vertices = targetVertices;
                else targetPMesh.vertices = targetSharedMesh.vertices;
                if (targetOptions.normal) targetPMesh.normals = targetNormals;
                else targetPMesh.normals = targetSharedMesh.normals;

                Bounds bounds = targetPMesh.bounds;
                targetPMesh.bounds = targetSharedMesh.bounds;

                GL.PushMatrix();
                GL.MultMatrix(Matrix4x4.identity);
                var materials = target.GetSharedMaterialsFromSubMeshes();
                for (int i = 0; i < targetPMesh.subMeshCount; i++)
                {
                    var mat = materials[i];
                    if (showMaterials  && mat && mat.shader && mat.passCount > 0) mat.SetPass(0);
                    else SkinnGizmos.GizmoMaterial.SetPass(0);
                    Graphics.DrawMeshNow(targetPMesh, Matrix4x4.identity, i);
                }
                GL.PopMatrix();
            }

            var sourceVertexCount = sourceVertices.Length;
            var targetVertexCount = targetVertices.Length;

            var showMapping = drawMapping && binding && !SkinnEx.IsNullOrEmpty(binding.indices, targetVertices.Length) &&
             !SkinnEx.IsNullOrEmpty(binding.distance, targetVertices.Length);

            var distance = search.MaxDistance;
            var showDistance = drawRange && search.maxDistance != 0;

            if (!showDistance && !showMapping) return;

            GL.PushMatrix();
            SkinnGizmos.GizmoMaterial.SetPass(0);
            GL.MultMatrix(Matrix4x4.identity);

            var errorDisplaySize = 0.1f.ToUnitFromStandard(ConversionUnits.Inch);
            var maxPreview = Mathf.Clamp(targetVertexCount, 0, 10000);

            var gizmoColor = Gizmos.color;
            var rangeColor = SkinnInternalAsset.GetColors.rangeColor;
            Gizmos.color = rangeColor;
            for (int i = 0; i < maxPreview; i++)
            {
                if (showDistance) Gizmos.DrawWireSphere(targetVertices[i], distance);

                if (showMapping)
                {
                    var index = binding.indices[i];
                    if (index > -1 && index < sourceVertexCount)
                    {
                        GL.Begin(GL.LINES);
                        var pow = (Mathf.Clamp01(binding.distance[i]));
                        var power = Color.Lerp(Color.black, Color.white, pow);
                        GL.Color(power);
                        GL.Vertex(targetVertices[i]);
                        GL.Color(power);
                        GL.Vertex(sourceVertices[index]);
                        GL.End();
                    }
                    else
                    {
                        GL.Begin(GL.LINES);
                        GL.Color(Color.magenta);
                        Matrix4x4 matrix4X4 = Camera.current ? Camera.current.worldToCameraMatrix : Matrix4x4.identity;
                        GL.Vertex(targetVertices[i] + matrix4X4.MultiplyVector(Vector3.up) * errorDisplaySize);
                        GL.Vertex(targetVertices[i] + matrix4X4.MultiplyVector(Vector3.left) * errorDisplaySize);
                        GL.Vertex(targetVertices[i] + matrix4X4.MultiplyVector(Vector3.down) * errorDisplaySize);
                        GL.Vertex(targetVertices[i] + matrix4X4.MultiplyVector(Vector3.right) * errorDisplaySize);
                        GL.Vertex(targetVertices[i] + matrix4X4.MultiplyVector(Vector3.up) * errorDisplaySize);
                        GL.End();
                    }
                }

            }

            GL.PopMatrix();
            Gizmos.color = gizmoColor;
        }


        public static implicit operator bool(PreviewSkinner skinner) { return skinner != null; }
    }
}
