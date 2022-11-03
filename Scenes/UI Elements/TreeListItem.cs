using Godot;
using System.Collections.Generic;

public class TreeListItem : MarginContainer
{
	[Signal]
	public delegate void ButtonPressed(TreeListItem listItem);

	[Signal]
	public delegate void DoubleClicked(TreeListItem listItem);

	[Signal]
	public delegate void Renamed(TreeListItem listItem, string newName);

	[Signal]
	public delegate void TextInputChanged(TreeListItem listItem, string newName);

	[Signal]
	public delegate void Deleted(TreeListItem listItem);

	[Signal]
	public delegate void CollapseChanged(TreeListItem listItem, bool collapsed);

	[Signal]
	public delegate void OverridingChanged(TreeListItem listItem, bool overriding);

	public ListItem ListItemControl
	{
		get => listItem;
	}

	[Export]
	public bool Collapsed
	{
		get => collapsed;
		set
		{
			var oc = collapsed;
			collapsed = value;

			(Parent ?? this).UpdateState();

			if (collapsed != oc)
			{
				EmitSignal(nameof(CollapseChanged), this, collapsed);
			}
		}
	}

	public bool ParentCollapsed
	{
		get
		{
			if (parent != null)
			{
				return (parent.Collapsed || parent.ParentCollapsed);
			}

			return false;
		}
	}

	[Export]
	public string Text
	{
		get
		{
			if (listItem != null)
			{
				return listItem.Text;
			}

			return "";
		}
		set
		{
			if (listItem != null)
			{
				listItem.Text = value;
			}
		}
	}

	[Export]
	public Texture Icon
	{
		get => icon;
		set
		{
			icon = value;
		}
	}

	[Export]
	public Texture IconCollapsed
	{
		get => iconCollapsed;
		set
		{
			iconCollapsed = value;
		}
	}

	[Export]
	public bool KeepPressedOutside
	{
		get
		{
			if (listItem != null)
			{
				return listItem.KeepPressedOutside;
			}

			return false;
		}
		set
		{
			if (listItem != null)
			{
				listItem.KeepPressedOutside = value;
			}
		}
	}

	[Export]
	public bool TintIcon
	{
		get
		{
			if (listItem != null)
			{
				return listItem.TintIcon;
			}

			return true;
		}
		set
		{
			if (listItem != null)
			{
				listItem.TintIcon = value;
			}
		}
	}

	[Export]
	public bool Enabled
	{
		get
		{
			if (listItem != null)
			{
				return listItem.Enabled;
			}

			return true;
		}
		set
		{
			if (listItem != null)
			{
				listItem.Enabled = !value;
			}
		}
	}

	[Export]
	public bool CanBeRenamed
	{
		get
		{
			if (listItem != null)
			{
				return listItem.CanBeRenamed;
			}

			return true;
		}
		set
		{
			if (listItem != null)
			{
				listItem.CanBeRenamed = value;
			}
		}
	}

	[Export]
	public bool CanBeDeleted
	{
		get
		{
			if (listItem != null)
			{
				return listItem.CanBeDeleted;
			}

			return true;
		}
		set
		{
			if (listItem != null)
			{
				listItem.CanBeDeleted = value;
			}
		}
	}

	public int ListIndex
	{
		get => listIndex;
		set
		{
			listIndex = value;
		}
	}

	public int LocalIndex
	{
		get
		{
			if (parent != null)
			{
				return parent.children.FindIndex(match => match == this);
			}

			return -1;
		}
	}

	public TreeListItem Parent
	{
		get => parent;
		set
		{
			if (parent != null)
			{
				parent.RemoveChildItem(this);
			}

			parent = value;

			if (parent != null)
			{
				parent.AddChildItem(this);
				NestLevel = parent.NestLevel + 1;
			}
			else
			{
				NestLevel = 0;
			}
		}
	}

