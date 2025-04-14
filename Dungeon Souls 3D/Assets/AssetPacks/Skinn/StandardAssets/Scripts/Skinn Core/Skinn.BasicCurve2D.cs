using UnityEngine;

namespace CWM.Skinn
{
    [System.Serializable]
    public class BasicCurve2D
    {
        public float start, end;
        public Vector4 tangents;

        public BasicCurve2D() { Reset(); }

        public BasicCurve2D(CurveType curveType) { Reset(curveType); }

        public void Reset(CurveType curveType = CurveType.LinearIn)
        {
            switch (curveType)
            {
                default:
                case CurveType.LinearIn: { start = 0f; end = 1f; tangents = new Vector4(0.25f, 0.25f, 0.75f, 0.75f); break; }
                case CurveType.LinearOut: { start = 1f; end = 0f; tangents = new Vector4(0.25f, 0.75f, 0.75f, 0.25f); break; }
                case CurveType.EaseIn: { start = 0f; end = 1f; tangents = new Vector4(0.1f, 0f, 1f, 0); break; }
                case CurveType.EaseOut: { start = 1f; end = 0f; tangents = new Vector4(0.1f, 1f, 0.9f, 1f); break; }
                case CurveType.Min: { start = 0f; end = 0f; tangents = new Vector4(0.25f, 0f, 0.75f, 0f); break; }
                case CurveType.Max: { start = 1f; end = 1f; tangents = new Vector4(0.25f, 1f, 0.75f, 1f); break; }
            }
        }

        public float Evaluate(float value)
        {
            Vector2 a = new Vector2(0, start),
                b = new Vector2(1, end),
                c = new Vector2(tangents.x, tangents.y),
                d = new Vector2(tangents.z, tangents.w);

            var t = 1 - value;
            var x1 = t * a + value * b;
            var x2 = t * b + value * c;
            var x3 = t * c + value * d;
            var x4 = t * x1 + value * x2;
            var x5 = t * x2 + value * x3;
            var x6 = t * x4 + value * x5;
            return x6.y;
        }

        public static float Evaluate(float value, CurveType curve)
        {
            switch (curve)
            {
                case CurveType.LinearIn: return LinearIn.Evaluate(value);
                case CurveType.LinearOut: return LinearOut.Evaluate(value);
                case CurveType.EaseIn: return EaseIn.Evaluate(value);
                case CurveType.EaseOut: return EaseOut.Evaluate(value);
                case CurveType.Min: return Min.Evaluate(value);
                case CurveType.Max: return Max.Evaluate(value);
                default: return value;
            }
        }

        public static implicit operator bool(BasicCurve2D value) { return value != null; }

        public static readonly BasicCurve2D LinearIn = new BasicCurve2D(CurveType.LinearIn);
        public static readonly BasicCurve2D LinearOut = new BasicCurve2D(CurveType.LinearOut);
        public static readonly BasicCurve2D EaseIn = new BasicCurve2D(CurveType.EaseIn);
        public static readonly BasicCurve2D EaseOut = new BasicCurve2D(CurveType.EaseOut);
        public static readonly BasicCurve2D Min = new BasicCurve2D(CurveType.Min);
        public static readonly BasicCurve2D Max = new BasicCurve2D(CurveType.Max);

        public static BasicCurve2D FallbackToDefualtValue(BasicCurve2D source)
        { if (!source) return LinearIn; else return source; }
    }
}
