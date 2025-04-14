using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public static void BakeAndBindpose(this SkinnedMeshRenderer context, bool worldSpace = true)
        {
            if (!HasMinimumRequirements(context, true)) return;

            var shapes = context.BindposedBlendShapes();
            var mesh = context.BakeCustom(worldSpace, false, false, false);
            mesh.ClearBlendShapes();
            var vertexCount = mesh.vertexCount;
            for (int i = 0; i < shapes.Length; i++)
            {
                var shape = shapes[i];
                for (int ii = 0; ii < shape.frames.Count; ii++)
                {
                    var keyframe = shape.frames[ii];
                    mesh.AddBlendShapeFrame(shape.name, keyframe.frameWeight, keyframe.GetDeltas(vertexCount), keyframe.GetNormals(vertexCount), keyframe.GetTangents(vertexCount));
                }
            }

            var bindpose = context.bones.GetBindposes(worldSpace);
            mesh.bindposes = bindpose;
            context.sharedMesh = mesh;
            context.rootBone = context.bones.HasTrueRootBone() ? context.bones.FindMinHierarchicalOrder() : null;;
            context.bones = context.bones;
        }

        public static bool GetSkinning(this SkinnedMeshRenderer source, out Vector3[] vertices, out Vector3[] normals, out Vector4[] tangents, BakeOptions options = null)
        {
            if (!HasMinimumRequirements(source)) { vertices = null; normals = null; tangents = null; return false; }
            var skinner = new LinearSkinner();
            Vector3[] bakedVertices, bakedNormals; Vector4[] bakedTangents;
            if (!skinner.GetSkinning(source, out bakedVertices, out bakedNormals, out bakedTangents, options, true))
            {
                vertices = null; normals = null; tangents = null; return false;
            }
            vertices = bakedVertices;
            normals = bakedNormals;
            tangents = bakedTangents;
            return true;
        }

        public static Mesh BakeCustom(this Renderer source, bool worldSpace, bool removeBoneWeights, bool bakeBlendShapes, bool removeBlendshapes)
        {
            if (IsNullOrNotVailid(source)) return null;
            var mesh = source.GetSharedMesh().Clone() as Mesh;
            var meshInfo = new MeshVertexInfo(mesh);
            var options = new BakeOptions() { world = worldSpace };

            Vector3[] vertices, normals; Vector4[] tangents;

            if (source as SkinnedMeshRenderer)
            {
                var skinnedMeshRenderer = source as SkinnedMeshRenderer;
                if (skinnedMeshRenderer.HasMinimumRequirements())
                {
                    if(bakeBlendShapes) skinnedMeshRenderer.BakeBlendshapes(mesh, removeBlendshapes);
                    options.OverrideVertices = mesh.vertices;
                    options.OverrideNormals = mesh.normals;
                    options.OverrideTangents = mesh.tangents;

                    var skinner = new LinearSkinner();
                    if (skinner.GetSkinning(skinnedMeshRenderer, out vertices, out normals, out tangents, options, true))
                    {
                        mesh.vertices = vertices;
                        if(meshInfo.hasNormals) mesh.normals = normals;
                        if (meshInfo.hasTangents) mesh.tangents = tangents;
                        mesh.RecalculateBounds();
                    }
                }
                else 
                {
                    if (bakeBlendShapes) skinnedMeshRenderer.BakeBlendshapes(mesh, removeBlendshapes);

                    if (worldSpace)
                    {
                        if (mesh.vertices.Transformed(out vertices, source.transform.localToWorldMatrix, false))
                        { mesh.vertices = vertices; }

                        if (meshInfo.hasNormals)
                        { if(mesh.normals.Transformed(out normals, source.transform.localToWorldMatrix, true)) { mesh.normals = normals; }}

                        if (meshInfo.hasTangents)
                        { if (mesh.tangents.Transformed(out tangents, source.transform.localToWorldMatrix, true)) { mesh.tangents = tangents; } }

                        mesh.RecalculateBounds();
                    }
                }
            }
            else
            {
                if (worldSpace)
                {
                    if (mesh.vertices.Transformed(out vertices, source.transform.localToWorldMatrix, false))
                    { mesh.vertices = vertices; }

                    if (meshInfo.hasNormals)
                    { if (mesh.normals.Transformed(out normals, source.transform.localToWorldMatrix, true)) { mesh.normals = normals; } }

                    if (meshInfo.hasTangents)
                    { if (mesh.tangents.Transformed(out tangents, source.transform.localToWorldMatrix, true)) { mesh.tangents = tangents; } }

                    mesh.RecalculateBounds();
                }
            }
            if (removeBoneWeights) { mesh.boneWeights = null; mesh.bindposes = null; }
            return mesh;
        }

        public static MeshFilter BakeToMeshFilter(this SkinnedMeshRenderer source, bool removeBoneWieghts = false, string name = "")
        {
            var gameObjectName = string.IsNullOrEmpty(name) ? EnforceObjectName(source) : name;
            var newGo = new GameObject(gameObjectName);
            var mf = newGo.AddComponent<MeshFilter>();
            var mr = newGo.AddComponent<MeshRenderer>();
            if (source)
            {
                mf.sharedMesh = source.BakeCustom(true, removeBoneWieghts, true, true);
                mr.sharedMaterials = source.sharedMaterials;
            }
            return mf;
        }

        private static MeshFilter BakeToMeshFilterUnity(this SkinnedMeshRenderer source, string name = "")
        {
            if (source == null || !HasMinimumRequirements(source)) return null;

            GameObject newGo = new GameObject(string.IsNullOrEmpty(name) ? "BakedMesh" : name);
            Mesh mesh = new Mesh();

            Transform p = source.transform.parent;
            Vector3 pos = source.transform.localPosition;
            Quaternion rot = source.transform.localRotation;
            Vector3 scl = source.transform.localScale;

            source.transform.SetParent(null);
            source.transform.localPosition = Vector3.zero;
            source.transform.localRotation = Quaternion.identity;
            source.transform.localScale = Vector3.one;

            source.BakeMesh(mesh);
            mesh.name = source.sharedMesh.name;

            source.transform.SetParent(p);
            source.transform.localPosition = pos;
            source.transform.localRotation = rot;
            source.transform.localScale = scl;

            MeshFilter mf = newGo.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = newGo.AddComponent<MeshRenderer>();
            mr.sharedMaterials = source.sharedMaterials;
            return mf;
        }
    }
}