	public int NestLevel
	{
		get => nestLevel;
		set
		{
			nestLevel = value;

			if (relationship != null)
			{
				var scale = 15;

				var rect = relationship.RectSize;
				rect.x = scale * value;
				relationship.RectSize = rect;

				rect = relationship.RectMinSize;
				rect.x = scale * value;
				relationship.RectMinSize = rect;
			}

			foreach (var child in children)
			{
				child.NestLevel = nestLevel + 1;
			}
		}
	}

	public List<TreeListItem> Children
	{
		get => children;
	}

	public int ChildCount
	{
		get
		{
			var val = Children.Count;

			foreach (var child in Children)
			{
				val += child.ChildCount;
			}

			return val;
		}
	}

	[Export]
	public bool CanContainItems
	{
		get => canContainItems;
		set
		{
			canContainItems = value;

			UpdateState();
		}
	}

	[Export]
	public bool Selected
	{
		get
		{
			if (listItem != null)
			{
				return listItem.Selected;
			}

			return false;
		}
		set
		{
			if (listItem != null)
			{
				listItem.Selected = value;
			}

			UpdateSelectionState();
		}
	}

	public bool Hidden
	{
		// Parent was collapsed
		get => hidden || ParentCollapsed;
	}

	public bool Filtered
	{
		get => filtered;
		set
		{
			filtered = value;

			UpdateState();
		}
	}

	public bool Inherited
	{
		get
		{
			if (listItem != null)
			{
				return listItem.Inherited;
			}

			return false;
		}
		set
		{
			if (listItem != null)
			{
				listItem.Inherited = value;
			}

			if (lockContainer != null)
			{
				lockContainer.Visible = Inherited && (!CanContainItems);
			}
		}
	}

	public bool Overriding
	{
		get
		{
			if (listItem != null)
			{
				return Inherited && listItem.Overriding;
			}

			return false;
		}
		set
		{
			if (listItem != null)
			{
				listItem.Overriding = value;
			}

			if (lockButton != null)
			{
				lockButton.Texture = (Overriding ? lockOpen : lockClosed);
			}
		}
	}

	public bool HasSelected
	{
		get => selectedChildren.Count > 0;
	}

	public TreeList ContainingTreeList
	{
		get => containingTreeList;
		set => containingTreeList = value;
	}

	public bool CanDragSibling
	{
		get => canDragSibling;
		set => canDragSibling = value;
	}

	public List<Vector2> RelationshipPoints
	{
		get
		{
			if (updateRelationshipPoints)
			{
				updateRelationshipPoints = false;
				UpdateRelationshipPoints();
			}

			return relationshipPoints;
		}
	}

	private enum DragDropLocation
	{
		none,
		above,
		within,
		below
	}

	private DragDropLocation DragLocation
	{
		get => dragLocation;
		set
		{
			dragLocation = value;

			switch (dragLocation)
			{
				default:
				case DragDropLocation.none:
					dragPanels.Visible = false;
					dragAbovePanel.Visible = false;
					dragWithinPanel.Visible = false;
					dragBelowPanel.Visible = false;
					break;
				case DragDropLocation.above:
					dragPanels.Visible = true;
					dragAbovePanel.Visible = true;
					dragWithinPanel.Visible = false;
					dragBelowPanel.Visible = false;
					break;
				case DragDropLocation.within:
					dragPanels.Visible = true;
					dragAbovePanel.Visible = false;
					dragWithinPanel.Visible = true;
					dragBelowPanel.Visible = false;
					break;
				case DragDropLocation.below:
					dragPanels.Visible = true;
					dragAbovePanel.Visible = false;
					dragWithinPanel.Visible = false;
					dragBelowPanel.Visible = true;
					break;
			}
		}
	}

	public object MetaData
	{
		get => metaData;
		set
		{
			metaData = value;
		}
	}

	private readonly List<Vector2> relationshipPoints = new List<Vector2>();
	private bool updateRelationshipPoints = false;
	private Control dragPreview;

