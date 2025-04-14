using UnityEngine;
using UnityEditor.IMGUI.Controls;
using CWM.VeiwTree;
using System;

namespace CWM.Skinn
{
    internal class BoneTreeView : TreeViewWithTreeModel<BoneElement>
    {
        public BoneTreeView(TreeViewState state, TreeModel<BoneElement> model) : base(state, model)
        {
            showBorder = false;
            showAlternatingRowBackgrounds = true;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var row = CreateElementRow(args);
            row.contentIndent = GetContentIndent(args.item);
            if (OnElementRow != null) OnElementRow(row);
        }

        public void DoSelectionClick(TreeViewItem item, bool keelMultiSelection = false) { SelectionClick(item, keelMultiSelection); }

        private static ElementRow CreateElementRow(RowGUIArgs args)
        {
            var row = new ElementRow
            {
                item = args.item,
                label = args.label,
                rowRect = args.rowRect,
                row = args.row,
                selected = args.selected,
                focused = args.focused,
                isRenaming = args.isRenaming
            };
            return row;
        }

        public struct ElementRow
        {
            public TreeViewItem item;
            public string label;
            public Rect rowRect;
            public int row;
            public bool selected;
            public bool focused;
            public bool isRenaming;
            public float contentIndent;
        }

        public Action<ElementRow> OnElementRow;

        public static implicit operator bool(BoneTreeView value) { return value != null; }
    }
}
