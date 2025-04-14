using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CWM.Skinn
{

#if CWM_DEV
    [CreateAssetMenu(fileName = "InternalAsset", menuName = "Skinn/InternalAsset", order = 1)]
#endif
    [System.Serializable]
    public class SkinnInternalAsset : ScriptableObject
    {
        public static readonly bool IsFullVersion = false;

        [System.Serializable]
        public class ContextSettings
        {
            [Tooltip("Remove extra bones when performing commands.")]
            public bool autoOptimzeBones = false;
        }

        [System.Serializable]
        public class Colors
        {
            public Color smoothOnColor, smoothOffColor, latticeColor, rangeColor;

            public void Reset()
            {
                smoothOnColor = SmoothOnColor; smoothOffColor = SmoothOffColor;
                latticeColor = LatticeColor; rangeColor = RangeColor;
            }

            public Colors() { Reset(); }

            internal static readonly Color RangeColor = new Color(0, 1f, 0.1f, 0.26f);
            internal static readonly Color SmoothOnColor = Color.white;
            internal static readonly Color SmoothOffColor = Color.black;
            internal static readonly Color LatticeColor = Color.cyan;

            public static Colors DefualtColors = new Colors();

            public static implicit operator bool (Colors x) { return x != null; }
        }

        [System.Serializable]
        public class VetexImportSettings
        {
            public bool compareSubMesh, compareUV, compareNormal, compareTangent;
            public VetexImportSettings() { compareSubMesh = true; compareUV = true; compareNormal = true; compareTangent = true; }
            public static implicit operator bool(VetexImportSettings x) { return x != null; }
        }

        public ContextSettings contextSettings = new ContextSettings();
        [Space]

        [Header("Gizmos")]
        public Colors colors = new Colors();
        [Space]

        [Header("Compute Shaders")]
        [HighlightNull()] public ComputeShader adjacentIndices = null;
        [HighlightNull()] public ComputeShader radialIndices = null;
        [HighlightNull()] public ComputeShader combineDeltas = null;
        [HighlightNull()] public ComputeShader deltaProjector = null;
        [HighlightNull()] public ComputeShader laplacianFilter = null;
        [HighlightNull()] public ComputeShader linearSkinning = null;
        [HighlightNull()] public ComputeShader linearTransform = null;
        [HighlightNull()] public ComputeShader maskFilter = null;
        [HighlightNull()] public ComputeShader weightMapping = null;
        [HighlightNull()] public ComputeShader texturePadding = null;

        [Header("Custom Shaders")]
        [HighlightNull()] public Shader lightMapAtlas = null;

        [Space]

        [HighlightNull()] public Shader debugNormals = null;
        [HighlightNull()] public Shader debugNull = null;
        [HighlightNull()] public Shader debugVertexColor = null;
        [HighlightNull()] public Shader debugVertexOverlay = null;

        [Header("Vertex Importer Settings")]
        [Tooltip("settings for re-mapping imported vertex information.")]
        public VetexImportSettings vetexImporterSettings = new VetexImportSettings();

        [Header("Quick Materials")]
        public Material[] unlitMaterials = new Material[0];
        public Material[] standardMaterials = new Material[0];

        
        public static Material[] GetUnlitMaterials { get { if (!internalAsset) return new Material[0]; else return internalAsset.unlitMaterials; } }
        public static Material[] GetStandardMaterials { get { if (!internalAsset) return new Material[0]; else return internalAsset.standardMaterials; } }


        public static Colors GetColors
        {
            get { if (!Asset || SkinnEx.IsNullOrEmpty(Asset.unlitMaterials)) return Colors.DefualtColors; else return Asset.colors; }
        }

        public static Material GetUnlitMaterial
        {
            get { if (!Asset || SkinnEx.IsNullOrEmpty(Asset.unlitMaterials)) return null; else return Asset.unlitMaterials[0]; }
        }

        public static Material GetStandardMaterial
        {
            get { if (!Asset || SkinnEx.IsNullOrEmpty(Asset.standardMaterials)) return null; else return Asset.standardMaterials[0]; }
        }

        private const string missingShaderWarning = "Skinn InteralData Missing Shader";

        [HideInInspector] public List<LocalTransform> transforms = new List<LocalTransform>();
        [HideInInspector] public ElementFilter blendshapeFilter = new ElementFilter();

        [ContextMenu("ClearTempData")]
        public static void ClearTempData()
        {
            if (!Asset) return;
            if(Asset.transforms.Count > 0) Asset.transforms = new List<LocalTransform>();
            if (Asset.blendshapeFilter.elements.Count > 0) Asset.blendshapeFilter = new ElementFilter();
        }

        public bool HasShaders()
        {
            string log = "";
            if (!SystemInfo.supportsComputeShaders) log += Environment.NewLine + string.Format("{1} : {0}", "system does not support compute shaders", SystemInfo.deviceModel);
            //Compute Shaders
            if (!adjacentIndices) log += Environment.NewLine +  string.Format("{1} : {0}", "adjacentIndices", missingShaderWarning);
            if (!radialIndices) log += Environment.NewLine + string.Format("{1} : {0}", "radialIndices", missingShaderWarning);
            if (!combineDeltas) log += Environment.NewLine + string.Format("{1} : {0}", "combineDeltas", missingShaderWarning);
            if (!deltaProjector) log += Environment.NewLine + string.Format("{1} : {0}", "deltaProjector", missingShaderWarning);
            if (!laplacianFilter) log += Environment.NewLine + string.Format("{1} : {0}", "laplacianFilter", missingShaderWarning);
            if (!linearSkinning) log += Environment.NewLine + string.Format("{1} : {0}", "linearSkinning", missingShaderWarning);
            if (!linearTransform) log += Environment.NewLine + string.Format("{1} : {0}", "linearTransform", missingShaderWarning);
            if (!maskFilter) log += Environment.NewLine + string.Format("{1} : {0}", "maskFilter", missingShaderWarning);
            if (!weightMapping) log += Environment.NewLine + string.Format("{1} : {0}", "weightMapping", missingShaderWarning);
            if (!texturePadding) log += Environment.NewLine + string.Format("{1} : {0}", "texturePadding", missingShaderWarning);
            
            //Shaders
            if (!lightMapAtlas) log += Environment.NewLine + string.Format("{1} : {0}", "lightMapAtlas", missingShaderWarning);

            if (!debugNormals) log += Environment.NewLine + string.Format("{1} : {0}", "debugNormals", missingShaderWarning);

            if (!debugVertexColor) log += Environment.NewLine + string.Format("{1} : {0}", "debugVertexColor", missingShaderWarning);

            if (!debugVertexOverlay) log += Environment.NewLine + string.Format("{1} : {0}", "debugVertexOverlay", missingShaderWarning);
       
            if (log != "")
            {
#if UNITY_EDITOR
                Selection.activeObject = this;
#endif
                if (Application.isEditor) Debug.LogWarning(log);
                return false;
            }
            return true;
        }

        private static SkinnInternalAsset internalAsset = null;
        private static int internalDataFailed = 0;
        private const string internalDataWarning = "Missing Internal Data Re-import Package or Create a SkinnInternalData and place it in a 'Resources' folder";

        public static SkinnInternalAsset Asset
        {
            get
            {
                if (internalAsset || internalDataFailed > 5) return internalAsset;
                var data = Resources.LoadAll("", typeof(SkinnInternalAsset));
                if (data != null && data.Length > 0) internalAsset = data[0] as SkinnInternalAsset;
                if (!internalAsset) { if(internalDataFailed > 0) Debug.LogError(internalDataWarning); internalDataFailed++; };
                return internalAsset;
            }
        }


        public static bool  CanCompute()
        {
            return SystemInfo.supportsComputeShaders && Asset && Asset.HasShaders();
        }
    }
}