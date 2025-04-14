using UnityEngine;

namespace CWM.Skinn
{
    [System.Serializable]
    internal class LinearSkinner
    {
        public ComputeShader computeShader;
        public ComputeBuffer Vertices, Normals, Tangents, BoneWeights, BoneMatrices, OutVertices, OutNormals, OutTangents;
        public bool HasBuffers()
        {
            if (computeShader == null ||
                Vertices == null ||
                Normals == null ||
                Tangents == null ||
                BoneMatrices == null ||
                computeShader == null ||
                OutVertices == null ||
                OutNormals == null ||
                OutTangents == null
                ) return false;
            return true;
        }

        public int vertexCount,transformCount;
        public uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
        public int kernel, threadGroupsX;

        public bool GetSkinning(SkinnedMeshRenderer source,
            out Vector3[] vertices,
            out Vector3[] normals,
            out Vector4[] tangents,

            BakeOptions options = null,
            bool oneShot = true)
        {
            if (!SkinnEx.HasMinimumRequirements(source) || !SkinnInternalAsset.CanCompute())
            {
                vertices = null;  normals = null; tangents = null;
                Release();
                return false;
            }

            var sharedMesh = source.sharedMesh;
            var vertexCount = sharedMesh.vertexCount;
            var transformCount = sharedMesh.bindposes.Length;
            var bakingOptions = BakeOptions.FallbackToDefualtValue(options);

            vertices = sharedMesh.vertices;
            normals = sharedMesh.GetNormalsRecalculateIfNeeded();
            tangents = sharedMesh.GetTangentsRecalculateIfNeeded();

            if (bakingOptions.HasOverrides()) bakingOptions.GetOverrides(ref vertices, ref normals, ref tangents);

            var setupComputeShader = !HasBuffers() ? true : false;
            if (vertexCount != this.vertexCount || transformCount != this.transformCount || oneShot) setupComputeShader = true;

            if (setupComputeShader)
            {
                Release();

                this.vertexCount = vertexCount;
                this.transformCount = transformCount;

                computeShader = SkinnInternalAsset.Asset.linearSkinning;
                kernel = computeShader.FindKernel("LinearSkinPass");
                computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
                Vertices = new ComputeBuffer(vertexCount, 3 * sizeof(float));
                Normals = new ComputeBuffer(vertexCount, 3 * sizeof(float));
                Tangents = new ComputeBuffer(vertexCount, 4 * sizeof(float));
                BoneWeights = new ComputeBuffer(vertexCount, 4 * sizeof(float) + 4 * sizeof(int));
                BoneMatrices = new ComputeBuffer(transformCount, 16 * sizeof(float));
                OutVertices = new ComputeBuffer(vertexCount, 3 * sizeof(float));
                OutNormals = new ComputeBuffer(vertexCount, 3 * sizeof(float));
                OutTangents = new ComputeBuffer(vertexCount, 4 * sizeof(float));

                Vertices.SetData(vertices);
                Normals.SetData(normals);
                Tangents.SetData(tangents);
                BoneWeights.SetData(sharedMesh.boneWeights);

                OutVertices.SetData(vertices);
                OutNormals.SetData(normals);
                OutTangents.SetData(tangents);
            }

            if (BoneMatrices == null) 
            {
                vertices = null;
                normals = null;
                tangents = null;
                Release();
                return false;
            }

            var matrices = bakingOptions.GetMatrices(sharedMesh.bindposes, source.bones);
            if (SkinnEx.IsNullOrEmpty(matrices))
            {
                Release();
                return false;
            }
            else BoneMatrices.SetData(matrices);

            computeShader.SetInt("VertexCount", vertexCount);
            computeShader.SetBuffer(kernel, "Vertices", Vertices);
            computeShader.SetBuffer(kernel, "Normals", Normals);
            computeShader.SetBuffer(kernel, "Tangents", Tangents);
            computeShader.SetBuffer(kernel, "Weights", BoneWeights);
            computeShader.SetBuffer(kernel, "Bones", BoneMatrices);

            computeShader.SetBuffer(kernel, "OutVertices", OutVertices);
            computeShader.SetBuffer(kernel, "OutNormals", OutNormals);
            computeShader.SetBuffer(kernel, "OutTangents", OutTangents);

            computeShader.Dispatch(kernel, threadGroupsX, 1, 1);

            OutVertices.GetData(vertices);
            OutNormals.GetData(normals);
            OutTangents.GetData(tangents);

            if (oneShot) Release();

            return true;
        }

        public void Release()
        {
            SkinnEx.Release(Vertices); SkinnEx.Release(Normals); SkinnEx.Release(Tangents); SkinnEx.Release(BoneWeights);
            SkinnEx.Release(BoneMatrices); SkinnEx.Release(OutVertices); SkinnEx.Release(OutNormals); SkinnEx.Release(OutTangents);
        }

        public static implicit operator bool(LinearSkinner value) { return value != null; }

        public static LinearSkinner Defualt = new LinearSkinner();
    }

}
