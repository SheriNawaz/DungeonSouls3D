using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public static Rect GetRectX(this Rect source, bool firstHalf = true)
        {
            if (firstHalf) return new Rect(source.x, source.y, source.width * 0.5f, source.height);
            return new Rect(source.x + source.width * 0.5f, source.y, source.width * 0.5f, source.height);
        }

        public static Rect GetRectY(this Rect source, bool firstHalf = true)
        {
            if (firstHalf) return new Rect(source.x, source.y, source.width, source.height * 0.5f);
            return new Rect(source.x, source.y + source.height * 0.5f, source.width, source.height * 0.5f);
        }

        public static Rect GetRectX(this Rect source, int i, int count, float padding = 0)
        {
            float pad = padding;
            float width = (source.width - (pad * count)) / count;
            return new Rect(source.x + (width * i) + (pad * i), source.y, width, source.height);
        }

        public static Rect GetRectY(this Rect source, int i, int count, float padding = 0)
        {
            float pad = padding;
            float height = (source.height - (pad * count)) / count;
            return new Rect(source.x, source.y + (height * i) + (pad * i), source.width, height);
        }

        public static Rect ClampToUniform(this Rect source)
        {
            var rect = source;
            float min = Mathf.Min(source.width, source.height);
            rect.width = min;
            rect.height = min;
            return rect.Scale();
        }

        public static Rect Scale(this Rect source, float scale = 1f)
        {
            Vector2 center = source.center;
            Vector2 position = source.position;
            position.x = ((position.x - center.x) * scale) + center.x;
            position.y = ((position.y - center.y) * scale) + center.y;
            Rect rect = new Rect(position.x, position.y, source.width * scale, source.height * scale);
            return rect;
        }

        public static Rect SetWidth(this Rect source, float value)
        {
            Rect rect = new Rect(source.x, source.y, value, source.height);
            return rect;
        }

        public static Rect SetWidth(this Rect source, float value, float offset)
        {
            Rect rect = new Rect(source.x + offset, source.y, value, source.height);
            return rect;
        }

        public static Rect SetHeight(this Rect source, float value)
        {
            Rect rect = new Rect(source.x, source.y, source.width, value);
            return rect;
        }

        public static Rect SetHeight(this Rect source, float value, float offset)
        {
            Rect rect = new Rect(source.x, source.y + offset, source.width, value);
            return rect;
        }

        public static Rect OffsetY(this Rect source, float value, bool constrain = false)
        {
            Rect rect = new Rect(source.x, source.y + value, source.width, source.height + (constrain ? -value : 0));
            return rect;
        }

        public static Rect OffsetX(this Rect source, float value, bool constrain = false)
        {
            Rect rect = new Rect(source.x + value, source.y, source.width + (constrain ? -value : 0), source.height);
            return rect;
        }

        public static Rect Scale(this Rect source, int scale) { return Scale(source, (float)scale); }

        public static bool IsRectInside(this Rect source, Rect inside, int padding = 0)
        {
            Rect top = new Rect(source);
            Rect bottom = new Rect(source);
            Rect left = new Rect(source);
            Rect right = new Rect(source);
            top.x -= top.width + padding;
            bottom.x += top.width + padding;
            left.y -= top.height + padding;
            right.y += top.height + padding;
            if (top.Overlaps(inside) || bottom.Overlaps(inside) || left.Overlaps(inside) || right.Overlaps(inside)) return false;
            return true;
        }

    }

}