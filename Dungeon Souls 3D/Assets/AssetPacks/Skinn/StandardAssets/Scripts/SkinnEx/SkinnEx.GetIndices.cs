using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        /// <summary>
        /// returns a parallel array with the index of the first occurrence of each vector.
        /// </summary>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static int[] GetUniqueIndices(this Vector3[] vectors)
        {
            var indices = new int[vectors.Length];
            var uniqueVectors = new Dictionary<Vector3, int>(vectors.Length);
            for (int i = 0; i < vectors.Length; i++) if (!uniqueVectors.ContainsKey(vectors[i])) uniqueVectors.Add(vectors[i], i);
            for (int i = 0; i < vectors.Length; i++) { int ii; if (uniqueVectors.TryGetValue(vectors[i], out ii)) indices[i] = ii; }
            return indices;
        }

        public static int[] GetSubMeshIndices(this Mesh mesh)
        {
            if (!mesh) return new int[0];
            if (mesh.subMeshCount == 1) return new int[mesh.vertexCount];

            int[] subMeshIndex = new int[mesh.vertexCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] tris = mesh.GetTriangles(i);
                for (int ii = 0; ii < tris.Length; ii++)
                {
                    subMeshIndex[tris[ii]] = i;
                }
            }
            return subMeshIndex;
        }

        public static bool GetRadialWeights(Vector3[] vertices, out Vector2[] indices, int count = 16)
        {
            if (IsNullOrEmpty(vertices) || !SkinnInternalAsset.Asset || !SkinnInternalAsset.Asset.HasShaders())
            {
                indices = null;
                return false;
            }

            ComputeShader computeShader = SkinnInternalAsset.Asset.radialIndices;

            uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
            int threadGroupsX;

            ComputeBuffer Vertices, Duplicates, RadialIndices;

            var vertexCount = vertices.Length;
            Vertices = new ComputeBuffer(vertexCount, sizeof(float) * 3);
            Vertices.SetData(vertices);
            Duplicates = new ComputeBuffer(vertexCount, sizeof(int));
            Duplicates.SetData(vertices.GetUniqueIndices());

            int radialCount = Mathf.Clamp(count, 1, 1000);
            indices = new Vector2[vertexCount * radialCount];
            RadialIndices = new ComputeBuffer(indices.Length, sizeof(float) * 2);
            RadialIndices.SetData(indices);

            computeShader.SetInt("VertexCount", vertexCount);
            computeShader.SetInt("RadialCount", radialCount);

            {
                int kernal = computeShader.FindKernel("RadialPass");
              
                computeShader.SetBuffer(kernal, "Vertices", Vertices);
                computeShader.SetBuffer(kernal, "Duplicates", Duplicates);
                computeShader.SetBuffer(kernal, "RadialIndices", RadialIndices);

                computeShader.GetKernelThreadGroupSizes(kernal, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                computeShader.Dispatch(kernal, threadGroupsX, 1, 1);
            }

            {
                int kernal = computeShader.FindKernel("RadialWeightPass");

                computeShader.SetBuffer(kernal, "Vertices", Vertices);
                computeShader.SetBuffer(kernal, "RadialIndices", RadialIndices);

                computeShader.GetKernelThreadGroupSizes(kernal, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
                computeShader.Dispatch(kernal, threadGroupsX, 1, 1);
            }
           
            RadialIndices.GetData(indices);

            Release(Vertices); Release(Duplicates); Release(RadialIndices);
            return true;
        }


        public static bool GetAdjacentIndices(this Mesh mesh, out int[,] indices, int count = 16, float minDistance = 1e-04f)
        {
            indices = null;
            if (!mesh) return false;
            int[] adjacentIndices;
            if (!mesh.GetAdjacentIndices(out adjacentIndices, count)) return false;
            var adjacencyData = new AdjacencyData { indices = adjacentIndices, adjacentCount = count, vertexCount = mesh.vertexCount };
            indices = adjacencyData;
            return true;
        }

        public static bool GetAdjacentIndices(int[] triangles, Vector3[] vertices, out int[,] indices, int count = 16, float minDistance = 1e-04f)
        {
            indices = null;
            if (IsNullOrEmpty(triangles) || IsNullOrEmpty(vertices)) return false;
            int[] adjacentIndices;
            if (!GetAdjacentIndices(triangles, vertices, out adjacentIndices, count)) return false;
            var adjacencyData = new AdjacencyData { indices = adjacentIndices, adjacentCount = count, vertexCount = vertices.Length };
            indices = adjacencyData;
            return true;
        }

        public static bool GetAdjacentIndices(this Mesh source, out int[] indices, int count = 16, float minDistance = 1e-04f)
        {
            if (source == null || !source.HasMinimumRequirements() || !SkinnInternalAsset.Asset || !SkinnInternalAsset.Asset.HasShaders())
            {
                indices = null;
                return false;
            }

            int[] getIndices;
            if(!GetAdjacentIndices(source.triangles, source.vertices, out getIndices, count, minDistance))
            {
                indices = null;
                return false;
            }
            indices = getIndices;
            return true;
        }

        public static bool GetAdjacentIndices(int[] triangles, Vector3[] vertices, out int[] indices,
            int count = 16,
            float minDistance = 1e-04f)
        {
            if (IsNullOrEmpty(triangles) || IsNullOrEmpty(vertices) || !SkinnInternalAsset.Asset || !SkinnInternalAsset.Asset.HasShaders())
            {
                indices = null;
                return false;
            }

            ComputeShader computeShader = SkinnInternalAsset.Asset.adjacentIndices;

            uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
            int threadGroupsX, threadGroupsY;

            ComputeBuffer Vertices, VertexIndices, Adjacency;

            var vertexCount = vertices.Length;
            Vertices = new ComputeBuffer(vertexCount, sizeof(float) * 3);
            Vertices.SetData(vertices);
            var vertexIndices = new int[vertexCount];
            VertexIndices = new ComputeBuffer(vertexCount, sizeof(int));
            VertexIndices.SetData(vertexIndices);

            var adjacencyLength = vertexCount * count;
            indices = new int[adjacencyLength];

            Adjacency = new ComputeBuffer(adjacencyLength, sizeof(int));
            Adjacency.SetData(indices);

            computeShader.SetFloat("MinDistance", 1e-4f);
            computeShader.SetInt("VertexCount", vertexCount);
            computeShader.SetInt("AdjacentCount", count);

            {
                int kernal = computeShader.FindKernel("PreDistancePass");
                computeShader.SetBuffer(kernal, "VertexIndices", VertexIndices);
                computeShader.GetKernelThreadGroupSizes(kernal, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
                computeShader.Dispatch(kernal, threadGroupsX, 1, 1);
            }

            {
                int kernal = computeShader.FindKernel("DistancePass");
                computeShader.SetBuffer(kernal, "Vertices", Vertices);
                computeShader.SetBuffer(kernal, "VertexIndices", VertexIndices);
                computeShader.GetKernelThreadGroupSizes(kernal, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
                threadGroupsY = (vertexCount + (int)threadGroupSizeY - 1) / (int)threadGroupSizeY;
                computeShader.Dispatch(kernal, threadGroupsX, threadGroupsY, 1);
            }


            {
                int kernal = computeShader.FindKernel("AdjacencyPass");
                computeShader.SetBuffer(kernal, "Adjacency", Adjacency);
                computeShader.GetKernelThreadGroupSizes(kernal, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
                computeShader.Dispatch(kernal, threadGroupsX, 1, 1);
            }

            {
                Adjacency.GetData(indices);
                VertexIndices.GetData(vertexIndices);
                indices = AdjacencyData.FillAdjacency(indices, vertexIndices, triangles, count);
            }

            {
                Adjacency.SetData(indices);

                int kernal = computeShader.FindKernel("FinalPass");
                computeShader.SetBuffer(kernal, "VertexIndices", VertexIndices);
                computeShader.SetBuffer(kernal, "Adjacency", Adjacency);
                computeShader.GetKernelThreadGroupSizes(kernal, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
                computeShader.Dispatch(kernal, threadGroupsX, 1, 1);
            }

            Adjacency.GetData(indices);
            Release(Vertices); Release(VertexIndices); Release(Adjacency);
            return true;
        }
        

        public struct AdjacencyData
        {
            public int vertexCount;
            public int adjacentCount;
            public int[] indices;

            public static implicit operator bool(AdjacencyData value) { return value.indices != null && value.indices.Length > 0; }

            public static implicit operator AdjacencyData(int[,] value)
            {
                if (value == null || value.Length < 1) new AdjacencyData();
                AdjacencyData matrix = new AdjacencyData() { };
                matrix.vertexCount = value.GetLength(0);
                matrix.adjacentCount = value.GetLength(1);
                matrix.indices = new int[matrix.vertexCount * matrix.adjacentCount];
                System.Buffer.BlockCopy(value, 0, matrix.indices, 0, matrix.indices.Length * sizeof(int));
                return matrix;
            }

            public static implicit operator int[,] (AdjacencyData value)
            {
                if (value.indices == null || value.indices.Length < 1) return new int[0, 0];
                var matrix = new int[value.vertexCount, value.adjacentCount];
                System.Buffer.BlockCopy(value.indices, 0, matrix, 0, value.indices.Length * sizeof(int));
                return matrix;
            }

            public static implicit operator int[] (AdjacencyData value)
            {
                if (value.indices == null || value.indices.Length < 1) return new int[0];
                return value.indices;
            }

            public static AdjacencyData Create(int[] array, int vertexCount, int adjacentCount)
            {
                return new AdjacencyData() { indices = array, vertexCount = vertexCount, adjacentCount = adjacentCount };
            }

            public static int[] FillAdjacency(int[] adjacency, int[] uniqueIndices, int[] triangles, int adjacentCount)
            {
                int[,] matrix = new AdjacencyData() { indices = adjacency, vertexCount = uniqueIndices.Length, adjacentCount = adjacentCount };
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    AddEdge(ref matrix, uniqueIndices, triangles[i + 0], triangles[i + 1]);
                    AddEdge(ref matrix, uniqueIndices, triangles[i + 0], triangles[i + 2]);
                    AddEdge(ref matrix, uniqueIndices, triangles[i + 1], triangles[i + 2]);
                }
                AdjacencyData adjacencyData = matrix;
                return adjacencyData.indices;
            }

            private static void AddEdge(ref int[,] matrix, int[] uniqueIndices, int a, int b)
            {
                var u0 = uniqueIndices[a];
                var u1 = uniqueIndices[b];

                AddVertex(ref matrix, u0, u1);
                AddVertex(ref matrix, u1, u0);
            }

            private static void AddVertex(ref int[,] matrix, int a, int b)
            {
                var length = matrix.GetLength(1);
                for (int i = 0; i < length; i++)
                {
                    if (matrix[a, i] == b) break;
                    if (matrix[a, i] == -1)
                    {
                        matrix[a, i] = b;
                        break;
                    }
                }
            }
        }
    }
}