	private TreeList containingTreeList;
	private bool canContainItems = true;
	private bool canDragSibling = true;
	private bool collapsed = false;
	private bool filtered = false;
	private readonly bool hidden = false;
	private object metaData;
	private DragDropLocation dragLocation = DragDropLocation.none;
	private Texture icon = null;
	private Texture iconCollapsed = null;
	private int listIndex = -1;
	private TreeListItem parent = null;
	private readonly List<TreeListItem> children = new List<TreeListItem>();
	private int nestLevel = 0;
	private Texture chevronRight;
	private Texture chevronDown;
	private Texture lockClosed;
	private Texture lockOpen;

	private Control relationship;
	private MarginContainer contents;
	private MarginContainer dropArrowContainer;
	private TextureRect dropArrow;
	private MarginContainer lockContainer;
	private TextureRect lockButton;
	private ListItem listItem;
	private Panel hasSelectedPanel;
	private Control dragPanels;
	private Panel dragAbovePanel;
	private Panel dragWithinPanel;
	private Panel dragBelowPanel;

	private bool dragging = false;

	protected List<TreeListItem> selectedChildren = new List<TreeListItem>();

	public override void _Ready()
	{
		chevronRight = ResourceLoader.Load<Texture>("res://Assets/Icons/chevron-right.png");
		chevronDown = ResourceLoader.Load<Texture>("res://Assets/Icons/chevron-down.png");
		lockClosed = ResourceLoader.Load<Texture>("res://Assets/Icons/locked.png");
		lockOpen = ResourceLoader.Load<Texture>("res://Assets/Icons/unlocked.png");

		relationship = GetNode<Control>("HBoxContainer/Relationship");
		contents = GetNode<MarginContainer>("HBoxContainer/Contents");
		dropArrowContainer = GetNode<MarginContainer>("HBoxContainer/Contents/HBoxContainer/DropArrowContainer");
		dropArrow = GetNode<TextureRect>("HBoxContainer/Contents/HBoxContainer/DropArrowContainer/DropArrow");
		lockContainer = GetNode<MarginContainer>("HBoxContainer/Contents/HBoxContainer/LockContainer");
		lockButton = GetNode<TextureRect>("HBoxContainer/Contents/HBoxContainer/LockContainer/LockIcon");
		listItem = GetNode<ListItem>("HBoxContainer/Contents/HBoxContainer/ListItem");
		hasSelectedPanel = GetNode<Panel>("HBoxContainer/Contents/HBoxContainer/ListItem/HasSelectedPanel");
		dragPanels = GetNode<Control>("HBoxContainer/Contents/HBoxContainer/ListItem/DragPanels");
		dragAbovePanel = GetNode<Panel>("HBoxContainer/Contents/HBoxContainer/ListItem/DragPanels/DragAbovePanel");
		dragWithinPanel = GetNode<Panel>("HBoxContainer/Contents/HBoxContainer/ListItem/DragPanels/DragWithinPanel");
		dragBelowPanel = GetNode<Panel>("HBoxContainer/Contents/HBoxContainer/ListItem/DragPanels/DragBelowPanel");

		listItem.SetDragForwarding(this);
		KeepPressedOutside = false;

		listItem.Connect(nameof(ListItem.ButtonPressed), this, nameof(OnButtonPressed));
		listItem.Connect(nameof(ListItem.DoubleClicked), this, nameof(OnDoubleClicked));
		listItem.Connect(nameof(ListItem.Renamed), this, nameof(OnRenamed));
		listItem.Connect(nameof(ListItem.TextInputChanged), this, nameof(OnTextInputChanged));
		listItem.Connect(nameof(ListItem.Deleted), this, nameof(OnDeleted));
		listItem.Connect("mouse_exited", this, nameof(OnMouseExited));

		Collapsed = collapsed;

		UpdateState();
		UpdateGraphics();
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mbEvent)
		{
			if (mbEvent.Pressed && mbEvent.ButtonIndex == (int)ButtonList.Left)
			{
				if (MouseOnDropArrow())
				{
					Collapsed = !Collapsed;
				}

				if (MouseOnLock())
				{
					Overriding = !Overriding;
					EmitSignal(nameof(OverridingChanged), this, Overriding);
				}
			}
		}
	}

	public override void _Process(float delta)
	{
		UpdateGraphics();

		/*if (updateRelationshipPoints)
        {
            updateRelationshipPoints = false;
            UpdateRelationshipPoints();
        }*/

		listItem.Dragging = IsInstanceValid(dragPreview);

		if (!Input.IsMouseButtonPressed((int)ButtonList.Left))
		{
			dragging = false;
			DragLocation = DragDropLocation.none;
			return;
		}

		var pos = GetGlobalMousePosition();

		if (!GetGlobalRect().HasPoint(pos))
		{
			dragging = false;
			DragLocation = DragDropLocation.none;
			return;
		}

		if (dragging)
		{
			listItem.DragScroll(pos - listItem.RectGlobalPosition);
		}
	}

	public bool MouseOnLock()
	{
		if (lockButton == null)
			return false;
		if (!lockButton.IsVisibleInTree())
			return false;

		if (lockButton.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
		{
			return true;
		}

		return false;
	}

	public bool MouseOnDropArrow()
	{
		if (dropArrow == null)
			return false;
		if (!dropArrow.IsVisibleInTree())
			return false;

		var rect = dropArrow.GetGlobalRect();
		var center = rect.Position + rect.Size * 0.5f;
		var radius = 14.0f;

		var mPos = GetGlobalMousePosition();

		if (mPos.DistanceTo(center) <= radius)
		{
			return true;
		}

		return false;
	}

	public void UpdateGraphics()
	{
		Inherited = Inherited;
		Overriding = Overriding;

		if (lockButton != null)
		{
			lockButton.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.6f);

			if (MouseOnLock())
			{
				lockButton.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			}
		}

		if (dropArrow != null)
		{
			dropArrow.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.6f);

			if (MouseOnDropArrow())
			{
				dropArrow.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			}
		}
	}

	public void UpdateRelationshipPoints()
	{
		relationshipPoints.Clear();

		if (children.Count > 0 && !collapsed)
		{
			var rect = GetGlobalRect();
			var arrowRect = dropArrow.GetGlobalRect();
			arrowRect.Position -= rect.Position;

			var startPos = new Vector2(arrowRect.Position.x + arrowRect.Size.x * 0.5f, arrowRect.Position.y + arrowRect.Size.y);
			var endPos = startPos;

			foreach (var child in children)
			{
				if (!child.Visible)
					continue;
				if (!child.IsInsideTree())
				{
					updateRelationshipPoints = true;
					continue;
				}

				var childRect = child.dropArrowContainer.GetGlobalRect();

				if (child.children.Count <= 0)
				{
					childRect = child.listItem.GetGlobalRect();
				}

				childRect.Position -= rect.Position;

				var leftX = startPos.x;
				var rightX = childRect.Position.x;
				var y = childRect.Position.y + childRect.Size.y * 0.5f;

				endPos = new Vector2(leftX, y);
				relationshipPoints.Add(endPos);
				relationshipPoints.Add(new Vector2(rightX, y));
			}

			if (startPos != endPos)
			{
				endPos.y++;

				relationshipPoints.Add(startPos);
				relationshipPoints.Add(endPos);
			}
		}

		containingTreeList?.Refresh();
	}

	private Control MakeDragPreview()
	{
		var preview = new Button
		{
			Text = Text,
			Icon = Icon,
			Modulate = new Color(1.0f, 1.0f, 1.0f, 0.65f)
		};

		preview.AddConstantOverride("hseparation", 6);
		preview.AddStyleboxOverride("normal", new StyleBoxEmpty());

		dragPreview = preview;

		return preview;
	}

