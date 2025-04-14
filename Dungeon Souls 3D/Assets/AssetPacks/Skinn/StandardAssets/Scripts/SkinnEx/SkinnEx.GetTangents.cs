using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public static Vector4[] GetTangentsRecalculateIfNeeded(this Mesh mesh)
        {
            if (!mesh) return new Vector4[0];
            if (!IsNullOrEmpty(mesh.tangents)) return mesh.tangents;
            else return GetTangentsRecalculated(mesh);
        }

        public static Vector4[] GetTangentsRecalculated(this Mesh mesh)
        {
            if (!mesh) return new Vector4[0];
            Vector4[] tangents = mesh.tangents;
            Vector3[] normals = mesh.normals;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            Vector4[] newTangents = mesh.tangents;
            mesh.normals = IsNullOrEmpty(normals, mesh.vertices.Length) ? null : normals;
            mesh.tangents = IsNullOrEmpty(tangents, mesh.vertices.Length) ? null : tangents;
            return newTangents;
        }
    }
}
