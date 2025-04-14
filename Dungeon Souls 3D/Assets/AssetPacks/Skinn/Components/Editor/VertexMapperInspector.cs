using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace CWM.Skinn
{
    [CustomEditor(typeof(VertexMapper))]
    public partial class VertexMapperInspector : Editor
    {
        private static class VMGUIContent
        {
            internal static readonly GUIContent Create = new GUIContent("Create", "");
            internal static readonly GUIContent Smooth = new GUIContent("Additional Smoothing", "applies Additional Weight Smoothing");
            internal static readonly GUIContent AppendToNew = new GUIContent("Append To New", "creates a new transform stack and merges the preview mesh too it.");
            internal static readonly GUIContent Done = new GUIContent("Done", "sets the preview mesh as null so it will not be destroyed on the next 'Create'.");

            internal static readonly GUIContent BoneFilter = new GUIContent("Bone Filter", "bone projection filter.");
            internal static readonly GUIContent BlendshapeFilter = new GUIContent("Blend-shapes Filter", "blend-shapes projection filter.");

        }

        public AnimBool showBones;
        public AnimBool showShapes;

        private BoneTreeGUI boneUI = new BoneTreeGUI();
        private ShapeTreeGUI shapeUI = new ShapeTreeGUI();

        public static Editor Inspector { get; private set; }

        [MenuItem("CONTEXT/VertexMapper/Edit Inspector Script", false, 9999)]
        private static void Open(MenuCommand command) { if (Inspector) AssetDatabase.OpenAsset(MonoScript.FromScriptableObject(Inspector), 0); }

        [MenuItem("GameObject/Create Skinn/Vertex Mapper", false, 15)]
        private static void CreateVertexMapper(MenuCommand command)
        {
            var go = new GameObject("Vertex Mapper", new System.Type[1] { typeof(VertexMapper) });
            Undo.RegisterCreatedObjectUndo(go, "Create VM");
            Selection.activeObject = go;
        }

        private const float fadeSpeed = 5f;

        public override bool RequiresConstantRepaint()
        {
            var repaint = BasicCurve2DDrawer.NeedsRepaint;
            return repaint;
        }

        public void OnEnable()
        {
            Inspector = this;

            if (showBones == null) showBones = new AnimBool(false);
            showBones.valueChanged.RemoveListener(Repaint);
            showBones.valueChanged.AddListener(Repaint);
            showBones.speed = fadeSpeed;

            if (showShapes == null) showShapes = new AnimBool(false);
            showShapes.valueChanged.RemoveListener(Repaint);
            showShapes.valueChanged.AddListener(Repaint);
            showShapes.speed = fadeSpeed;

            Undo.undoRedoPerformed -= boneUI.OnUndoRedoPerformed;
            Undo.undoRedoPerformed -= shapeUI.OnUndoRedoPerformed;
            Undo.undoRedoPerformed += boneUI.OnUndoRedoPerformed;
            Undo.undoRedoPerformed += shapeUI.OnUndoRedoPerformed;
        }

        public override void OnInspectorGUI()
        {
            Inspector = this;

            DrawDefaultInspector();
            var rectWidth = GUILayoutUtility.GetLastRect().width;
            var vm = target as VertexMapper; if (!vm) return;

            EditorGUI.BeginChangeCheck();
            showBones.target = EditorGUILayout.Foldout(showBones.target, VMGUIContent.BoneFilter);
            if (EditorGUI.EndChangeCheck()) { if (showBones.target) showShapes.target = false; }

            if (showBones.faded > 0)
            {
                using (new EditorGUILayout.FadeGroupScope(showBones.faded))
                {
                    Rect rect = GUILayoutUtility.GetRect(rectWidth, boneUI.GetPropertyHeight() + 20);
                    boneUI.OnGUI(rect, vm, "_0");
                }
            }

            EditorGUI.BeginChangeCheck();
            showShapes.target = EditorGUILayout.Foldout(showShapes.target, VMGUIContent.BlendshapeFilter);
            if (EditorGUI.EndChangeCheck()) { if (showShapes.target) showBones.target = false; }

            if (showShapes.faded > 0)
            {
                using (new EditorGUILayout.FadeGroupScope(showShapes.faded))
                {
                    Rect rect = GUILayoutUtility.GetRect(rectWidth, shapeUI.GetPropertyHeight() + 20);
                    shapeUI.OnGUI(rect, vm, "_1");
                }
            }
            EditorGUILayout.LabelField("");

            EditorGUI.BeginDisabledGroup(!vm.CanCreateBinding);
            {
                if (GUILayout.Button(VMGUIContent.Create, GUI.skin.button))
                {
                    vm.Create();

                    if (!Application.isPlaying) SceneView.RepaintAll();
                }
            }
            EditorGUI.EndDisabledGroup();

            var hasPreviewAsset = vm.HasPreviewAsset;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginDisabledGroup(!hasPreviewAsset);
                {
                    var previewAsset = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(vm.preveiwAsset, typeof(SkinnedMeshRenderer), true);
                    if (previewAsset) { }
                    EditorGUILayout.Space();

                    if (GUILayout.Button(VMGUIContent.Smooth, EditorStyles.miniButton))
                    {
                        vm.AdditionalWeightSmoothing();
                    }

                    EditorGUILayout.LabelField("Finish", EditorStyles.centeredGreyMiniLabel);

                    if (GUILayout.Button(VMGUIContent.AppendToNew, EditorStyles.miniButton))
                    {
                        vm.AppendToNew();
                        if (!Application.isPlaying) SceneView.RepaintAll();
                    }

                    EditorGUILayout.Space();

                    if (GUILayout.Button(VMGUIContent.Done, EditorStyles.miniButton))
                    {
                        Undo.RecordObject(vm, "Done");
                        vm.preveiwAsset = null;
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.LabelField("");

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Summary", EditorStyles.centeredGreyMiniLabel);

                GUIStyle infoStlye = EditorStyles.wordWrappedMiniLabel;

                EditorGUILayout.LabelField("Source " + vm.sourceBaking.ToString(), infoStlye);
                EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), SkinnEx.DefaultColors.WhiteLightOverlay);
                EditorGUILayout.LabelField("Target " + vm.targetBaking.ToString(), infoStlye);
                EditorGUILayout.LabelField(vm.search.ToString(), infoStlye);
                EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), SkinnEx.DefaultColors.WhiteLightOverlay);

                EditorGUILayout.LabelField(vm.weightProjection.ToString(), infoStlye);
                EditorGUILayout.LabelField(vm.shapeProjection.ToString(), infoStlye);
                EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), SkinnEx.DefaultColors.WhiteLightOverlay);

                EditorGUILayout.LabelField(vm.gizmos.ToString(), infoStlye);
                EditorGUILayout.LabelField("Bone " + vm.boneFilter.ToString(), infoStlye);
                EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), SkinnEx.DefaultColors.WhiteLightOverlay);

                EditorGUILayout.LabelField("Shape " + vm.shapeFilter.ToString(), infoStlye);
            }


            EditorGUILayout.LabelField("");
        }

    }
}
