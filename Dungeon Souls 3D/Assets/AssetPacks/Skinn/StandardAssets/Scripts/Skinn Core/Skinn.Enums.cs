namespace CWM.Skinn
{
    public enum BezierSelection { Start, End, StartTanget, EndTanget, CountOf }

    public enum ConversionUnits { Meter, Millimeter, Centimeter, Yard, Foot, Inch }

    public enum MeshSurfaceType { Orangic, HardSuface, Mixed }

    public enum GizmosDisplayMode { Selected, Always, Never }

    public enum CurveType { LinearIn, LinearOut, EaseIn, EaseOut, Min, Max };

    public enum TransferUV
    {
        ApplyToChannel_1,
        ApplyToChannel_2,
        ApplyToChannel_3,
        ApplyToChannel_4,
        ApplyToChannel_5,
        ApplyToChannel_6,
        ApplyToChannel_7,
        ApplyToChannel_8,
        Off,
    }

    public enum ContextFlag
    {
        None,
        Header,
        PasteFunction,
        Bone,
        BoneWithChildern,
        BoneBone,
    }

    /// <summary>
    /// unless specified context requirements work with single selection.
    /// </summary>
    public enum ContextRequirements
    {
        None,
        SingleItem,
        MultipleItems,
        Children,
        SubMeshes,
        SingleSubMesh,
        RootBone,
        CanPasteVertex,

        //MeshNotRequired
    }


}