#pragma warning disable IDE1006 // Naming Styles
	public object get_drag_data_fw(Vector2 position, Control source)
#pragma warning restore IDE1006 // Naming Styles
	{
		if (Inherited)
			return null;

		SetDragPreview(MakeDragPreview());

		return this;
	}

	public override bool CanDropData(Vector2 position, object data)
	{
		return false;
	}

#pragma warning disable IDE1006 // Naming Styles
	public bool can_drop_data_fw(Vector2 position, object data, Control source)
#pragma warning restore IDE1006 // Naming Styles
	{
		dragging = false;
		if (Inherited)
			return false;

		DragLocation = DragDropLocation.none;
		TreeListItem treeListItem = null;

		if (data is TreeListItem tli)
		{
			treeListItem = tli;
		}

		if (treeListItem != null)
		{
			dragging = true;
			if (treeListItem == this)
				return false;
			if (treeListItem.ContainsChildItem(this))
				return false;

			// Make sure position is legal
			if (treeListItem.ContainingTreeList == containingTreeList)
			{
				position /= RectSize;

				if (canContainItems)
				{
					if (canDragSibling)
					{
						var border = 0.3f;

						if (position.y <= border)
						{
							DragLocation = DragDropLocation.above;
						}
						else if (position.y <= (1.0f - border))
						{
							DragLocation = DragDropLocation.within;
						}
						else
						{
							DragLocation = DragDropLocation.below;
						}
					}
					else
						DragLocation = DragDropLocation.within;
				}
				else if (canDragSibling)
				{
					if (position.y <= 0.5f)
					{
						DragLocation = DragDropLocation.above;
					}
					else
					{
						DragLocation = DragDropLocation.below;
					}
				}
				else
				{
					return false;
				}

				return true;
			}
		}

		return false;
	}

