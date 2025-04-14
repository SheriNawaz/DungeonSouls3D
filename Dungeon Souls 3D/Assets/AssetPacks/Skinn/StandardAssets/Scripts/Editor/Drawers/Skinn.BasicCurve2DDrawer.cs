using UnityEngine;
using UnityEditor;

namespace CWM.Skinn
{
    [CustomPropertyDrawer(typeof(BasicCurve2D))]
    public class BasicCurve2DDrawer : PropertyDrawer
    {
        public static bool NeedsRepaint { get; private set; }

        private static BezierSelection SelectedBezierGUICurvePoint = 0;

        private static string controlID = "-1";


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (DrawCurveRect(position.SetHeight(EditorGUIUtility.singleLineHeight * 3), property))
            {
                NeedsRepaint = true;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (property.isExpanded ? 4 : 1) * EditorGUIUtility.singleLineHeight;
        }

        public static class BasicCurveContextMenu
        {
            private static SerializedProperty ContextCurve = null;
            private static BasicCurve2D CurveCopy = null;

            private const string LinearIn = "LinearIn";
            private const string LinearOut = "LinearOut";
            private const string EaseIn = "EaseIn";
            private const string EaseOut = "EaseOut";
            private const string Min = "Min";
            private const string Max = "Max";

            private static readonly string[] CurveTypes = new string[]
            {
                LinearIn,
                LinearOut,
                EaseIn,
                EaseOut,
                Min,
                Max
            };

            private const string Copy = "Copy";
            private const string Paste = "Paste";

            public static void ShowMenu(SerializedProperty basicCurve)
            {
                ContextCurve = basicCurve;
                if (ContextCurve == null) { Debug.LogWarning("missing ContextCurve!"); return; }

                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < CurveTypes.Length; i++)
                {
                    string commandID = CurveTypes[i];
                    menu.AddItem(new GUIContent(commandID), false, ContextMenuSelected, commandID);
                }

                menu.AddItem(new GUIContent(Copy), false, ContextMenuSelected, Copy);

                if (CurveCopy == null) menu.AddDisabledItem(new GUIContent(Paste));
                else menu.AddItem(new GUIContent(Paste), false, ContextMenuSelected, Paste);

                menu.ShowAsContext();
            }

            private static void ContextMenuSelected(object contextCommandID)
            {
                if (ContextCurve == null) { Debug.LogWarning("missing ContextCurve!"); return; }

                string id;
                try { id = contextCommandID as string; } catch { id = ""; }
                switch (id)
                {
                    case Copy:
                        {
                            CurveCopy = new BasicCurve2D();
                            CurveCopy.start = ContextCurve.FindPropertyRelative("start").floatValue;
                            CurveCopy.end = ContextCurve.FindPropertyRelative("end").floatValue;
                            CurveCopy.tangents = ContextCurve.FindPropertyRelative("tangents").vector4Value;

                            break;
                        }
                    case Paste:
                        {
                            if (CurveCopy == null) { Debug.LogWarning("missing CurveCopy!"); return; }
                            ContextCurve.FindPropertyRelative("start").floatValue = CurveCopy.start;
                            ContextCurve.FindPropertyRelative("end").floatValue = CurveCopy.end;
                            ContextCurve.FindPropertyRelative("tangents").vector4Value = CurveCopy.tangents;
                            break;
                        }
                    case LinearIn:
                        {
                            ContextCurve.FindPropertyRelative("start").floatValue = 0f;
                            ContextCurve.FindPropertyRelative("end").floatValue = 1f;
                            ContextCurve.FindPropertyRelative("tangents").vector4Value = new Vector4(0.25f, 0.25f, 0.75f, 0.75f);
                            break;
                        }
                    case LinearOut:
                        {
                            ContextCurve.FindPropertyRelative("start").floatValue = 1f;
                            ContextCurve.FindPropertyRelative("end").floatValue = 0f;
                            ContextCurve.FindPropertyRelative("tangents").vector4Value = new Vector4(0.25f, 0.75f, 0.75f, 0.25f);
                            break;
                        }
                    case EaseIn:
                        {
                            ContextCurve.FindPropertyRelative("start").floatValue = 0f;
                            ContextCurve.FindPropertyRelative("end").floatValue = 1f;
                            ContextCurve.FindPropertyRelative("tangents").vector4Value = new Vector4(0.1f, 0f, 1f, 0);
                            break;
                        }
                    case EaseOut:
                        {
                            ContextCurve.FindPropertyRelative("start").floatValue = 1f;
                            ContextCurve.FindPropertyRelative("end").floatValue = 0f;
                            ContextCurve.FindPropertyRelative("tangents").vector4Value = new Vector4(0.1f, 1f, 0.9f, 1f);
                            break;
                        }
                    case Min:
                        {
                            ContextCurve.FindPropertyRelative("start").floatValue = 0f;
                            ContextCurve.FindPropertyRelative("end").floatValue = 0f;
                            ContextCurve.FindPropertyRelative("tangents").vector4Value = new Vector4(0.25f, 0f, 0.75f, 0f);
                            break;
                        }
                    case Max:
                        {
                            ContextCurve.FindPropertyRelative("start").floatValue = 1f;
                            ContextCurve.FindPropertyRelative("end").floatValue = 1f;
                            ContextCurve.FindPropertyRelative("tangents").vector4Value = new Vector4(0.25f, 1f, 0.75f, 1f);
                            break;
                        }
                    default: break;
                }
                ContextCurve.serializedObject.ApplyModifiedProperties();
            }
        }

