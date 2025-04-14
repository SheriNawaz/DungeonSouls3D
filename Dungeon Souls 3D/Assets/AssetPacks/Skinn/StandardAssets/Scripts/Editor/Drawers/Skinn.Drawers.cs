using UnityEngine;
using UnityEditor;

namespace CWM.Skinn
{

    [CustomPropertyDrawer(typeof(DisabledAttribute))]
    public class DisabledDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property);
            var attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
            foreach (var item in attributes)
            {
                var attribute = item as TooltipAttribute; if (attribute == null) continue;
                EditorGUI.LabelField(position, new GUIContent("", attribute.tooltip));
                break;
            }
            EditorGUI.EndDisabledGroup();
        }
    }

    [CustomPropertyDrawer(typeof(ClampAbsAttribute))]
    public class ClampAbsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            if (property.propertyType == SerializedPropertyType.Float)
            {
                if (property.floatValue < 0.0f)
                {
                    property.floatValue = Mathf.Clamp(property.floatValue, 0.0f, Mathf.Infinity);
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorGUI.PropertyField(position, property);

            var attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
            foreach (var item in attributes)
            {
                var attribute = item as TooltipAttribute; if (attribute == null) continue;
                EditorGUI.LabelField(position, new GUIContent("", attribute.tooltip));
                break;
            }
        }
    }

    [CustomPropertyDrawer(typeof(NormalizeAttribute))]
    public class NormalizeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            if (property.propertyType == SerializedPropertyType.Vector3)
            {
                if (property.vector3Value.magnitude != 1f)
                {
                    property.vector3Value = Vector3.Normalize(property.vector3Value);
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorGUI.PropertyField(position, property);

            var attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
            foreach (var item in attributes)
            {
                var attribute = item as TooltipAttribute; if (attribute == null) continue;
                EditorGUI.LabelField(position, new GUIContent("", attribute.tooltip));
                break;
            }
        }
    }

    [CustomPropertyDrawer(typeof(SelectableLabelAttribute))]
    public class SelectableLabelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String) EditorGUI.SelectableLabel(position, property.stringValue);
            else EditorGUI.SelectableLabel(position, "use with string value! : " + label.text);
            var attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
            foreach (var item in attributes)
            {
                var attribute = item as TooltipAttribute; if (attribute == null) continue;
                EditorGUI.LabelField(position, new GUIContent("", attribute.tooltip));
                break;
            }
        }
    }

    [CustomPropertyDrawer(typeof(ClampAttribute))]
    public class ClampDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ClampAttribute context = attribute as ClampAttribute;

            if (property.propertyType == SerializedPropertyType.Float)
            {
                if (property.floatValue < context.min || property.floatValue > context.max)
                {
                    property.floatValue = Mathf.Clamp(property.floatValue, context.min, context.max);
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorGUI.PropertyField(position, property);
            var attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
            foreach (var item in attributes)
            {
                var attribute = item as TooltipAttribute; if (attribute == null) continue;
                EditorGUI.LabelField(position, new GUIContent("", attribute.tooltip));
                break;
            }
        }
    }


    [CustomPropertyDrawer(typeof(RepeatAttribute))]
    public class RepeatDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RepeatAttribute context = attribute as RepeatAttribute;
            if (property.propertyType == SerializedPropertyType.Float)
            {
                if (property.floatValue < context.min)
                {
                    property.floatValue = context.max;
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
                else if (property.floatValue > context.max)
                {
                    property.floatValue = context.min;
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            EditorGUI.PropertyField(position, property);
            var attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
            foreach (var item in attributes)
            {
                var attribute = item as TooltipAttribute; if (attribute == null) continue;
                EditorGUI.LabelField(position, new GUIContent("", attribute.tooltip));
                break;
            }
        }
    }


    [CustomPropertyDrawer(typeof(MeshInfoAttribute))]
    public class MeshInfoDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //var context = attribute as MeshInfoAttribute;
            var renderer = property.objectReferenceValue as Renderer;
            var monoBehaviour = property.objectReferenceValue as MonoBehaviour;

            
            float singleLine = EditorGUIUtility.singleLineHeight;
            Rect mRec = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(property, GUIContent.none, false));
            Rect toogleRect = mRec.SetWidth(singleLine);
            mRec = mRec.OffsetX(singleLine * 2, true);
            Rect infoRec = new Rect(position.x, position.y + singleLine, position.width, singleLine);

            EditorGUI.PropertyField(mRec, property);

            if (!property.objectReferenceValue) EditorGUI.DrawRect(mRec, new Color(1, 0f, 0f, SkinnEx.DefaultColors.Alpha0));
            if (!renderer && monoBehaviour) { EditorGUI.SelectableLabel(infoRec, "use with skinned mesh or mesh renderer" + label.text); return; }

            bool disableGame = renderer ? renderer.gameObject.activeSelf : true;
            EditorGUI.BeginChangeCheck();
            disableGame = EditorGUI.Toggle(toogleRect, disableGame);
            if (EditorGUI.EndChangeCheck())
            {
                if (renderer)
                {
                    Undo.RecordObject(renderer.gameObject, "toggle enabled");
                    renderer.gameObject.SetActive(disableGame);
                }
            }

            EditorGUI.LabelField(infoRec, SkinnEx.MeshDisplayInfo(renderer), EditorStyles.centeredGreyMiniLabel);
            var attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
            foreach (var item in attributes)
            {
                var attribute = item as TooltipAttribute; if (attribute == null) continue;
                EditorGUI.LabelField(position, new GUIContent("", attribute.tooltip));
                break;
            }
            EditorGUI.DrawRect(position, SkinnEx.DefaultColors.WhiteLightOverlay);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUI.GetPropertyHeight(property, label, false);
            height += EditorGUIUtility.singleLineHeight * 1f;
            return height;
        }
    }

    [CustomPropertyDrawer(typeof(HighlightNullAttribute))]
    public class HighlightNullDrawer : PropertyDrawer
    {
        private static Color nullColor = new Color(1f, 0f, 0f, SkinnEx.DefaultColors.Alpha0);
        private static Color warningColor = new Color(1f, 0.7f, 0.4f, SkinnEx.DefaultColors.Alpha0);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var d = Color.yellow;

            var context = attribute as HighlightNullAttribute;
            if (!property.objectReferenceValue) EditorGUI.DrawRect(position, context.warning ? warningColor : nullColor);
            EditorGUI.PropertyField(position, property);
            var attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
            foreach (var item in attributes)
            {
                var attribute = item as TooltipAttribute; if (attribute == null) continue;
                EditorGUI.LabelField(position, new GUIContent("", attribute.tooltip));
                break;
            }
            var go = property.objectReferenceValue as GameObject;
            var component = go ? go.transform : property.objectReferenceValue as Component; if (!component) return;
            if (!context.allowPrefabs && SkinnEx.IsNullOrNotInAScene(component))
            {
                property.objectReferenceValue = null;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    [CustomPropertyDrawer(typeof(ComponentHeaderAttribute))]
    public class ComponentHeaderDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String) { EditorGUI.SelectableLabel(position, "use with string value! : " + label.text); return; }
            if(string.IsNullOrEmpty((property.serializedObject.targetObject as MonoBehaviour).gameObject.name)) return;

            var rec = GUILayoutUtility.GetLastRect();
            rec = rec.OffsetY(EditorGUIUtility.singleLineHeight + 2, true);
            rec = rec.OffsetX(-(EditorGUIUtility.singleLineHeight + 2), true);
            EditorGUI.DrawRect(rec, new Color(0f, 1f, 1f, 0.02f));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }
    }

    [CustomPropertyDrawer(typeof(ModelRefrence))]
    public class ModelRefrenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rect1 = position.SetHeight(EditorGUIUtility.singleLineHeight);
            var rect2 = rect1.OffsetY(EditorGUIUtility.singleLineHeight);
            var rect3 = rect2.OffsetY(EditorGUIUtility.singleLineHeight);

            var rootSP = property.FindPropertyRelative("root");
            var renderersSP = property.FindPropertyRelative("renderers");
            var selectedSP = property.FindPropertyRelative("selected");
            var meshInfoSP = property.FindPropertyRelative("meshInfo");
            var assetIDSP = property.FindPropertyRelative("assetID");

            EditorGUI.BeginChangeCheck();
            EditorGUI.ObjectField(rect1, rootSP, new GUIContent(property.displayName, property.tooltip));

            bool changed = EditorGUI.EndChangeCheck();

            var root = rootSP.objectReferenceValue as GameObject;
            if (!root)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.Popup(rect2, 0, new string[1] {"null"} );
                EditorGUI.EndDisabledGroup();

                EditorGUI.LabelField(rect3, SkinnEx.MeshDisplayInfo((Mesh)null), EditorStyles.centeredGreyMiniLabel);

                EditorGUI.DrawRect(rect1, new Color(1f, 0f, 0f, SkinnEx.DefaultColors.Alpha0));

                return;
            }

            var assetInstanceID = assetIDSP.stringValue;
            var rendererInstanceID = root.GetRendererInstanceID();

            if(rendererInstanceID != assetInstanceID)
            {
                assetIDSP.stringValue = rendererInstanceID;
                changed = true;
            }

            if (changed)
            {
                var getRenderers = root.GetRenderers(true, property.name.Contains("target"));
                renderersSP.arraySize = getRenderers.Count;
                for (int i = 0; i < getRenderers.Count; i++) renderersSP.GetArrayElementAtIndex(i).objectReferenceValue = getRenderers[i];
                renderersSP.serializedObject.ApplyModifiedProperties();
                rootSP.serializedObject.ApplyModifiedProperties();
            }

            var names = new string[renderersSP.arraySize];
            for (int i = 0; i < names.Length; i++)
                names[i] = SkinnEx.EnforceObjectName(renderersSP.GetArrayElementAtIndex(i).objectReferenceValue);

            EditorGUI.BeginChangeCheck();
            selectedSP.intValue = EditorGUI.Popup(rect2, selectedSP.intValue, names);
            changed = EditorGUI.EndChangeCheck() || changed;

            EditorGUI.LabelField(rect3, meshInfoSP.stringValue, EditorStyles.centeredGreyMiniLabel);

            EditorGUI.DrawRect(position, SkinnEx.DefaultColors.WhiteLightOverlay);

            if (changed)
            {
                var selected = selectedSP.intValue;

                if (selected > -1 && selected < renderersSP.arraySize)
                    meshInfoSP.stringValue = SkinnEx.MeshDisplayInfo(renderersSP.GetArrayElementAtIndex(selected).objectReferenceValue as Renderer);
                else meshInfoSP.stringValue = SkinnEx.MeshDisplayInfo((Mesh)null);

#if CWM_DEV
                Debug.Log(rendererInstanceID);
#endif

                renderersSP.serializedObject.ApplyModifiedProperties();
            }
            var attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
            foreach (var item in attributes)
            {
                var attribute = item as TooltipAttribute; if (attribute == null) continue;
                EditorGUI.LabelField(rect1, new GUIContent("", attribute.tooltip));
                break;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3f;
        }
    }
}