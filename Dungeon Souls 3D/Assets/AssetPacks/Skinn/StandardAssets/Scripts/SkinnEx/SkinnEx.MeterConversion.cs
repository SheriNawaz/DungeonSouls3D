using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public static float ToUnitFromNonStandard(this float meterValue, ConversionUnits conversion)
        {
            float converion = meterValue;
            switch (conversion)
            {
                case ConversionUnits.Meter: return converion * 1f;
                case ConversionUnits.Millimeter: return converion * 1000f;
                case ConversionUnits.Centimeter: return converion * 100f;
                case ConversionUnits.Yard: return converion * 1.09361f;
                case ConversionUnits.Foot: return converion * 3.28084f;
                case ConversionUnits.Inch: return converion * 39.3701f;
                default: break;
            }
            return converion;
        }

        public static float ToUnitFromStandard(this float unitValue, ConversionUnits conversion)
        {
            float converion = unitValue;
            switch (conversion)
            {
                case ConversionUnits.Meter: return converion / 1f;
                case ConversionUnits.Millimeter: return converion / 1000f;
                case ConversionUnits.Centimeter: return converion / 100f;
                case ConversionUnits.Yard: return converion / 1.09361f;
                case ConversionUnits.Foot: return converion / 3.28084f;
                case ConversionUnits.Inch: return converion / 39.3701f;
                default: break;
            }
            return converion;
        }

        public static Vector3 ToInches(this Vector3 vector)
        {
            Vector3 inches;
            inches.x = vector.x.ToUnitFromStandard(ConversionUnits.Inch);
            inches.y = vector.y.ToUnitFromStandard(ConversionUnits.Inch);
            inches.z = vector.z.ToUnitFromStandard(ConversionUnits.Inch);
            return inches;
        }

        public static string ToAbbreviation(this ConversionUnits conversion, string prefix = "")
        {
            switch (conversion)
            {
                case ConversionUnits.Meter: return prefix + "m";
                case ConversionUnits.Millimeter: return prefix + "mm";
                case ConversionUnits.Centimeter: return prefix + "cm";
                case ConversionUnits.Yard: return prefix + "yd";
                case ConversionUnits.Foot: return prefix + "ft";
                case ConversionUnits.Inch: return prefix + "in";
                default: return "";
            }
        }
    }
}