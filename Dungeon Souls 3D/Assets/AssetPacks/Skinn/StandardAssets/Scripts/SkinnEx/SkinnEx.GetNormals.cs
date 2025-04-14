using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public static Vector3[] GetNormalsRecalculateIfNeeded(this Mesh mesh)
        {
            if (!mesh) return new Vector3[0];
            if (!IsNullOrEmpty(mesh.normals)) return mesh.normals;
            else return GetNormalsRecalculated(mesh);
        }

        public static Vector3[] GetNormalsRecalculated(this Mesh mesh)
        {
            if (!mesh) return new Vector3[0];
            bool hasNormals = !IsNullOrEmpty(mesh.normals, mesh.vertices.Length);
            Vector3[] normals = mesh.normals;
            mesh.RecalculateNormals();
            Vector3[] newNormals = mesh.normals;
            mesh.normals = !hasNormals ? null : normals;
            return newNormals;
        }
    }
}
