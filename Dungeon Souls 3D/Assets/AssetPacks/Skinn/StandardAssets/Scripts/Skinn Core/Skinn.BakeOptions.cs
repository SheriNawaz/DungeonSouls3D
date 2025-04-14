using UnityEngine;

namespace CWM.Skinn
{
    [System.Serializable]
    public class BakeOptions
    {
        [Tooltip("calculate baking in world space or local space.")]
        public bool world;
        [Tooltip("transform vertices.")]
        public bool vertex;
        [Tooltip("transform normals.")]
        public bool normal;
        [Tooltip("transform tangents.")]
        public bool tangent;

        /// <summary>
        /// overrides the vertices before baking. the length must match the skins bone-weight to be used.
        /// </summary>
        public Vector3[] OverrideVertices { get; set; }

        /// <summary>
        /// overrides the normals before baking. the length must match the skins bone-weight to be used.
        /// </summary>
        public Vector3[] OverrideNormals { get; set; }

        /// <summary>
        /// overrides the normals before baking. the length must match the skins bone-weight to be used.
        /// </summary>
        public Vector4[] OverrideTangents { get; set; }

        public BakeOptions() { Reset(); }

        public void Reset()
        {
            world = true; vertex = true; normal = true; tangent = true;
            OverrideVertices = null; OverrideNormals = null; OverrideTangents = null;
        }

        public bool HasOverrides()
        {
            if (SkinnEx.IsNullOrEmpty(OverrideVertices) && SkinnEx.IsNullOrEmpty(OverrideNormals)
                && SkinnEx.IsNullOrEmpty(OverrideTangents)) return false;
            return true;
        }

        public bool GetOverrides(ref Vector3[] vertices, ref Vector3[] normals, ref Vector4[] tangents)
        {
            var overridden = false;
            var overrideV = SkinnEx.IsNullOrEmpty(OverrideVertices, vertices.Length) ? new Vector3[0] : OverrideVertices;
            if (vertices.Length == overrideV.Length) { overridden = true; vertices = overrideV; }
            var overrideN = SkinnEx.IsNullOrEmpty(OverrideNormals, normals.Length) ? new Vector3[0] : OverrideNormals;
            if (normals.Length == overrideN.Length) { overridden = true; normals = overrideN; }
            var overrideT = SkinnEx.IsNullOrEmpty(OverrideTangents, tangents.Length) ? new Vector4[0] : OverrideTangents;
            if (tangents.Length == overrideT.Length) { overridden = true; tangents = overrideT; }
            return overridden;
        }

        public override string ToString()
        {
            return string.Format("Baking: world = {0}, vertex = {1}, normal = {2}, tangent = {3}", world, vertex, normal, tangent);
        }

        public static implicit operator bool(BakeOptions value) { return value != null; }
        public static readonly BakeOptions DefaultValue = new BakeOptions();
        public static BakeOptions FallbackToDefualtValue(BakeOptions source) { if (!source) return BakeOptions.DefaultValue; else return source; }
    }
}