using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {

        public static bool InflateDeltas(Vector3[] vertices, int[] triangles, out Vector3[] vectors, float inflate, bool deltaPass = false, int passes = 300)
        {
            vectors = null;
            if (IsNullOrEmpty(vertices) || IsNullOrEmpty(triangles) || !SkinnInternalAsset.Asset || !SkinnInternalAsset.Asset.HasShaders()) return false;

            ComputeShader computeShader = SkinnInternalAsset.Asset.laplacianFilter;
            ComputeBuffer Adjacency, Duplicates, InVectors, OutVectors;

            int vertexCount = vertices.Length;
            int[] adjacentIndices;

            if (!GetAdjacentIndices(triangles, vertices, out adjacentIndices, 16)) { Debug.LogError("computeShader error!"); }

            Adjacency = new ComputeBuffer(adjacentIndices.Length, sizeof(int));
            Adjacency.SetData(adjacentIndices);
            Duplicates = new ComputeBuffer(vertexCount, sizeof(int));
            Duplicates.SetData(vertices.GetUniqueIndices());
            InVectors = new ComputeBuffer(vertexCount, sizeof(float) * 3);
            InVectors.SetData(vertices);
            OutVectors = new ComputeBuffer(vertexCount, sizeof(float) * 3);
            OutVectors.SetData(vertices);

            computeShader.SetInt("VertexCount", vertexCount);
            computeShader.SetInt("AdjacentCount", 16);
            computeShader.SetFloat("Inflation", inflate);
            computeShader.SetBool("DeltaPass", deltaPass);

            {
                var kernel = computeShader.FindKernel("Vector3LaplacianPass");
                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                computeShader.SetBuffer(kernel, "Adjacency", Adjacency);
                computeShader.SetBuffer(kernel, "OutVectors", OutVectors);

                for (int i = 0; i < passes; i++) computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            {
                var kernel = computeShader.FindKernel("InflationPass");
                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                computeShader.SetBuffer(kernel, "InVectors", InVectors);
                computeShader.SetBuffer(kernel, "OutVectors", OutVectors);

                computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            {
                var kernel = computeShader.FindKernel("UniqueIndicesPass");
                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                computeShader.SetBuffer(kernel, "Duplicates", Duplicates);
                computeShader.SetBuffer(kernel, "OutVectors", OutVectors);

                computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            vectors = new Vector3[vertices.Length];
            OutVectors.GetData(vectors);

            Release(Adjacency); Release(Duplicates); Release(InVectors); Release(OutVectors);
            return true;
        }

        public static bool Inflate(Mesh mesh, out Vector3[] vertex, out Vector3[] normal, out Vector3[] tangent,
            float inflate,
            bool deltaPass = false,
            int passes = 300)
        {
            vertex = null;
            normal = null;
            tangent = null;

            if (IsNullOrNotVailid(mesh) || !SkinnInternalAsset.Asset || !SkinnInternalAsset.Asset.HasShaders()) return false;

            ComputeShader computeShader = SkinnInternalAsset.Asset.laplacianFilter;
            ComputeBuffer Adjacency, Duplicates, InVectors, OutVectors;

            var vertices = mesh.vertices;
            var normals = mesh.GetNormalsRecalculateIfNeeded();
            Vector3[] tangents;
            mesh.GetTangentsRecalculateIfNeeded().ConvertArray(out tangents);

            int vertexCount = vertices.Length;
            int[] adjacentIndices;

            if (!GetAdjacentIndices(mesh.triangles, vertices, out adjacentIndices, 16)) { Debug.LogError("computeShader error!"); }

            Adjacency = new ComputeBuffer(adjacentIndices.Length, sizeof(int));
            Adjacency.SetData(adjacentIndices);

            Duplicates = new ComputeBuffer(vertexCount, sizeof(int));
            Duplicates.SetData(vertices.GetUniqueIndices());

            InVectors = new ComputeBuffer(vertexCount, sizeof(float) * 3);
            OutVectors = new ComputeBuffer(vertexCount, sizeof(float) * 3);

            computeShader.SetInt("VertexCount", vertexCount);
            computeShader.SetInt("AdjacentCount", 16);
            computeShader.SetFloat("Inflation", inflate);
            computeShader.SetBool("DeltaPass", deltaPass);

            for (int i = 0; i < 3; i++)
            {
                switch (i)
                {
                    default:
                    case 0: { InVectors.SetData(vertices); OutVectors.SetData(vertices); } break;
                    case 1: { InVectors.SetData(normals); OutVectors.SetData(normals); } break;
                    case 2: { InVectors.SetData(tangents); OutVectors.SetData(tangents); } break;
                }

                {
                    var kernel = computeShader.FindKernel("Vector3LaplacianPass");
                    uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                    computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                    var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                    computeShader.SetBuffer(kernel, "Adjacency", Adjacency);
                    computeShader.SetBuffer(kernel, "OutVectors", OutVectors);

                    for (int ii = 0; ii < passes; ii++) computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
                }

                {
                    var kernel = computeShader.FindKernel("InflationPass");
                    uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                    computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                    var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                    computeShader.SetBuffer(kernel, "InVectors", InVectors);
                    computeShader.SetBuffer(kernel, "OutVectors", OutVectors);

                    computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
                }

                {
                    var kernel = computeShader.FindKernel("UniqueIndicesPass");
                    uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                    computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                    var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                    computeShader.SetBuffer(kernel, "Duplicates", Duplicates);
                    computeShader.SetBuffer(kernel, "OutVectors", OutVectors);

                    computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
                }

                switch (i)
                {
                    default:
                    case 0: { vertex = new Vector3[vertexCount]; OutVectors.GetData(vertex); } break;
                    case 1: { normal = new Vector3[vertexCount]; OutVectors.GetData(normal); } break;
                    case 2: { tangent = new Vector3[vertexCount]; OutVectors.GetData(tangent); } break;
                }
            }

            Release(Adjacency); Release(Duplicates); Release(InVectors); Release(OutVectors);
            return true;
        }


        internal static bool Inflate(Vector3[] vertices, int[] triangles, out Vector3[] vectors, float inflate, int[] adjacentIndices, int adjancentCount, int[] duplicates,
          bool deltaPass = false,
          int passes = 300)
        {
            vectors = null;
            if (IsNullOrEmpty(vertices) || IsNullOrEmpty(triangles) || !SkinnInternalAsset.Asset || !SkinnInternalAsset.Asset.HasShaders()) return false;

            ComputeShader computeShader = SkinnInternalAsset.Asset.laplacianFilter;
            ComputeBuffer Adjacency, Duplicates, InVectors, OutVectors;

            int vertexCount = vertices.Length;

            Adjacency = new ComputeBuffer(adjacentIndices.Length, sizeof(int));
            Adjacency.SetData(adjacentIndices);
            Duplicates = new ComputeBuffer(vertexCount, sizeof(int));
            Duplicates.SetData(duplicates);
            InVectors = new ComputeBuffer(vertexCount, sizeof(float) * 3);
            InVectors.SetData(vertices);
            OutVectors = new ComputeBuffer(vertexCount, sizeof(float) * 3);
            OutVectors.SetData(vertices);

            computeShader.SetInt("VertexCount", vertexCount);
            computeShader.SetInt("AdjacentCount", adjancentCount);
            computeShader.SetFloat("Inflation", inflate);
            computeShader.SetBool("DeltaPass", deltaPass);

            {
                var kernel = computeShader.FindKernel("Vector3LaplacianPass");
                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                computeShader.SetBuffer(kernel, "Adjacency", Adjacency);
                computeShader.SetBuffer(kernel, "OutVectors", OutVectors);

                for (int i = 0; i < passes; i++) computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            {
                var kernel = computeShader.FindKernel("InflationPass");
                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                computeShader.SetBuffer(kernel, "InVectors", InVectors);
                computeShader.SetBuffer(kernel, "OutVectors", OutVectors);

                computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            {
                var kernel = computeShader.FindKernel("UniqueIndicesPass");
                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                computeShader.SetBuffer(kernel, "Duplicates", Duplicates);
                computeShader.SetBuffer(kernel, "OutVectors", OutVectors);

                computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            vectors = new Vector3[vertexCount];
            OutVectors.GetData(vectors);

            Release(Adjacency); Release(Duplicates); Release(InVectors); Release(OutVectors);
            return true;
        }

    }
}
