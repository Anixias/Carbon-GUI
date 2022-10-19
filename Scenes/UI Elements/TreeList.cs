using Glint.Collections;
using Godot;
using System.Collections.Generic;

public class TreeList : MarginContainer
{
	[Signal]
	public delegate void ListItemPressed(TreeListItem listItem);

	[Signal]
	public delegate void ListItemDoubleClicked(TreeListItem listItem);

	[Signal]
	public delegate void ListItemDeleted(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex);

	[Signal]
	public delegate void ListItemRenamed(TreeListItem listItem, string newName);

	[Signal]
	public delegate void ListItemCollapseChanged(TreeListItem listItem, bool collapsed);

	[Signal]
	public delegate void ListItemMoved(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex, int newLocalIndex);

	[Signal]
	public delegate void ListItemOverridingChanged(TreeListItem listItem, bool overriding);

	[Signal]
	public delegate void NoItemPressed();

	public TreeListItem[] ListItems
	{
		get
		{
			return tree.ToArray();
		}
	}

	private PackedScene treeListItemScene;

	private MarginScrollContainer scrollContainer;
	private VBoxContainer vBoxContainer;
	private Tree<TreeListItem> tree;

	public override void _Ready()
	{
		scrollContainer = GetNode<MarginScrollContainer>("MarginScrollContainer");
		vBoxContainer = GetNode<VBoxContainer>("MarginScrollContainer/MarginContainer/VBoxContainer");
		treeListItemScene = ResourceLoader.Load<PackedScene>("res://TreeListItem.tscn");

		scrollContainer.ContainingTreeList = this;

		tree = new Tree<TreeListItem>();
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mbEvent)
		{
			if (mbEvent.Pressed && mbEvent.ButtonIndex == (int)ButtonList.Left)
			{
				EmitSignal(nameof(NoItemPressed));
			}
		}
	}

	public async void UpdateGraphics(bool repeat = true)
	{
		await ToSignal(GetTree(), "idle_frame");

		foreach (var item in tree)
		{
			item.UpdateRelationshipPoints();
		}

		if (repeat)
		{
			UpdateGraphics(false);
		}
	}

	public override bool CanDropData(Vector2 position, object data)
	{
		return false;
	}

	private void UpdateIndices()
	{
		// Update all list item's indices
		for (int idx = 0; idx < tree.Count; idx++)
		{
			var item = tree[idx];
			item.ListIndex = idx;
		}
	}

	public TreeListItem AddListItem(TreeListItem parent = null, string text = "", Texture icon = null, Texture iconCollapsed = null, bool canContainItems = true)
	{
		var listItem = treeListItemScene.Instance() as TreeListItem;

		while (parent != null && !parent.CanContainItems)
		{
			// If the parent node is not allowed to contain items, go up the tree hierarchy until an ancestor can contain items or it hits null
			parent = parent.Parent;
		}

		if (parent != null)
		{
			var idx = parent.ListIndex + parent.ChildCount;

			listItem.Parent = parent;
			listItem.ListIndex = idx;

			vBoxContainer.AddChildBelowNode(tree[idx], listItem);
			tree.Add(listItem, parent);

			// Update all list item's indices
			UpdateIndices();

			parent.UpdateRelationshipPoints();
		}
		else
		{
			listItem.Parent = null;

			listItem.ListIndex = tree.Count;
			vBoxContainer.AddChild(listItem);
			tree.Add(listItem);
		}

		listItem.Text = text;
		listItem.Icon = icon;
		listItem.IconCollapsed = iconCollapsed;
		listItem.CanContainItems = canContainItems;
		listItem.Collapsed = listItem.Collapsed;
		listItem.ContainingTreeList = this;

		listItem.Connect(nameof(TreeListItem.Deleted), this, nameof(OnListItemDeleted));
		listItem.Connect(nameof(TreeListItem.Renamed), this, nameof(OnListItemRenamed));
		listItem.Connect(nameof(TreeListItem.TextInputChanged), this, nameof(OnListItemTextInputChanged));
		listItem.Connect(nameof(TreeListItem.ButtonPressed), this, nameof(OnListItemPressed));
		listItem.Connect(nameof(TreeListItem.DoubleClicked), this, nameof(OnListItemDoubleClicked));
		listItem.Connect(nameof(TreeListItem.CollapseChanged), this, nameof(OnListItemCollapseChanged));
		listItem.Connect(nameof(TreeListItem.OverridingChanged), this, nameof(OnListItemOverridingChanged));

		return listItem;
	}

	public void RemoveListItem(TreeListItem listItem)
	{
		if (listItem == null || !Godot.Object.IsInstanceValid(listItem))
			return;

		vBoxContainer.RemoveChild(listItem);
		listItem.Deselect();
		listItem.QueueFree();

		foreach (var child in listItem.GetAllChildren())
		{
			vBoxContainer.RemoveChild(child);
			child.Deselect();
			child.QueueFree();
		}

		listItem.Children.Clear();

		tree.Remove(listItem);

		UpdateIndices();

		listItem.Parent?.UpdateRelationshipPoints();
		listItem.Parent = null;
	}

	/// <summary>
	/// Moves <paramref name="listItem"/> to the local index <paramref name="destIndex"/> within its parent <see cref="TreeListItem"/>.
	/// </summary>
	/// <param name="listItem">The <see cref="TreeListItem"/> to move.</param>
	/// <param name="destIndex">The local index within its parent to move <paramref name="listItem"/> to.</param>
	/// <param name="emitSignal">If <see langword="true"/>, emits a signal after moving <paramref name="listItem"/>.</param>
	public void MoveListItem(TreeListItem listItem, int destIndex, bool emitSignal = true)
	{
		// Moves listItem to destIndex local index within its parent
		var node = tree.Find(listItem);
		if (node == null)
			return;

		var oldParent = listItem.Parent;
		var oldLocalIndex = node.GetLocalIndex();

		if (tree.Move(node, destIndex))
		{
			if (destIndex < 0)
			{
				destIndex = node.Parent.ChildCount - 1;
			}

			listItem.SetParent(listItem.Parent, destIndex);
			HandleItemMovement(listItem, oldParent, oldLocalIndex, node.GetLocalIndex(), emitSignal);
		}
	}

	public void MoveListItem(TreeListItem listItem, TreeListItem destItem, bool above, bool emitSignal = true)
	{
		// Move listItem above or below destItem
		var node = tree.Find(listItem);
		if (node == null)
			return;

		var oldParent = listItem.Parent;
		var oldLocalIndex = node.GetLocalIndex();

		int idx;

		if (tree.Move(listItem, destItem, above, out idx))
		{
			listItem.SetParent(destItem.Parent, idx);
			HandleItemMovement(listItem, oldParent, oldLocalIndex, idx, emitSignal);
		}
	}

	public void ReparentListItem(TreeListItem listItem, TreeListItem parent, int newLocalIndex = -1, bool emitSignal = true)
	{
		var node = tree.Find(listItem);
		if (node == null)
			return;

		var oldParent = listItem.Parent;
		var oldLocalIndex = node.GetLocalIndex();

		if (tree.Reparent(node, tree.Find(parent), newLocalIndex))
		{
			listItem.SetParent(parent);
			HandleItemMovement(listItem, oldParent, oldLocalIndex, node.GetLocalIndex(), emitSignal);
		}
	}

	private void HandleItemMovement(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex, int newLocalIndex, bool emitSignal = true)
	{
		var oldListIdx = listItem.ListIndex;
		var tempList = new List<TreeListItem>();

		tempList.Add(listItem);
		vBoxContainer.RemoveChild(listItem);

		foreach (var child in listItem.GetAllChildren())
		{
			tempList.Add(child);
			vBoxContainer.RemoveChild(child);
		}

		tempList.Remove(listItem);
		vBoxContainer.AddChild(listItem);

		if (listItem.Parent != null)
		{
			var idx = 0;

			for (var i = 0; i < newLocalIndex; i++)
			{
				idx += 1 + listItem.Parent.Children[i].ChildCount;
			}

			vBoxContainer.MoveChild(listItem, listItem.Parent.GetIndex() + 1 + idx);
		}
		else if (newLocalIndex >= 0)
		{
			var idx = 0;

			for (var i = 0; i < newLocalIndex; i++)
			{
				idx += 1 + tree[i].ChildCount;
			}

			vBoxContainer.MoveChild(listItem, newLocalIndex);
		}

		var prevNode = listItem;
		while (tempList.Count > 0)
		{
			vBoxContainer.AddChildBelowNode(prevNode, tempList[0]);
			prevNode = tempList[0];
			tempList.RemoveAt(0);
		}

		// Update all item's local indices
		UpdateIndices();

		Refresh();

		if (emitSignal)
		{
			EmitSignal(nameof(ListItemMoved), listItem, oldParent, oldLocalIndex, newLocalIndex);
		}
	}

	public void Clear()
	{
		tree.Clear();

		foreach (Node child in vBoxContainer.GetChildren())
		{
			vBoxContainer.RemoveChild(child);
			child.QueueFree();
		}
	}

	/*private void RemoveIndex(int listIndex, bool update = true, bool emitSignal = true)
    {
        if (listIndex < 0) return;
        if (listIndex >= tree.Count) return;
        
        var treeListItem = tree[listIndex];
        if (treeListItem == null) return;
        
        treeListItem.ContainingTreeList = null;
        
        // First remove children in reverse order
        for(var childIdx = treeListItem.Children.Count - 1; childIdx >= 0; childIdx = treeListItem.Children.Count - 1)
        {
            var childItem = treeListItem.Children[childIdx];
            
            if (childItem != null)
            {
                RemoveIndex(childItem.ListIndex, false, emitSignal);
            }
        }
        
        listItems.RemoveAt(listIndex);
        tree.Remove(treeListItem);
        
        if (treeListItem.Parent != null)
        {
            treeListItem.Parent.Children.Remove(treeListItem);
            treeListItem.Parent.UpdateState();
        }
        
        vBoxContainer.RemoveChild(treeListItem);
        treeListItem.QueueFree();
        
        for(int idx = 0; idx < listItems.Count; idx++)
        {
            listItems[idx].ListIndex = idx;
        }
        
        if (update)
        {
            // Update all list item's indices
            for(var idx = 0; idx < listItems.Count; idx++)
            {
                var item = listItems[idx];
                item.ListIndex = idx;
            }
        }
        
        if (emitSignal)
        {
            EmitSignal(nameof(ListItemDeleted), listIndex);
        }
    }*/

	public void Print()
	{
		GD.Print(tree);
	}

	public void OnListItemDeleted(TreeListItem listItem)
	{
		var node = tree.Find(listItem);
		if (node == null)
			return;

		var oldParent = listItem.Parent;
		var oldLocalIndex = node.GetLocalIndex();

		RemoveListItem(listItem);
		EmitSignal(nameof(ListItemDeleted), listItem, oldParent, oldLocalIndex);
	}

	public void OnListItemRenamed(TreeListItem listItem, string newName)
	{
		EmitSignal(nameof(ListItemRenamed), listItem, newName);
	}

	public void OnListItemTextInputChanged(TreeListItem listItem, string newName)
	{
		//scrollContainer.EnsureControlVisible(vBoxContainer);
	}

	public void OnListItemPressed(TreeListItem listItem)
	{
		EmitSignal(nameof(ListItemPressed), listItem);
	}

	public void OnListItemDoubleClicked(TreeListItem listItem)
	{
		EmitSignal(nameof(ListItemDoubleClicked), listItem);
	}

	public void OnListItemCollapseChanged(TreeListItem listItem, bool collapsed)
	{
		EmitSignal(nameof(ListItemCollapseChanged), listItem, collapsed);
	}

	public void OnListItemOverridingChanged(TreeListItem listItem, bool overriding)
	{
		EmitSignal(nameof(ListItemOverridingChanged), listItem, overriding);
	}

	public void Refresh()
	{
		scrollContainer.Refresh();
	}
}
