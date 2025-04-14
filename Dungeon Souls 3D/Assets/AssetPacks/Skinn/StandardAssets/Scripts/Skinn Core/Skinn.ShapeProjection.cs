using UnityEngine;

namespace CWM.Skinn
{
    [System.Serializable]
    public class ShapeProjection
    {
        [Tooltip("allow blend-shape projection, only blend-shapes that are not in the target mesh are projected.")]
        public bool enabled;

        [Tooltip("the unit of distance to use.")]
        public ConversionUnits units;

        [Tooltip("adds inflation relative to the source deltas.")]
        [ClampAbs]
        public float inflate;

        [Range(1, 32)]
        [Tooltip("collect deltas from closest vertices.")]
        public int radialCount;

        [Header("Deltas")]

        [Tooltip("create delta vertices.")]
        public bool vertex;
        [Tooltip("create delta normals.")]
        public bool normal;
        [Tooltip("create delta tangent.")]
        public bool tangent;

        [Range(0.01f, 10f)]
        [Tooltip("how much to exaggerate the source delta for to calculate normals and tangents.")]
        public float lighting;
             
        public void Reset()
        {
            enabled = true;
            units = ConversionUnits.Inch;
            inflate = 0f;
            radialCount = 16;
            vertex = true;
            normal = true;
            tangent = true;
            lighting = 3f;
        }

        public ShapeProjection() { Reset(); }

        public float Inflate { get { return inflate.ToUnitFromStandard(units); } }

        public ShapeFilter ShapeMask { get; set; }

        public override string ToString()
        {
            return string.Format("Shape: {0}, inflate = {1}, radial = {2}, vertex = {3}, normal = {4}, tangent = {5}, light = {6}"
                , enabled, inflate == 0 ? "False": inflate.ToString("0.0") + units.ToAbbreviation(), radialCount, radialCount, vertex, normal, tangent, lighting);
        }

        public static implicit operator bool(ShapeProjection value) { return value != null; }
        public static readonly ShapeProjection DefaultValue = new ShapeProjection();
        public static ShapeProjection FallbackToDefualtValue(ShapeProjection source)
        { if (!source) return DefaultValue; else return source; }
    }
}
