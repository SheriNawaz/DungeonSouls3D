using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using CWM.VeiwTree;

namespace CWM.Skinn
{
    public partial class VertexMapperInspector
    {
        private class BoneTreeGUI
        {
            private BoneTreeView treeview;
            private SearchField searchFeild;

            private VertexMapper vm;
            private ElementFilter filter;

            private string kSession = "";
            const string kSessionStateKeyPrefix = "BoneTree342562";
            const float topToolbarHeight = 20f;
            const float spacing = 2f;
            const float startingIndent = 30f;

            public float GetPropertyHeight()
            {
                float lineHeight = EditorGUIUtility.singleLineHeight;
                if (!treeview) return lineHeight;
                float totalHeight = treeview.totalHeight + topToolbarHeight + lineHeight;
                return totalHeight;
            }

            private void SetupTreeVeiw(ElementFilter filter, string stateHash)
            {
                var bones = filter.elements;
                string hashName = stateHash;
                if (treeview && hashName == kSession) return;
                if (treeview && !string.IsNullOrEmpty(kSession)) SessionState.SetString(kSession, JsonUtility.ToJson(treeview.state));

                kSession = hashName;

                var treeViewState = new TreeViewState();
                var jsonState = SessionState.GetString(kSession, "");
                if (!string.IsNullOrEmpty(jsonState)) JsonUtility.FromJsonOverwrite(jsonState, treeViewState);
                var treeModel = new TreeModel<BoneElement>(bones);
                treeview = new BoneTreeView(treeViewState, treeModel);

                treeview.OnBeforeDroppingDraggedItems += OnBeforeDroppingDraggedItems;
                treeview.OnTreeChanged += OnTreeChanged;
                treeview.OnSelectionChanged += OnTreeSelectionChanged;

                searchFeild = new SearchField();
                searchFeild.downOrUpArrowKeyPressed += treeview.SetFocusAndEnsureSelectedItem;

                treeview.OnElementRow -= RowGUI;
                treeview.OnElementRow += RowGUI;
                SkinnGizmos.OnSceneGUI -= SceneGUI;
                SkinnGizmos.OnSceneGUI += SceneGUI;

                treeview.Reload();
                treeview.ExpandAll();
            }

            private void SceneGUI()
            {
                if (!vm || !filter || SkinnEx.IsNullOrEmpty(filter.elements)) return;
            }

            private void RowGUI(BoneTreeView.ElementRow args)
            {
                if (!vm || !filter) return;
                if (args.isRenaming) return;

                Event evt = Event.current;
                int id = args.item.id;
                Rect rowRect = args.rowRect.OffsetX(args.contentIndent);
                if (evt.type == EventType.ContextClick && rowRect.Contains(evt.mousePosition))
                {
                    treeview.DoSelectionClick(args.item, true);
                    ShowContextMenu();
                }

                var bones = filter.elements; if (SkinnEx.IsNullOrEmpty(bones)) return;
                int index = filter.elements.GetBoneTreeElementIndex(id, true); if (index < 0) return;
                var bone = bones[index];

                string info = "(enabled)";
                if (id == bone.orginalIndex) info = "(not available)";
                else if (!bone.enabled) info = "(disabled)";

                GUIContent contentLabel = new GUIContent(bone.Name + "    " + info, "right click for options.");
                GUIStyle style = bone.enabled ? EditorStyles.label : new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Italic };
                EditorGUI.LabelField(rowRect, contentLabel, style);
            }

            public void OnGUI(Rect position, VertexMapper vm, string stateHash = "")
            {
                this.vm = vm;
                var skinnedMesh = vm.Source;

                float labelHeight = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(position.SetHeight(labelHeight).OffsetX(30)
                    , "Bones: "+ SkinnEx.EnforceObjectName(skinnedMesh), EditorStyles.miniLabel);

                if (!skinnedMesh || SkinnEx.IsNullOrEmpty(skinnedMesh.bones))
                {
                    return;
                }

                filter = vm.boneFilter;

                if (!filter.IsValidTransforms(skinnedMesh.bones))
                {
                    filter.GenerateAsBones(skinnedMesh);
                    filter.assetID = vm.sourceAsset.assetID;
                    //filter.elements = skinnedMesh.GenerateTransformTree();

                    treeview = null;

                    if (treeview)
                    {
                        treeview.treeModel.SetData(filter.elements);
                        treeview.Reload();
                        treeview.ExpandAll();
                    }
                }

                SetupTreeVeiw(filter, GetStateHash(vm.sourceAsset.assetID + stateHash));

                Rect rect = position.OffsetX(startingIndent, true);
                Rect searchRect = rect.OffsetY(labelHeight - 1f).SetHeight(topToolbarHeight).GetRectX(false);
                Rect treeRect = rect.OffsetY(labelHeight + topToolbarHeight, true);
                treeview.searchString = searchFeild.OnGUI(searchRect, treeview.searchString);

                treeview.OnGUI(treeRect);
            }


