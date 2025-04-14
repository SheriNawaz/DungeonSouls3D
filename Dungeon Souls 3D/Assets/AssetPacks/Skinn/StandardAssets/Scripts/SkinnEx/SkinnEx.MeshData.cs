using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        internal class MeshVertexInfo
        {
            public int vertexCount;
            public int triangleCount;
            public string fullName;

            public bool hasNormals, hasTangents, hasColors, hasUV, hasUV2, hasUV3, hasUV4, hasUV5, hasUV6, hasUV7, hasUV8,
               hasBoneWeights, hasBindposes, hasBlendshapes, hasTriangles;

            public MeshVertexInfo() { Reset(); }

            public MeshVertexInfo(Mesh mesh) { Reset(); Add(mesh); }

            public void Add(Mesh mesh)
            {
                if (!mesh) return;
                if (IsNullOrEmpty(mesh.vertices)) return;
                int vertexCount = mesh.vertexCount;
                if (!IsNullOrEmpty(mesh.normals, vertexCount)) hasNormals = true;
                if (!IsNullOrEmpty(mesh.tangents, vertexCount)) hasTangents = true;
                if (!IsNullOrEmpty(mesh.colors32, vertexCount)) hasColors = true;
                if (!IsNullOrEmpty(mesh.uv, vertexCount)) hasUV = true;
                if (!IsNullOrEmpty(mesh.uv2, vertexCount)) hasUV2 = true;
                if (!IsNullOrEmpty(mesh.uv3, vertexCount)) hasUV3 = true;
                if (!IsNullOrEmpty(mesh.uv4, vertexCount)) hasUV4 = true;


#if UNITY_2018_2_OR_NEWER
                if (!IsNullOrEmpty(mesh.uv5, vertexCount)) hasUV5 = true;
                if (!IsNullOrEmpty(mesh.uv6, vertexCount)) hasUV6 = true;
                if (!IsNullOrEmpty(mesh.uv7, vertexCount)) hasUV7 = true;
                //if (!IsNullOrEmpty(mesh.uv8, vertexCount)) hasUV8 = true;
#endif

                if (!IsNullOrEmpty(mesh.boneWeights, vertexCount)) hasBoneWeights = true;
                if (!IsNullOrEmpty(mesh.bindposes) && mesh.bindposes.Length > 0) hasBindposes = true;
                if (mesh.blendShapeCount > 0) hasBlendshapes = true;
                this.vertexCount += vertexCount;
                if (!IsNullOrEmpty(mesh.triangles)) { hasTriangles = true; triangleCount += mesh.triangles.Length; };

                fullName += string.Format(".{0}", EnforceString(mesh.name));
            }

            public void Add(Renderer source) { if (!source) return; else Add(source.GetSharedMesh()); }

            public void Reset()
            {
                vertexCount = 0;
                triangleCount = 0;
                fullName = "";
                hasNormals = false; hasTangents = false; hasColors = false; hasUV = false; hasUV2 = false; hasUV3 = false;
                hasUV4 = false; hasUV5 = false; hasUV6 = false; hasUV7 = false; hasUV8 = false;
                hasBoneWeights = false; hasBindposes = false; hasBlendshapes = false; hasTriangles = false;
            }

            public static implicit operator bool(MeshVertexInfo value) { return value != null; }
        }
    }
}