        public static void Reset()
        {
            NeedsRepaint = false;
            SelectedBezierGUICurvePoint = BezierSelection.CountOf;
            controlID = "-1";
        }

        private static bool DrawCurveRect(Rect position, SerializedProperty property)
        {
            var startValue = property.FindPropertyRelative("start").floatValue;
            var endValue = property.FindPropertyRelative("end").floatValue;

            var preValue = new Vector2(startValue, endValue);
            var tangentValues = property.FindPropertyRelative("tangents").vector4Value;
            var tangentsPreValues = tangentValues;

            var startPoint = new Vector2(0, startValue);
            var startTangent = new Vector2(tangentValues.x, tangentValues.y);

            var endTanget = new Vector2(tangentValues.z, tangentValues.w);
            var endPoint = new Vector2(1, endValue);

            var singleLineHeight = EditorGUIUtility.singleLineHeight;
            var header = position.SetHeight(singleLineHeight);

            position = position.OffsetX(singleLineHeight, true);

            EditorGUI.BeginChangeCheck();
            property.isExpanded = EditorGUI.Foldout(header, property.isExpanded, new GUIContent(property.displayName, property.tooltip));
            if (EditorGUI.EndChangeCheck() && !property.isExpanded)
            {
                Reset();
            }

            if (!property.isExpanded) return false;

            var rect = position.OffsetY(singleLineHeight);
            GUI.Box(rect, new GUIContent("", "drag end points to control blending. right-click for options." + property.tooltip), EditorStyles.helpBox);

            rect.height *= 0.8f;
            rect = rect.Scale(0.99f);

            var maxheight = Mathf.Abs(rect.min.y - rect.max.y);
            var maxWidth = Mathf.Abs(rect.min.x - rect.max.x);

            var maxY = Mathf.Max(1f, startPoint.y, endPoint.y, startTangent.y, endTanget.y);
            var maxX = Mathf.Max(1f, startPoint.x, endPoint.x, startTangent.x, endTanget.x);

            Vector2 mStartPoint;
            mStartPoint.x = rect.min.x;
            mStartPoint.y = rect.max.y;
            mStartPoint.y -= startPoint.y / maxY * maxheight;

            Vector2 mEndPoint;
            mEndPoint.x = rect.max.x;
            mEndPoint.y = rect.max.y;
            mEndPoint.y -= endPoint.y / maxY * maxheight;

            Vector2 mStartTanget;
            mStartTanget.x = rect.min.x;
            mStartTanget.y = rect.max.y;

            mStartTanget.x += startTangent.x / maxX * maxWidth;
            mStartTanget.y -= startTangent.y / maxY * maxheight;

            Vector2 mEndTanget;
            mEndTanget.x = rect.min.x;
            mEndTanget.y = rect.max.y;

            mEndTanget.x += endTanget.x / maxX * maxWidth;
            mEndTanget.y -= endTanget.y / maxY * maxheight;

            var currentEvent = Event.current;
            var eventType = currentEvent.type;

            if (eventType == EventType.MouseUp) { Reset(); }

            if (eventType == EventType.ContextClick && rect.Contains(currentEvent.mousePosition))
            {
                BasicCurveContextMenu.ShowMenu(property);

                Reset();
            }

            float minRange = 10;
            float distance = -1;

            if (eventType == EventType.MouseDown)
            {

                if ((currentEvent.mousePosition - mStartTanget).magnitude < minRange)
                {
                    distance = (currentEvent.mousePosition - mStartTanget).magnitude;
                    SelectedBezierGUICurvePoint = BezierSelection.StartTanget;

                }
                else if((currentEvent.mousePosition - mStartPoint).magnitude < minRange)
                {
                    distance = (currentEvent.mousePosition - mStartPoint).magnitude;
                    SelectedBezierGUICurvePoint = BezierSelection.Start;
                }
                else if ((currentEvent.mousePosition - mEndTanget).magnitude < minRange)
                {
                    distance = (currentEvent.mousePosition - mEndTanget).magnitude;
                    SelectedBezierGUICurvePoint = BezierSelection.EndTanget;
                }
                else if ((currentEvent.mousePosition - mEndPoint).magnitude < minRange)
                {
                    distance = (currentEvent.mousePosition - mEndPoint).magnitude;
                    SelectedBezierGUICurvePoint = BezierSelection.End;
                }

                if (distance >= 0f) controlID = property.propertyPath + property.serializedObject.targetObject.GetInstanceID().ToString();
            }

            var currentID = property.propertyPath + property.serializedObject.targetObject.GetInstanceID().ToString();
            if (eventType == EventType.MouseDrag && currentID == controlID)
            {
                var drag = -currentEvent.delta;
                if (drag.sqrMagnitude < 0.01f) drag = Vector2.zero;
                drag.x *= rect.width / (float)Screen.width;
                drag.y *= rect.width / (float)Screen.width;
                drag.x *= 0.005f;
                drag.y *= 0.025f;

                switch (SelectedBezierGUICurvePoint)
                {
                    case BezierSelection.Start:
                        {
                            startValue += drag.y;
                            startValue = Mathf.Clamp(startValue, 0f, 1f);
                        }
                        break;
                    case BezierSelection.StartTanget:
                        {
                            tangentValues.x -= drag.x;
                            tangentValues.x = Mathf.Clamp(tangentValues.x, 0f, 1f);

                            tangentValues.y += drag.y;
                            tangentValues.y = Mathf.Clamp(tangentValues.y, 0f, 1f);

                        }
                        break;
                    case BezierSelection.EndTanget:
                        {
                            tangentValues.z -= drag.x;
                            tangentValues.z = Mathf.Clamp(tangentValues.z, 0f, 1f);

                            tangentValues.w += drag.y;
                            tangentValues.w = Mathf.Clamp(tangentValues.w, 0f, 1f);

                        }
                        break;
                    case BezierSelection.End:
                        {
                            endValue += drag.y;
                            endValue = Mathf.Clamp(endValue, 0f, 1f);
                        }
                        break;
                    default:
                        break;

                }
                //Debug.Log("DrawCurveRect Drag");
            }

            Handles.BeginGUI();
            {
                var handleColor = Handles.color;

                var lineColor = SkinnEx.DefaultColors.BlueOverlayDark;
                Handles.DrawBezier(mStartPoint, mEndPoint, mStartTanget, mEndTanget, lineColor, null, 4f);

                if ((mStartPoint - mStartTanget).sqrMagnitude > 0.001f)
                    Handles.DrawBezier(mStartPoint, mStartTanget, mStartPoint, mStartTanget, lineColor, null, 2f);

                if ((mEndPoint - mEndTanget).sqrMagnitude > 0.001f)
                    Handles.DrawBezier(mEndPoint, mEndTanget, mEndPoint, mEndTanget, lineColor, null, 2f);

                Handles.color = Color.black;

                Handles.DrawWireDisc(mStartPoint, Vector3.forward, 1f);
                Handles.DrawWireDisc(mEndPoint, Vector3.forward, 1f);
                Handles.DrawWireDisc(mStartTanget, Vector3.forward, 1f);
                Handles.DrawWireDisc(mEndTanget, Vector3.forward, 1f);

                var labelStyle = EditorStyles.wordWrappedMiniLabel;
                var startLabel = mStartPoint;
                var startTangetLabel = mStartTanget;

                if (startTangent.x > 0.8f) startTangetLabel.x -= 50f;

                if ((startLabel - startTangetLabel).magnitude > 60f)
                    Handles.Label(startLabel, new GUIContent(startPoint.y.ToString("0.00")), labelStyle);

                Handles.Label(startTangetLabel, new GUIContent(startTangent.ToString("0.00")), labelStyle);

                var endTangetLabel = mEndTanget;
                if(endTanget.x > 0.2f) endTangetLabel.x -= 50f;

                if ((startTangetLabel - endTangetLabel).magnitude > 60f)
                    Handles.Label(endTangetLabel, new GUIContent(endTanget.ToString("0.00")), labelStyle);

                var endLabel = mEndPoint;
                endLabel.x -= 50f;

                if((endLabel - endTangetLabel).magnitude > 60f && (endLabel - startLabel).magnitude > 60f)
                    Handles.Label(endLabel, new GUIContent(endPoint.y.ToString("0.00")), labelStyle);

                Handles.color = handleColor;
            }
            Handles.EndGUI();

            var changed = (tangentValues - tangentsPreValues).sqrMagnitude > 0f || (new Vector2(startValue, endValue) - preValue).sqrMagnitude > 0f;
            if (changed)
            {
                startValue = startValue.FormatFloat(2);
                endValue = endValue.FormatFloat(2);
                tangentValues.x = tangentValues.x.FormatFloat(2);
                tangentValues.y = tangentValues.y.FormatFloat(2);
                tangentValues.z = tangentValues.z.FormatFloat(2);
                tangentValues.w = tangentValues.w.FormatFloat(2);

                property.FindPropertyRelative("start").floatValue = startValue;
                property.FindPropertyRelative("end").floatValue = endValue;
                property.FindPropertyRelative("tangents").vector4Value = tangentValues;
                property.serializedObject.ApplyModifiedProperties();
            }
            return changed;
        }
    }
}