using UnityEngine;

namespace CWM.Skinn
{
    [System.Serializable]
    public class WeightProjection
    {
        [Tooltip("allow weight projection, can only be disabled with skinned target assets.")]
        public bool enabled;

        [Tooltip("the unit of distance to use.")]
        public ConversionUnits units;
 
        [Range(1, 4)]
        [Tooltip("max number of weight channels in the final bone-weights.")]
        public int skinQuality;

        [Tooltip("inflate/deflate targets vertices for weighting before any weighting passes.")]
        public float startingPose;

        [Tooltip("the max number of bones to used when blending weights.")]
        [Range(4, 32)]
        public int blendQuality;

        [Header("Radial Blending")]
        [Tooltip("how many radial weights are blended.")]
        [Range(1, 32)]
        public int radialCount;

        [Header("Adjacent Blending")]
        [Range(0, 32)]
        [Tooltip("how many neighbors weights are blended.")]
        public int adjacentCount;

        [Header("Surface Blending")]

        [ClampAbs]
        [Tooltip("how much weight to apply to the mapped surface.")]
        public float surfaceWeight;

        [Tooltip("how much the mapped distance applies to the weighting. 0 is the close, 1 is far. Default is a LinearIn curve")]
        public BasicCurve2D surfaceBlending;

        [Header("DeltaMush Blending")]
        
        [Tooltip("number steps to reach the delta pose. 0 is off.")]
        [Range(0, 64)]
        public int deltaPasses;

        [Tooltip("inflate/deflate targets vertices over the number of delta passes.")]
        public float deltaPose;

        [Tooltip("how much weight is add each pass. 0 is the first pass, 1 is the last pass. Default is a LinearIn curve")]
        public BasicCurve2D deltaBlending;

        public void Reset()
        {
            enabled = true;
            units = ConversionUnits.Inch;
            skinQuality = 4;
            startingPose = 0f;
            blendQuality = 8;

            radialCount = 1;
            adjacentCount = 0;

            surfaceWeight = 1f;
            surfaceBlending = new BasicCurve2D(CurveType.LinearIn);

            deltaPasses = 0;
            deltaPose = -3f;
            deltaBlending = new BasicCurve2D(CurveType.LinearIn);
        }


        public override string ToString()
        {
            return string.Format("Weight: {0}, quality = {1}, pose = {2}, blendQ = {3}, radial = {4}, adj = {5}, surf = {6}, mush = {7}, mPose = {8},"
                , enabled, skinQuality, startingPose == 0 ? "False" : startingPose.ToString("0.0") + units.ToAbbreviation(), blendQuality, radialCount, adjacentCount, surfaceWeight, deltaPasses, deltaPose.ToString("0.0") + units.ToAbbreviation());
        }

        public WeightProjection() { Reset();  }

        public float DeltaPose { get {if (Mathf.Abs(deltaPose) < 0.00001) return -0.1f.ToUnitFromStandard(ConversionUnits.Inch); return deltaPose.ToUnitFromStandard(units); }}
        public float StartingPose { get { return startingPose.ToUnitFromStandard(units); } }

        public static implicit operator bool(WeightProjection value) { return value != null; }
        public static readonly WeightProjection DefaultValue = new WeightProjection();
        public static WeightProjection FallbackToDefualtValue(WeightProjection source)
        { if (!source) return DefaultValue; else return source; }
    }
}