            private static string GetStateHash(string id)
            {
                return kSessionStateKeyPrefix + id;
            }


            public void OnUndoRedoPerformed()
            {
                if (!treeview || !filter) return;

                treeview.treeModel.SetData(filter.elements);
                treeview.Reload();
            }

            private void OnTreeChanged()
            {
                if (!treeview || !filter || !vm) return;
                Undo.RecordObject(vm, string.Format("{0} Hierarchy Adjusted", SkinnEx.EnforceString(filter.name)));
            }

            private void OnGizmoSelectionChanged()
            {
                if (!treeview) return;

                //Debug.Log("OnTreeChanged");
            }


            private void OnTreeSelectionChanged()
            {
                if (!treeview) return;
                //Debug.Log("OnSelectionChanged");
            }

            private void OnBeforeDroppingDraggedItems(IList<TreeViewItem> draggedRows)
            {
                if (!treeview || !filter || !vm) return;
                Undo.RecordObject(vm, string.Format("{0} Hierarchy Adjusted", SkinnEx.EnforceString(filter.name)));
                //Debug.Log("OnBeforeDroppingDraggedItems");
            }

            public void ShowContextMenu()
            {
                GenericMenu menu = new GenericMenu();
                // string id = contextCommandID;
                for (int i = 0; i < ContextEnableDisableCommand.Commands.Length; i++)
                {
                    string commandID = ContextEnableDisableCommand.Commands[i];
                    if (commandID.EndsWith("/")) { menu.AddSeparator(commandID); continue; }
                    menu.AddItem(new GUIContent(commandID), false, ContextMenuSelected, commandID);
                }
                menu.ShowAsContext();
            }

            private static class ContextEnableDisableCommand
            {
                public const string depth = "";
                public static readonly string[] Commands = new string[]
                {
                 depth + "Disable", depth + "DisableChildren",  depth + "DisableAll",
                 depth + "/",
                 depth + "Enable", depth + "EnableChildren",  depth + "EnableAll"
                };
            }

            private void ContextMenuSelected(object contextCommandID)
            {

                string id;
                try { id = contextCommandID as string; } catch { id = ""; }
                if (!vm || !filter || !treeview) return;
                switch (id)
                {
                    case ContextEnableDisableCommand.depth + "Disable":
                        {
                            var bones = GetSelectedBones();
                            for (int i = 0; i < bones.Count; i++) bones[i].enabled = false;
                            break;
                        }
                    case ContextEnableDisableCommand.depth + "DisableChildren":
                        {
                            var bones = GetSelectedBones();
                            for (int i = 0; i < bones.Count; i++)
                            {
                                var children = bones[i].GetChildern(false);
                                for (int ii = 0; ii < children.Count; ii++) children[ii].enabled = false;
                            }
                        }
                        break;
                    case ContextEnableDisableCommand.depth + "DisableAll":
                        {
                            var bones = filter.elements;
                            for (int i = 0; i < bones.Count; i++) bones[i].enabled = false;
                        }
                        break;
                    case ContextEnableDisableCommand.depth + "Enable":
                        {
                            var bones = GetSelectedBones();
                            for (int i = 0; i < bones.Count; i++) bones[i].enabled = true;
                            break;
                        }
                    case ContextEnableDisableCommand.depth + "EnableChildren":
                        {
                            var bones = GetSelectedBones();
                            for (int i = 0; i < bones.Count; i++)
                            {
                                var children = bones[i].GetChildern(false);
                                for (int ii = 0; ii < children.Count; ii++) children[ii].enabled = true;
                            }
                        }
                        break;
                    case ContextEnableDisableCommand.depth + "EnableAll":
                        {
                            var bones = filter.elements;
                            for (int i = 0; i < bones.Count; i++) bones[i].enabled = true;
                        }
                        break;
                    default: break;
                }

                filter.Update();
                OnTreeChanged();
            }

            private List<BoneElement> GetSelectedBones()
            {
                List<BoneElement> selection = new List<BoneElement>();
                if (!vm || !treeview || !treeview.HasSelection()) return selection;
                IList<int> selectionHashes = treeview.GetSelection();
                for (int i = 0; i < selectionHashes.Count; i++)
                { var bone = filter.elements.GetBoneTreeElement(selectionHashes[i], true); if (bone) selection.Add(bone); }
                return selection;
            }

        }

    }
}