#pragma warning disable IDE1006 // Naming Styles
	public void drop_data_fw(Vector2 position, object data, Control source)
#pragma warning restore IDE1006 // Naming Styles
	{
		if (containingTreeList == null)
			return;

		TreeListItem treeListItem = null;

		if (data is TreeListItem tli)
		{
			treeListItem = tli;
		}

		if (treeListItem != null)
		{
			if (treeListItem == this)
				return;

			switch (DragLocation)
			{
				default:
				case DragDropLocation.none:
					return;
				case DragDropLocation.within:
					if (CanContainItems)
					{
						containingTreeList.ReparentListItem(treeListItem, this);
					}
					break;
				case DragDropLocation.above:
					if (CanDragSibling)
					{
						containingTreeList.MoveListItem(treeListItem, this, true);
					}
					break;
				case DragDropLocation.below:
					if (CanDragSibling)
					{
						containingTreeList.MoveListItem(treeListItem, this, false);
					}
					break;
			}
		}

		DragLocation = DragDropLocation.none;
	}

	public override string ToString()
	{
		return Text;
	}

	public void OnMouseExited()
	{
		DragLocation = DragDropLocation.none;
	}

	public void OnButtonPressed()
	{
		EmitSignal(nameof(ButtonPressed), this);
	}

	public void OnDoubleClicked(int listIndex)
	{
		EmitSignal(nameof(DoubleClicked), this);
	}

	public void OnRenamed(int listIndex, string newName)
	{
		EmitSignal(nameof(Renamed), this, newName);
	}

	public void OnTextInputChanged(int listIndex, string newName)
	{
		EmitSignal(nameof(TextInputChanged), this, newName);
	}

	public void OnDeleted(int listIndex)
	{
		EmitSignal(nameof(Deleted), this);
	}

	public void Edit()
	{
		if (!CanBeRenamed)
			return;

		listItem?.Edit();
	}

	public void Select()
	{
		if (!Selected)
		{
			listItem?.Select();

			UpdateSelectionState();
			UpdateParentsState();
		}
	}

	public void Deselect()
	{
		if (Selected)
		{
			listItem?.Deselect();

			UpdateSelectionState();
			UpdateParentsState();
		}
	}

	public bool ContainsChildItem(TreeListItem listItem)
	{
		if (listItem == null)
			return false;

		// Returns whether listItem is a child of this or any of its children (all the way down)
		foreach (var child in children)
		{
			if (child == listItem)
			{
				return true;
			}

			if (child.ContainsChildItem(listItem))
			{
				return true;
			}
		}

		return false;
	}

	public void AddChildItem(TreeListItem listItem)
	{
		listItem.parent = this;
		children.Add(listItem);
		listItem.UpdateSelectionState();
		UpdateState();
	}

	public void InsertChildItem(int idx, TreeListItem listItem)
	{
		// Inserts the child at index idx
		listItem.parent = this;
		children.Insert(idx, listItem);
		listItem.UpdateSelectionState();
		UpdateState();
	}

	public void RemoveChildItem(TreeListItem listItem)
	{
		if (children.Remove(listItem))
		{
			RemoveSelected(listItem);
			UpdateState();
		}
	}

	public void SetParent(TreeListItem parent, int idx = -1)
	{
		if (ContainsChildItem(parent))
			return;

		this.parent?.RemoveChildItem(this);

		if (parent != null)
		{
			if (idx < 0)
			{
				parent.AddChildItem(this);
			}
			else
			{
				parent.InsertChildItem(idx, this);
			}

			NestLevel = parent.NestLevel + 1;
		}
		else
		{
			NestLevel = 0;
		}
	}

	public List<TreeListItem> GetAllChildren()
	{
		var _children = new List<TreeListItem>();

		foreach (var child in children)
		{
			_children.Add(child);

			_children.AddRange(child.GetAllChildren());
		}

		return _children;
	}

	public void UpdateState(bool updateChildren = true, bool updateParents = true)
	{
		if (!Godot.Object.IsInstanceValid(listItem))
			return;

		if (dropArrow != null)
		{
			dropArrow.Texture = (collapsed ? chevronRight : chevronDown);
			dropArrow.Visible = (children.Count > 0);
		}

		if (dropArrowContainer != null)
		{
			dropArrowContainer.Visible = parent != null || canContainItems;
		}

		if (listItem != null)
		{
			listItem.Icon = (collapsed ? (iconCollapsed ?? icon) : icon);

			if (canContainItems && ChildCount == 0)
			{
				listItem.Icon = iconCollapsed;
			}
		}

		NestLevel = nestLevel;

		if (updateChildren)
		{
			foreach (var child in children)
			{
				child.UpdateState(true, false);
			}
		}

		Visible = !(filtered || Hidden);

		// If has selected, selected not visible, and there are no visible children with selected, show selected panel
		if (Visible && hasSelectedPanel != null)
		{
			var selectedVisible = true;

			foreach (var selectedChild in selectedChildren)
			{
				if (!selectedChild.Visible)
				{
					selectedVisible = false;
					break;
				}
			}

			hasSelectedPanel.Visible = false;
			if (!selectedVisible && HasSelected)
			{
				var childHasSelected = false;
				foreach (var child in children)
				{
					if (child.Visible && child.HasSelected)
					{
						childHasSelected = true;
						break;
					}
				}

				if (!childHasSelected)
				{
					hasSelectedPanel.Visible = true;
				}
			}
		}

		updateRelationshipPoints = true;
		//UpdateRelationshipPoints();

		if (updateParents)
		{
			UpdateParentsState();
		}
	}

	public void AddSelected(TreeListItem item)
	{
		if (!selectedChildren.Contains(item))
		{
			selectedChildren.Add(item);
		}

		if (parent != null)
		{
			parent.AddSelected(item);
		}
	}

	public void RemoveSelected(TreeListItem item)
	{
		selectedChildren.Remove(item);

		if (parent != null)
		{
			parent.RemoveSelected(item);
		}
	}

	private void UpdateParentsState()
	{
		if (parent != null)
		{
			parent.UpdateState(false);
		}
	}

	private void UpdateSelectionState()
	{
		if (parent != null)
		{
			if (Selected)
			{
				parent.AddSelected(this);
			}
			else
			{
				parent.RemoveSelected(this);
			}
		}
	}
}
