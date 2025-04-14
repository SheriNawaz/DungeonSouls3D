using UnityEngine;

namespace CWM.Skinn
{

    [System.Serializable]
    public class SearchOptions
    {
        public ConversionUnits units;

        [Tooltip(
            "the max distance to index vertices from the source." +
            " vertices outside this range will fall back to vertices with-in range." +
            " if no vertices are in range, maxDistance will be ignored!")]
        [ClampAbs] public float maxDistance;

        [Space]

        [Tooltip("try to compare sub-meshes of the source and target.")]
        public bool compareSubmesh;
        [Tooltip("try to compare the uvs of the source and target.")]
        public bool compareUV;
        [Tooltip("try to compare the normals of the source and target.")]
        public bool compareNormals;
        [Tooltip("try to compare the tangents of the source and target.")]
        public bool compareTangents;
        [Tooltip("try to compare the bone-weights of the source and target.")]
        public bool compareWeights;

        public float WeightTolerance { get { return 1f; } }

        public int[] sourceBoneMask { get; set; }

        public int[] targetBoneMask { get; set; }

        public void Reset()
        {
            units = ConversionUnits.Inch;
            maxDistance = 0f;

            compareSubmesh = false;
            compareUV = false;
            compareNormals = false;
            compareWeights = false;
        }

        public SearchOptions() { Reset(); }

        public float MaxDistance { get { return maxDistance.ToUnitFromStandard(units); } }

        public override string ToString()
        {
            return string.Format("Search: dis {0}; Compare: sub-mesh = {1}, uv = {2}, normal = {3}, tangent = {4}, weight = {5}"
                , maxDistance > 0.0 ? maxDistance.ToString("0.0") + units.ToAbbreviation() : "Infinity", compareSubmesh, compareUV, compareNormals, compareTangents, compareWeights);
        }

        public static implicit operator bool(SearchOptions value) { return value != null; }
        public static readonly SearchOptions DefaultValue = new SearchOptions();
        public static SearchOptions FallbackToDefualtValue(SearchOptions source) { if (!source) return DefaultValue; else return source; }
    }
}
