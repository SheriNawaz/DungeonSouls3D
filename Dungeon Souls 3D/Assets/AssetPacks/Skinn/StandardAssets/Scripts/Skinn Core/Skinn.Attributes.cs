using System;
using UnityEngine;

namespace CWM.Skinn
{
    public class CtxMenuItem : Attribute
    {
        public string menuItem;
        public string tooltip;
        public ContextFlag flags;
        public ContextRequirements requirements;
        public int version;

        public void Reset()
        { menuItem = ""; tooltip = ""; flags = ContextFlag.None; requirements = ContextRequirements.None; version = 0; }

        public CtxMenuItem(string menuItem, string tooltip, int version)
        {
            Reset();
            this.version = version;
            this.menuItem = menuItem;
            this.tooltip = tooltip;
        }

        public CtxMenuItem(string menuItem, string tooltip, ContextFlag flags, int version)
        {
            Reset();
            this.menuItem = menuItem;
            this.tooltip = tooltip;
            this.version = version;
            this.flags = flags;
        }

        public CtxMenuItem(string menuItem, string tooltip, ContextFlag flags, ContextRequirements requirements, int version)
        {
            Reset();
            this.menuItem = menuItem;
            this.tooltip = tooltip;
            this.version = version;
            this.flags = flags;
            this.requirements = requirements;
        }
        
        public CtxMenuItem( string menuItem,string tooltip, ContextRequirements requirements, int version)
        {
            Reset();
            this.menuItem = menuItem;
            this.tooltip = tooltip;
            this.version = version;
            this.requirements = requirements;
        }

        public static implicit operator bool(CtxMenuItem x) { return x != null; }
    }

    public class DisabledAttribute : PropertyAttribute { public DisabledAttribute() { } }

    public class ClampAbsAttribute : PropertyAttribute { public ClampAbsAttribute() { } }

    public class NormalizeAttribute : PropertyAttribute { public NormalizeAttribute() { } }

    public class SelectableLabelAttribute: PropertyAttribute { public SelectableLabelAttribute() { } }

    public class ClampAttribute : PropertyAttribute
    {
        public float min, max;
        public ClampAttribute(float min, float max) { this.min = min; this.max = max; }
    }

    public class RepeatAttribute : PropertyAttribute
    {
        public float min, max;
        public RepeatAttribute(float min, float max) { this.min = min; this.max = max; }
    }

    public class MeshInfoAttribute : PropertyAttribute { public MeshInfoAttribute() { } }

    public class ComponentHeaderAttribute : PropertyAttribute { public ComponentHeaderAttribute() { } }

    public class HighlightNullAttribute : PropertyAttribute
    {
        public bool allowPrefabs;
        public bool warning;

        public HighlightNullAttribute()
        {
            allowPrefabs = true;
            order = 0;
        }

        public HighlightNullAttribute(bool allowPrefabs)
        {
            this.allowPrefabs = allowPrefabs;
            order = 0;
        }

        public HighlightNullAttribute(bool allowPrefabs, bool warning)
        {
            this.allowPrefabs = allowPrefabs;
            this.warning = warning;
            order = 0;
        }
    }
}