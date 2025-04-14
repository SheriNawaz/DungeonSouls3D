/// <summary>
/// VeiwTree implementation based of the UnityEditor.IMGUI.Controls examples.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CWM.VeiwTree
{

    internal class TreeViewItem<T> : TreeViewItem where T : TreeElement
	{
		public T data { get; set; }

		public TreeViewItem (int id, int depth, string displayName, T data) : base (id, depth, displayName)
		{
			this.data = data;
		}
	}

	internal class TreeViewWithTreeModel<T> : TreeView where T : TreeElement
	{
		TreeModel<T> m_TreeModel;
		readonly List<TreeViewItem> m_Rows = new List<TreeViewItem>(100);
		public event Action OnTreeChanged;

        public event Action OnSelectionChanged;

        public TreeModel<T> treeModel { get { return m_TreeModel; } }
		public event Action<IList<TreeViewItem>>  OnBeforeDroppingDraggedItems;


		public TreeViewWithTreeModel (TreeViewState state, TreeModel<T> model) : base (state)
		{
			Init (model);
		}

		public TreeViewWithTreeModel (TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model)
			: base(state, multiColumnHeader)
		{
			Init (model);
		}

		void Init (TreeModel<T> model)
		{
			m_TreeModel = model;
			m_TreeModel.modelChanged += ModelChanged;
		}

		void ModelChanged ()
		{

			if (OnTreeChanged != null)
				OnTreeChanged ();

			Reload ();
		}

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (OnSelectionChanged != null)
                OnSelectionChanged();
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return true;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if(args.newName.Length > 0)
                args.acceptedRename = true;

            if (args.acceptedRename)
            {
                var element = treeModel.Find(args.itemID);
                element.Name = args.newName;
                Reload();
            }
        }

        protected override TreeViewItem BuildRoot()
		{
			int depthForHiddenRoot = -1;
			return new TreeViewItem<T>(m_TreeModel.root.ID, depthForHiddenRoot, m_TreeModel.root.Name, m_TreeModel.root);
		}

		protected override IList<TreeViewItem> BuildRows (TreeViewItem root)
		{
			if (m_TreeModel.root == null)
			{
				Debug.LogError ("tree model root is null. did you call SetData()?");
			}

			m_Rows.Clear ();
			if (!string.IsNullOrEmpty(searchString))
			{
				Search (m_TreeModel.root, searchString, m_Rows);
			}
			else
			{
				if (m_TreeModel.root.HasChildren)
					AddChildrenRecursive(m_TreeModel.root, 0, m_Rows);
			}

			// We still need to setup the child parent information for the rows since this
			// information is used by the TreeView internal logic (navigation, dragging etc)
			SetupParentsAndChildrenFromDepths (root, m_Rows);

			return m_Rows;
		}

		void AddChildrenRecursive (T parent, int depth, IList<TreeViewItem> newRows)
		{
			foreach (T child in parent.Children)
			{
				var item = new TreeViewItem<T>(child.ID, depth, child.Name, child);
				newRows.Add(item);

				if (child.HasChildren)
				{
					if (IsExpanded(child.ID))
					{
						AddChildrenRecursive (child, depth + 1, newRows);
					}
					else
					{
						item.children = CreateChildListForCollapsedParent();
					}
				}
			}
		}

		void Search(T searchFromThis, string search, List<TreeViewItem> result)
		{
			if (string.IsNullOrEmpty(search))
				throw new ArgumentException("Invalid search: cannot be null or empty", "search");

			const int kItemDepth = 0; // tree is flattened when searching

			Stack<T> stack = new Stack<T>();
			foreach (var element in searchFromThis.Children)
				stack.Push((T)element);
			while (stack.Count > 0)
			{
				T current = stack.Pop();
				// Matches search?
				if (current.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					result.Add(new TreeViewItem<T>(current.ID, kItemDepth, current.Name, current));
				}

				if (current.Children != null && current.Children.Count > 0)
				{
					foreach (var element in current.Children)
					{
						stack.Push((T)element);
					}
				}
			}
			SortSearchResult(result);
		}

		protected virtual void SortSearchResult (List<TreeViewItem> rows)
		{
			rows.Sort ((x,y) => EditorUtility.NaturalCompare (x.displayName, y.displayName)); // sort by displayName by default, can be override for multicolumn solutions
		}

		protected override IList<int> GetAncestors (int id)
		{
			return m_TreeModel.GetAncestors(id);
		}

		protected override IList<int> GetDescendantsThatHaveChildren (int id)
		{
			return m_TreeModel.GetDescendantsThatHaveChildren(id);
		}


		// Dragging
		//-----------

		const string k_GenericDragID = "GenericDragColumnDragging";

		protected override bool CanStartDrag (CanStartDragArgs args)
		{
			return true;
		}

		protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
		{
			if (hasSearch)
				return;

			DragAndDrop.PrepareStartDrag();
			var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
			DragAndDrop.SetGenericData(k_GenericDragID, draggedRows);
			DragAndDrop.objectReferences = new UnityEngine.Object[] { }; // this IS required for dragging to work
			string title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
			DragAndDrop.StartDrag (title);
		}

		protected override DragAndDropVisualMode HandleDragAndDrop (DragAndDropArgs args)
		{
			// Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
			var draggedRows = DragAndDrop.GetGenericData(k_GenericDragID) as List<TreeViewItem>;
			if (draggedRows == null)
				return DragAndDropVisualMode.None;

			// Parent item is null when dragging outside any tree view items.
			switch (args.dragAndDropPosition)
			{
				case DragAndDropPosition.UponItem:
				case DragAndDropPosition.BetweenItems:
					{
						bool validDrag = ValidDrag(args.parentItem, draggedRows);
						if (args.performDrop && validDrag)
						{
							T parentData = ((TreeViewItem<T>)args.parentItem).data;
							OnDropDraggedElementsAtIndex(draggedRows, parentData, args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
						}
						return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
					}

				case DragAndDropPosition.OutsideItems:
					{
						if (args.performDrop)
							OnDropDraggedElementsAtIndex(draggedRows, m_TreeModel.root, m_TreeModel.root.Children.Count);

						return DragAndDropVisualMode.Move;
					}
				default:
					Debug.LogError("Unhanded enum " + args.dragAndDropPosition);
					return DragAndDropVisualMode.None;
			}
		}

		public virtual void OnDropDraggedElementsAtIndex (List<TreeViewItem> draggedRows, T parent, int insertIndex)
		{
			if (OnBeforeDroppingDraggedItems != null)
				OnBeforeDroppingDraggedItems (draggedRows);

			var draggedElements = new List<TreeElement> ();
			foreach (var x in draggedRows)
				draggedElements.Add (((TreeViewItem<T>) x).data);

			var selectedIDs = draggedElements.Select (x => x.ID).ToArray();
			m_TreeModel.MoveElements (parent, insertIndex, draggedElements);
			SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
		}


		bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
		{
			TreeViewItem currentParent = parent;
			while (currentParent != null)
			{
				if (draggedItems.Contains(currentParent))
					return false;
				currentParent = currentParent.parent;
			}
			return true;
		}


        // Custom GUI

        protected override void RowGUI(RowGUIArgs args)
        {
            //Event evt = Event.current;
           // extraSpaceBeforeIconAndLabel = 1f;

            // GameObject isStatic toggle 
            //var gameObject = GetGameObject(args.item.id);
            //if (gameObject == null)
            //    return;

            //Rect toggleRect = args.rowRect;
            //toggleRect.x += GetContentIndent(args.item);
            //toggleRect.width = 16f;

            //// Ensure row is selected before using the toggle (usability)
            //if (evt.type == EventType.MouseDown && toggleRect.Contains(evt.mousePosition))
            //    SelectionClick(args.item, false);

            //EditorGUI.BeginChangeCheck();
            //bool isStatic = EditorGUI.Toggle(toggleRect, gameObject.isStatic);
            //if (EditorGUI.EndChangeCheck())
            //    gameObject.isStatic = isStatic;

            // Text
            base.RowGUI(args);
        }

    }

}
