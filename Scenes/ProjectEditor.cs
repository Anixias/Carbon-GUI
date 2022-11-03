using Glint;
using Glint.Collections;
using Godot;
using System.Collections.Generic;

public class ProjectEditor : HSplitContainer
{
	[Signal]
	public delegate void UpdateProjectStatus(bool hasProject);

	[Signal]
	public delegate void UpdateUndo(bool hasUndo);

	[Signal]
	public delegate void UpdateRedo(bool hasRedo);

	private Project currentProject;
	private Collection currentCollection;
	private Object currentObject;
	private Field currentField;
	private readonly History<EditorCommand> commands = new History<EditorCommand>();

	private int lastSavedHashCode;

	public bool HasUnsavedChanges
	{
		get
		{
			if (commands.GetHashCode() != lastSavedHashCode)
			{
				return true;
			}

			// @TODO: Add "pending inspector changes"
			if (currentObject != null)
			{
				foreach (var field in currentObject.fields)
				{
					if (field.GetEditor()?.HasChanges() ?? false)
					{
						return true;
					}
				}

				foreach (var field in currentObject.fieldOverrides.Values)
				{
					if (field.GetEditor()?.HasChanges() ?? false)
					{
						return true;
					}
				}
			}

			return false;
		}
	}

	private List<Collection> Collections
	{
		get
		{
			if (HasProject())
			{
				return currentProject.collections;
			}

			return null;
		}
		set
		{
			if (HasProject())
			{
				currentProject.collections = value;
			}
		}
	}

	private List<Object> Objects
	{
		get
		{
			if (currentCollection != null)
			{
				return currentCollection.objects;
			}

			return null;
		}

		set
		{
			if (currentCollection != null)
			{
				currentCollection.objects = value;
			}
		}
	}

	private List<Field> Fields
	{
		get
		{
			if (currentObject != null)
			{
				return currentObject.fields;
			}

			return null;
		}

		set
		{
			if (currentObject != null)
			{
				currentObject.fields = value;
			}
		}
	}

	private PackedScene treeListItem;
	private PackedScene iconButton;
	private Texture collectionIcon;
	private Texture typeIcon;
	private Texture typeOpenIcon;
	private Texture objectIcon;
	private Texture toolStringIcon;
	private Texture toolTextIcon;
	private Texture toolNumberIcon;
	private Texture toolBooleanIcon;
	private Texture toolImageIcon;

	private TreeList collectionList;
	private TextInputBox collectionFilter;
	private IconButton objAddType;
	private IconButton objAddObject;
	private TreeList objectList;
	private TextInputBox objectFilter;
	private PanelContainer toolBar;
	private HFlowContainer toolList;
	private VSplitContainer dataWindows;
	private ScrollContainer workArea;
	private VBoxContainer fieldList;
	private MarginContainer fieldMarginContainer;
	private TreeList designList;
	private TextInputBox designFilter;
	private Dictionary<Object, TreeListItem> designListCategories;
	private List<Field> designFieldList;
	private Panel fieldInspectorPanel;
	private Dictionary<Object, Label> fieldObjectLabels;

	private List<IconButton> tools;

	public override void _Ready()
	{
		currentProject = null;
		currentCollection = null;
		currentObject = null;

		treeListItem = ResourceLoader.Load<PackedScene>("res://Scenes/UI Elements/TreeListItem.tscn");
		iconButton = ResourceLoader.Load<PackedScene>("res://Scenes/UI Elements/IconButton.tscn");
		collectionIcon = ResourceLoader.Load<Texture>("res://Assets/Icons/keyframes.png");
		typeIcon = ResourceLoader.Load<Texture>("res://Assets/Icons/folder-closed.png");
		typeOpenIcon = ResourceLoader.Load<Texture>("res://Assets/Icons/folder-open.png");
		objectIcon = ResourceLoader.Load<Texture>("res://Assets/Icons/object.png");
		toolStringIcon = ResourceLoader.Load<Texture>("res://Assets/Icons/text-style.png");
		toolTextIcon = ResourceLoader.Load<Texture>("res://Assets/Icons/paper-note.png");
		toolNumberIcon = ResourceLoader.Load<Texture>("res://Assets/Icons/number-nine.png");
		toolBooleanIcon = ResourceLoader.Load<Texture>("res://Assets/Icons/toggle-left.png");
		toolImageIcon = ResourceLoader.Load<Texture>("res://Assets/Icons/photo-image.png");

		collectionList = GetNode<TreeList>("VSplitContainer/CollectionPanel/MarginContainer/VBoxContainer/Panel/TreeList");
		collectionFilter = GetNode<TextInputBox>("VSplitContainer/CollectionPanel/MarginContainer/VBoxContainer/Filter");
		objAddType = GetNode<IconButton>("VSplitContainer/ObjectPanel/MarginContainer/VBoxContainer/HBoxContainer/AddType");
		objAddObject = GetNode<IconButton>("VSplitContainer/ObjectPanel/MarginContainer/VBoxContainer/HBoxContainer/AddObject");
		objectList = GetNode<TreeList>("VSplitContainer/ObjectPanel/MarginContainer/VBoxContainer/Panel/TreeList");
		objectFilter = GetNode<TextInputBox>("VSplitContainer/ObjectPanel/MarginContainer/VBoxContainer/Filter");
		toolBar = GetNode<PanelContainer>("HSplitContainer/VBoxContainer/ToolBar");
		toolList = GetNode<HFlowContainer>("HSplitContainer/VBoxContainer/ToolBar/MarginContainer/ToolList");
		workArea = GetNode<ScrollContainer>("HSplitContainer/VBoxContainer/WorkArea");
		fieldMarginContainer = GetNode<MarginContainer>("HSplitContainer/VBoxContainer/WorkArea/MarginContainer");
		fieldList = GetNode<VBoxContainer>("HSplitContainer/VBoxContainer/WorkArea/MarginContainer/FieldList");
		dataWindows = GetNode<VSplitContainer>("HSplitContainer/DataWindows");
		designList = GetNode<TreeList>("HSplitContainer/DataWindows/DesignPanel/MarginContainer/VBoxContainer/Panel/TreeList");
		designFilter = GetNode<TextInputBox>("HSplitContainer/DataWindows/DesignPanel/MarginContainer/VBoxContainer/Filter");
		fieldInspectorPanel = GetNode<Panel>("%FieldInspectorPanel");

		workArea.GetVScrollbar().Connect("visibility_changed", this, nameof(OnWorkAreaVScrollbarVisibilityChanged));

		collectionList.Connect(nameof(TreeList.ListItemDoubleClicked), this, nameof(OnCollectionListItemDoubleClicked));
		collectionList.Connect(nameof(TreeList.ListItemRenamed), this, nameof(OnCollectionListItemRenamed));
		collectionList.Connect(nameof(TreeList.ListItemDeleted), this, nameof(OnCollectionListItemDeleted));
		collectionList.Connect(nameof(TreeList.ListItemMoved), this, nameof(OnCollectionListItemMoved));

		objectList.Connect(nameof(TreeList.ListItemDeleted), this, nameof(OnObjectListItemDeleted));
		objectList.Connect(nameof(TreeList.ListItemRenamed), this, nameof(OnObjectListItemRenamed));
		objectList.Connect(nameof(TreeList.ListItemPressed), this, nameof(OnObjectListItemPressed));
		objectList.Connect(nameof(TreeList.ListItemDoubleClicked), this, nameof(OnObjectListItemDoubleClicked));
		objectList.Connect(nameof(TreeList.ListItemCollapseChanged), this, nameof(OnObjectListItemCollapseChanged));
		objectList.Connect(nameof(TreeList.ListItemMoved), this, nameof(OnObjectListItemMoved));
		objectList.Connect(nameof(TreeList.NoItemPressed), this, nameof(OnObjectListNoItemPressed));

		designList.Connect(nameof(TreeList.ListItemDeleted), this, nameof(OnDesignListItemDeleted));
		designList.Connect(nameof(TreeList.ListItemRenamed), this, nameof(OnDesignListItemRenamed));
		designList.Connect(nameof(TreeList.ListItemPressed), this, nameof(OnDesignListItemPressed));
		designList.Connect(nameof(TreeList.ListItemDoubleClicked), this, nameof(OnDesignListItemDoubleClicked));
		designList.Connect(nameof(TreeList.ListItemMoved), this, nameof(OnDesignListItemMoved));
		designList.Connect(nameof(TreeList.ListItemOverridingChanged), this, nameof(OnDesignListItemOverridingChanged));
		designList.Connect(nameof(TreeList.NoItemPressed), this, nameof(OnDesignListNoItemPressed));

		// Add tools to toolbar
		tools = new List<IconButton>();
		CreateToolButton(toolStringIcon, "Add string field", nameof(OnToolStringPressed));
		CreateToolButton(toolTextIcon, "Add text field", nameof(OnToolTextPressed));
		CreateToolButton(toolNumberIcon, "Add numeric field", nameof(OnToolNumberPressed));
		CreateToolButton(toolBooleanIcon, "Add boolean field", nameof(OnToolBooleanPressed));
		CreateToolButton(toolImageIcon, "Add image field", nameof(OnToolImagePressed));

		designListCategories = new Dictionary<Object, TreeListItem>();
		designFieldList = new List<Field>();
		fieldObjectLabels = new Dictionary<Object, Label>();
	}

	public void ProjectSaved()
	{
		lastSavedHashCode = commands.GetHashCode();
	}

	public void Undo()
	{
		if (commands.HasUndo())
		{
			var cmd = commands.Undo();

			GD.Print("Undo '" + cmd + "'");
			cmd.Undo();
		}

		UpdateHistory();
	}

	public void Redo()
	{
		if (commands.HasRedo())
		{
			var cmd = commands.Redo();

			GD.Print("Redo '" + cmd + "'");
			cmd.Execute();
		}

		UpdateHistory();
	}

	private void UpdateHistory()
	{
		EmitSignal(nameof(UpdateUndo), commands.HasUndo());
		EmitSignal(nameof(UpdateRedo), commands.HasRedo());
	}

	public void OnWorkAreaVScrollbarVisibilityChanged()
	{
		if (workArea.GetVScrollbar().Visible)
		{
			fieldMarginContainer.AddConstantOverride("margin_right", 7);
		}
		else
		{
			fieldMarginContainer.AddConstantOverride("margin_right", 0);
		}
	}

	public void SetProject(Project project)
	{
		if (project == currentProject)
			return;

		UnloadCurrentObject();
		UnloadCurrentCollection();
		Collections?.Clear();
		commands.Clear();

		currentProject = project;

		LoadCollections();
		ProjectSaved();

		EmitSignal(nameof(UpdateProjectStatus), HasProject());
	}

	public bool HasProject()
	{
		return (currentProject != null);
	}

	private void PushCommand(EditorCommand command, bool execute = true)
	{
		var valid = true;

		if (command is ITestable testable)
		{
			// Only push command if it passes its defined test
			valid = testable.Test();
		}

		if (valid)
		{
			commands.Push(command);
			if (execute)
				command.Execute();

			UpdateHistory();
			GD.Print(command);
		}
	}

	private void CreateToolButton(Texture icon, string tooltip = "", string callback = null)
	{
		var toolBtn = iconButton.Instance<IconButton>();
		toolBtn.Text = "";
		toolBtn.Icon = icon;
		toolBtn.KeepPressedOutside = false;
		toolBtn.TintIcon = true;
		toolBtn.HintTooltip = tooltip;

		toolList.AddChild(toolBtn);
		tools.Add(toolBtn);

		if (callback != null)
		{
			toolBtn.Connect(nameof(IconButton.Pressed), this, callback);
		}
	}

	private void SetToolsEnabled(bool enabled)
	{
		foreach (var tool in tools)
		{
			tool.Enabled = enabled;
		}
	}

	private void OnFieldDataEdited(Field field, object previousData)
	{
		PushCommand(new EditFieldCommand(this, null, field, previousData, field.Data), false);
	}

	public void CreateField(Object obj, Texture icon, Field field)
	{
		if (obj == null)
			return;

		EnsureUnique(field, obj);
		obj.fields.Add(field);

		if (obj == currentObject)
		{
			TreeListItem parent = null;

			if (designListCategories.ContainsKey(obj))
			{
				parent = designListCategories[obj];
			}

			var listItem = designList.AddListItem(parent, field.Name, icon, null, false);
			designFieldList.Add(field);
			listItem.MetaData = field;
			field.ListItem = listItem;

			var editor = field.CreateEditor(false);
			if (editor != null)
			{
				fieldList.AddChild(editor);
			}
		}
		else if (currentObject != null && obj.IsAncestorOf(currentObject))
		{
			LoadObject(currentObject);
		}

		RefreshUI();
	}

	public void RestoreField(Object obj, Texture icon, Field field, int localIndex)
	{
		if (obj == null)
			return;

		EnsureUnique(field, obj);

		var indexDiff = 0;
		if (localIndex >= 0 && localIndex <= obj.fields.Count)
		{
			indexDiff = obj.fields.Count - localIndex;
			obj.fields.Insert(localIndex, field);
		}
		else
		{
			obj.fields.Add(field);
		}

		if (obj == currentObject)
		{
			TreeListItem parent = null;

			if (designListCategories.ContainsKey(obj))
			{
				parent = designListCategories[obj];
			}

			var listItem = designList.AddListItem(parent, field.Name, icon, null, false);
			designList.MoveListItem(listItem, localIndex, false);
			designFieldList.Add(field);
			listItem.MetaData = field;
			field.ListItem = listItem;

			var editor = field.CreateEditor(false);
			if (editor != null)
			{
				fieldList.AddChild(editor);

				if (indexDiff > 0)
				{
					fieldList.MoveChild(editor, editor.GetIndex() - indexDiff);
				}
			}
		}
		else if (currentObject != null && obj.IsAncestorOf(currentObject))
		{
			LoadObject(currentObject);
		}

		RefreshUI();
	}

	public void DeleteField(Object obj, Field field, bool removeFromTree = true)
	{
		if (obj == null)
			return;
		if (field == null)
			return;

		obj.fields.Remove(field);

		if (obj == currentObject)
		{
			if (removeFromTree)
			{
				designList.RemoveListItem(field.ListItem);
			}

			field.ListItem = null;
			designFieldList.Remove(field);
			field.RemoveEditor();
		}
		else if (currentObject != null && obj.IsAncestorOf(currentObject))
		{
			LoadObject(currentObject);
		}

		RefreshUI();
	}

	public void RenameField(Object obj, Field field, string newName)
	{
		field.Name = newName;
		EnsureUnique(field, obj);

		RefreshUI();
	}

	public void MoveField(Object obj, Field field, int localIndex)
	{
		if (obj == null)
			return;
		if (field == null)
			return;

		obj.fields.Remove(field);
		obj.fields.Insert(localIndex, field);

		if (obj == currentObject)
		{
			designList.MoveListItem(field.ListItem, localIndex, false);
		}

		if (currentObject != null)
		{
			LoadObject(currentObject);
		}
	}

	public void OverrideField(Object obj, Field field, Field fieldOverride = null)
	{
		var ancestorField = obj.GetFieldAncestor(field);
		fieldOverride ??= ancestorField.Duplicate();

		if (!obj.fieldOverrides.ContainsKey(field))
		{
			obj.fieldOverrides.Add(field, fieldOverride);
			field.AddLink(fieldOverride);
		}

		if (ancestorField.HasEditor())
		{
			ReplaceNode(ancestorField.GetEditor(), fieldOverride.CreateEditor(true));
		}

		fieldOverride.SetOverriding(true);

		if (obj == currentObject)
		{
			if (designFieldList.Contains(field))
			{
				field.ListItem.Overriding = true;
			}
		}
	}

	public void RemoveFieldOverride(Object obj, Field field)
	{
		var ancestorField = obj.GetFieldAncestor(field);

		if (obj.fieldOverrides.ContainsKey(field))
		{
			var overriddenField = obj.fieldOverrides[field];

			if (overriddenField.HasEditor())
			{
				ReplaceNode(overriddenField.GetEditor(), ancestorField.CreateEditor(true));
				overriddenField.RemoveEditor();
			}

			field.RemoveLink(overriddenField);
			obj.fieldOverrides.Remove(field);

			if (obj == currentObject)
			{
				if (designFieldList.Contains(field))
				{
					field.ListItem.Overriding = false;
				}
			}
		}
	}

	public void OnToolStringPressed()
	{
		var field = new StringField("String", "", OnFieldDataEdited);
		PushCommand(new CreateFieldCommand(this, currentObject, toolStringIcon, field));
		field.ListItem?.Edit();
		SelectField(field);
		designFilter.Text = "";
	}

	public void OnToolTextPressed()
	{
		var field = new TextField("Text", "", OnFieldDataEdited);
		PushCommand(new CreateFieldCommand(this, currentObject, toolTextIcon, field));
		field.ListItem?.Edit();
		SelectField(field);
		designFilter.Text = "";
	}

	public void OnToolNumberPressed()
	{
		var field = new NumberField("Number", 0, OnFieldDataEdited);
		PushCommand(new CreateFieldCommand(this, currentObject, toolNumberIcon, field));
		field.ListItem?.Edit();
		SelectField(field);
		designFilter.Text = "";
	}

	public void OnToolBooleanPressed()
	{
		var field = new BooleanField("Boolean", false, OnFieldDataEdited);
		PushCommand(new CreateFieldCommand(this, currentObject, toolBooleanIcon, field));
		field.ListItem?.Edit();
		SelectField(field);
		designFilter.Text = "";
	}

	public void OnToolImagePressed()
	{
		var field = new ImageField("Image", null, null, OnFieldDataEdited);
		PushCommand(new CreateFieldCommand(this, currentObject, toolImageIcon, field));
		field.ListItem?.Edit();
		SelectField(field);
		designFilter.Text = "";
	}

	public void OnDesignListItemDeleted(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex)
	{
		if (!HasProject())
			return;
		if (currentObject == null)
			return;

		if (listItem.MetaData is Field field)
		{
			PushCommand(new DeleteFieldCommand(this, currentObject, listItem.Icon, field, oldLocalIndex), false);
			DeleteField(currentObject, field, false);
		}
	}

	public void OnDesignListItemRenamed(TreeListItem listItem, string newName)
	{
		if (!HasProject())
			return;
		if (currentObject == null)
			return;

		if (listItem.MetaData is Field field)
		{
			if (field.Name != newName)
			{
				PushCommand(new RenameFieldCommand(this, currentObject, field, field.Name, newName));
			}
		}
		else if (listItem.MetaData is Object obj)
		{
			if (obj.Name != newName)
			{
				PushCommand(new RenameObjectCommand(this, currentCollection, obj, obj.Name, newName));
			}
		}
	}

	public void OnDesignListItemPressed(TreeListItem listItem)
	{
		if (!HasProject())
			return;
		if (currentObject == null)
			return;

		// Select the field, and cause the inspector to appear if it is the originating object
		if (listItem.MetaData is Field nextField)
		{
			SelectField(nextField);
		}
	}

	public void OnDesignListNoItemPressed()
	{
		if (currentField != null)
		{
			currentField.ListItem?.Deselect();
			currentField.RemoveInspector();
			currentField = null;
		}
	}

	public void OnDesignListItemDoubleClicked(TreeListItem listItem)
	{
		if (!HasProject())
			return;
		if (currentObject == null)
			return;

		listItem.Edit();
	}

	public void OnDesignListItemMoved(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex, int newLocalIndex)
	{
		if (!HasProject())
			return;
		if (currentObject == null)
			return;

		if (listItem.MetaData is Field field)
		{
			PushCommand(new MoveFieldCommand(this, currentObject, field, oldLocalIndex, newLocalIndex));
		}
	}

	public void OnDesignListItemOverridingChanged(TreeListItem listItem, bool overriding)
	{
		if (!HasProject())
			return;
		if (currentObject == null)
			return;

		if (listItem.MetaData is Field field)
		{
			PushCommand(new OverrideFieldCommand(this, currentObject, field, overriding));
		}
	}

	public void OnDesignFilterTextChanged(string newText)
	{
		FilterFields();
	}

	private void SelectField(Field nextField)
	{
		var prevField = currentField;

		if (nextField != prevField)
		{
			currentField = nextField;

			if (prevField != null)
			{
				prevField.ListItem?.Deselect();
				prevField.RemoveInspector();
			}

			if (currentField != null)
			{
				currentField.ListItem?.Select();
				var inspector = currentField.CreateInspector(currentField.GetEditor()?.Inherited ?? false);

				if (inspector != null)
				{
					fieldInspectorPanel.AddChild(inspector);
				}
			}
		}
	}

	private void ReplaceNode(Node current, Node target)
	{
		if (current == null || !Godot.Object.IsInstanceValid(current))
			return;
		if (target == null || !Godot.Object.IsInstanceValid(target))
			return;

		var parent = current.GetParent();
		if (parent == null)
			return;

		var pos = current.GetPositionInParent();
		parent.RemoveChild(current);
		parent.AddChild(target);
		parent.MoveChild(target, pos);
	}

	public void AddCollection(Collection collection, bool addToCollections = true)
	{
		var btn = collectionList.AddListItem(null, collection.Name, collectionIcon, null, false);
		btn.MetaData = collection;
		collection.ListItem = btn;

		if (addToCollections)
		{
			Collections.Insert(btn.ListIndex, collection);
		}

		EnsureUnique(collection);

		RefreshUI();

		if (collectionFilter.Text != "")
		{
			FilterCollection(collection, collectionFilter.Text);
		}
	}

	public void RestoreCollection(Collection collection, int listIndex)
	{
		var btn = collectionList.AddListItem(null, collection.Name, collectionIcon, null, false);
		btn.MetaData = collection;
		collection.ListItem = btn;
		collectionList.MoveListItem(btn, listIndex, false);
		Collections.Insert(btn.ListIndex, collection);
		EnsureUnique(collection);

		RefreshUI();

		if (collectionFilter.Text != "")
		{
			FilterCollection(collection, collectionFilter.Text);
		}
	}

	public void RemoveCollection(Collection collection, bool removeFromTree = true)
	{
		if (currentCollection == collection)
		{
			UnloadCurrentCollection();
		}

		if (removeFromTree)
		{
			collectionList.RemoveListItem(collection.ListItem);
		}

		collection.ListItem = null;
		Collections.Remove(collection);

		RefreshUI();
	}

	public void RenameCollection(Collection collection, string newName)
	{
		collection.Name = newName;
		EnsureUnique(collection);

		FilterCollection(collection, collectionFilter.Text);
	}

	public void MoveCollection(Collection collection, int destIndex)
	{
		var oldListIndex = collection.ListItem.ListIndex;
		collectionList.MoveListItem(collection.ListItem, destIndex, false);

		var newListIndex = collection.ListItem.ListIndex;

		Collections.RemoveAt(oldListIndex);
		Collections.Insert(newListIndex, collection);
	}

	public void OnAddCollectionPressed()
	{
		if (!HasProject())
			return;

		var collection = new Collection();
		PushCommand(new AddCollectionCommand(this, collection));

		collectionFilter.Text = "";
		collection.ListItem?.Edit();
	}

	public void OnCollectionListItemDeleted(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex)
	{
		if (!HasProject())
			return;

		var collection = listItem.MetaData as Collection;

		PushCommand(new DeleteCollectionCommand(this, collection, oldLocalIndex), false);
		RemoveCollection(collection, false);
	}

	public void OnCollectionListItemMoved(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex, int newLocalIndex)
	{
		if (listItem.MetaData is Collection collection)
		{
			Collections.Remove(collection);
			Collections.Insert(newLocalIndex, collection);

			PushCommand(new MoveCollectionCommand(this, collection, oldLocalIndex, newLocalIndex), false);
		}
	}

	public void OnCollectionListItemRenamed(TreeListItem listItem, string newName)
	{
		if (!HasProject())
			return;

		if (listItem.MetaData is Collection collection)
		{
			if (collection.Name != newName)
			{
				PushCommand(new RenameCollectionCommand(this, collection, collection.Name, newName));
			}
		}
	}

	public void OnCollectionListItemDoubleClicked(TreeListItem listItem)
	{
		if (!HasProject())
			return;

		if (listItem.MetaData is Collection collection)
		{
			if (collection != currentCollection)
			{
				LoadCollection(collection);
			}
		}
	}

	public void OnCollectionsFilterTextChanged(string newText)
	{
		FilterCollections(newText);
	}

	public void AddObject(Collection collection, Object parent, Object @object)
	{
		if (!HasProject())
			return;

		@object.ListItem = null;
		@object.Parent = parent;

		if (currentCollection == collection)
		{
			var parentItem = parent.ListItem;

			var btn = @object.IsType ? objectList.AddListItem(parentItem, @object.Name, typeOpenIcon, typeIcon, true)
									 : objectList.AddListItem(parentItem, @object.Name, objectIcon, null, false);
			parentItem.Collapsed = false;
			@object.ListItem = btn;
			objectList.UpdateGraphics();
		}

		EnsureUnique(@object);

		if (collection != null)
		{
			var idx = -1;

			if (parent != null)
			{
				idx = collection.objects.FindIndex(match => match == parent) + parent.ChildCount;
			}

			if (idx >= 0 && idx < collection.objects.Count)
			{
				collection.objects.Insert(idx, @object);
			}
			else
			{
				collection.objects.Add(@object);
			}
		}

		if (currentCollection == collection)
		{
			if (objectFilter.Text != "")
			{
				FilterObject(@object, objectFilter.Text);
			}
		}

		RefreshUI();
	}

	public void RestoreObject(Collection collection, Object parent, int localIndex, Object @object)
	{
		if (!HasProject())
			return;
		if (@object == null)
			return;

		parent ??= collection.Root;
		@object.ListItem = null;

		if (currentCollection == collection)
		{
			var parentItem = parent?.ListItem;

			var btn = @object.IsType ? objectList.AddListItem(parentItem, @object.Name, typeOpenIcon, typeIcon, true)
									 : objectList.AddListItem(parentItem, @object.Name, objectIcon, null, false);
			parentItem.Collapsed = false;
			@object.ListItem = btn;

			objectList.MoveListItem(btn, localIndex, false);

			foreach (var child in @object.Children)
			{
				parentItem = child.Parent?.ListItem;

				btn = child.IsType ? objectList.AddListItem(parentItem, child.Name, typeOpenIcon, typeIcon, true)
								   : objectList.AddListItem(parentItem, child.Name, objectIcon, null, false);
				parentItem.Collapsed = false;
				child.ListItem = btn;
			}

			objectList.UpdateGraphics();
		}

		EnsureUnique(@object);

		if (collection != null)
		{
			var idx = -1;
			var objIndex = -1;

			if (parent != null)
			{
				idx = collection.objects.FindIndex(match => match == parent);

				if (idx >= 0)
				{
					// Loop through the parent object's direct children until the desired local index is reached
					objIndex = idx + 1;

					var siblings = parent.ChildrenDirect;
					var siblingCount = siblings.Count;
					for (var i = 0; i < siblingCount && i < localIndex; i++)
					{
						objIndex += 1 + siblings[i].ChildCount;
					}
				}
			}

			@object.SetParent(parent, localIndex);

			if (objIndex >= 0)
			{
				var objList = new List<Object> { @object };
				objList.AddRange(@object.Children);
				collection.objects.InsertRange(objIndex, objList);
			}
			else
			{
				var objList = new List<Object> { @object };
				objList.AddRange(@object.Children);
				collection.objects.AddRange(objList);
			}
		}

		if (currentCollection == collection)
		{
			if (objectFilter.Text != "")
			{
				FilterObject(@object, objectFilter.Text);
			}
		}

		RefreshUI();
	}

	public void RemoveObject(Collection collection, Object @object, bool removeFromTree = true)
	{
		if (@object == null)
			return;

		if (@object == currentObject || @object.Children.Contains(currentObject))
		{
			UnloadCurrentObject();
		}

		if (removeFromTree && currentCollection == collection)
		{
			objectList.RemoveListItem(@object.ListItem);
			objectList.UpdateGraphics();
		}

		foreach (var child in @object.Children)
		{
			child.ListItem = null;
		}

		@object.ListItem = null;
		@object.Parent = null;

		if (collection != null)
		{
			var idx = collection.objects.FindIndex(match => match == @object);

			if (idx >= 0)
			{
				collection.objects.RemoveRange(idx, 1 + @object.ChildCount);
			}
		}

		if (objectFilter.Text != "")
		{
			FilterObjects();
		}

		if (fieldObjectLabels.ContainsKey(@object))
		{
			fieldObjectLabels[@object].QueueFree();
			fieldObjectLabels.Remove(@object);
		}

		RefreshUI();
	}

	public void MoveObject(Collection collection, Object @object, Object newParent, int newLocalIndex, Dictionary<Field, Field> newFieldOverrides = null)
	{
		if (!HasProject())
			return;
		if (@object == null)
			return;

		var wasCurrent = currentObject == @object;
		var oldParent = @object.Parent ?? collection.Root;
		newParent ??= collection.Root;
		var parentChanged = (oldParent != newParent);

		// Remove the object from the collection, then restore it to the given position
		RemoveObject(collection, @object, true);
		RestoreObject(collection, newParent, newLocalIndex, @object);

		/*if (parentChanged)
		{
			@object.Parent = newParent;
		}

		if (currentCollection == collection)
		{
			var parentItem = newParent?.ListItem;
			parentItem.Collapsed = false;
			objectList.ReparentListItem(@object.ListItem, parentItem, newLocalIndex, false);

			objectList.UpdateGraphics();
		}

		if (collection != null)
		{
			var oldidx = collection.objects.FindIndex(match => match == @object);
			var newidx = -1;

			if (newParent != null)
			{
				newidx = collection.objects.FindIndex(match => match == newParent);
			}

			// Remove from original location
			if (oldidx >= 0)
			{
				collection.objects.RemoveRange(oldidx, 1 + @object.ChildCount);
			}

			var objIndex = -1;

			if (newidx >= 0)
			{
				var currentIndex = newidx;
				var remaining = newLocalIndex + 1;

				while (remaining > 0 && currentIndex < newidx + newParent.ChildCount && currentIndex < (collection.objects.Count - 1))
				{
					var obj = collection.objects[++currentIndex];
					if (obj.Parent == newParent)
					{
						remaining--;
					}
				}

				if (remaining == 0)
				{
					objIndex = currentIndex;
				}
			}

			if (objIndex >= 0)
			{
				var objList = new List<Object> { @object };
				objList.AddRange(@object.Children);
				collection.objects.InsertRange(objIndex, objList);
			}
			else
			{
				var objList = new List<Object> { @object };
				objList.AddRange(@object.Children);
				collection.objects.AddRange(objList);
			}
		}*/

		if (parentChanged)
		{
			// Restore field overrides
			if (newFieldOverrides != null)
			{
				foreach (var field in newFieldOverrides.Keys)
				{
					@object.fieldOverrides[field] = newFieldOverrides[field];
				}
			}

			// Remove field overrides that are not valid in new ancestry
			var keys = new List<Field>();
			foreach (var key in @object.fieldOverrides.Keys)
			{
				keys.Add(key);
			}

			foreach (var field in keys)
			{
				var fieldValid = false;

				var objParent = @object.Parent;
				while (!fieldValid && objParent != null)
				{
					if (objParent.fields.Contains(field) || objParent.fieldOverrides.ContainsKey(field))
					{
						fieldValid = true;
						break;
					}

					objParent = objParent.Parent;
				}

				if (!fieldValid)
				{
					//field.RemoveLink(@object.fieldOverrides[field]);
					@object.fieldOverrides.Remove(field);
				}
			}
		}

		if (wasCurrent)
		{
			LoadObject(@object);
		}

		if (objectFilter.Text != "")
		{
			FilterObjects();
		}

		RefreshUI();
	}

	public void RenameObject(Collection collection, Object @object, string newName)
	{
		if (collection.Root == @object)
		{
			collection.Name = newName;
			EnsureUnique(collection);
		}
		else
		{
			@object.Name = newName;
			EnsureUnique(@object, collection);
		}

		if (designListCategories.ContainsKey(@object))
		{
			designListCategories[@object].Text = @object.Name;
		}

		if (fieldObjectLabels.ContainsKey(@object))
		{
			fieldObjectLabels[@object].Text = @object.Name;
		}

		if (currentCollection == collection)
		{
			if (Objects.Contains(@object))
			{
				if (objectFilter.Text != "")
				{
					ExpandObjects();
					FilterObject(@object, objectFilter.Text);
				}
			}
		}
	}

	public void OnAddTypePressed()
	{
		if (!HasProject())
			return;
		if (currentCollection == null)
			return;

		var @object = new Object(null, null, true);
		PushCommand(new AddObjectCommand(this, currentCollection, currentObject, @object));

		LoadObject(@object);
		@object.ListItem?.Edit();
	}

	public void OnAddObjectPressed()
	{
		if (!HasProject())
			return;
		if (currentCollection == null)
			return;

		var @object = new Object(null, null, false);
		PushCommand(new AddObjectCommand(this, currentCollection, currentObject, @object));

		LoadObject(@object);
		@object.ListItem?.Edit();
	}

	public void OnObjectsFilterTextChanged(string newText)
	{
		if (!HasProject())
			return;
		if (Objects == null)
			return;

		if (newText != null)
		{
			//ExpandObjects();
			FilterObjects();
		}
	}

	public void OnObjectListItemDeleted(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex)
	{
		if (!HasProject())
			return;

		if (listItem.MetaData is Object obj)
		{
			PushCommand(new DeleteObjectCommand(this, currentCollection, oldParent.MetaData as Object, oldLocalIndex, obj), false);
			RemoveObject(currentCollection, obj, false);
		}

		RefreshUI();
	}

	public void OnObjectListItemRenamed(TreeListItem listItem, string newName)
	{
		if (!HasProject())
			return;

		if (listItem.MetaData is Object obj)
		{
			if (obj.Name != newName)
			{
				PushCommand(new RenameObjectCommand(this, currentCollection, obj, obj.Name, newName));
			}
		}
	}

	public void OnObjectListNoItemPressed()
	{
		UnloadCurrentObject();
	}

	public void OnObjectListItemPressed(TreeListItem listItem)
	{
		if (!HasProject())
			return;
		if (currentCollection == null)
			return;

		if (listItem.MetaData is Object obj)
		{
			if (obj != currentObject)
			{
				UnloadCurrentObject();
				LoadObject(obj);
			}
		}
	}

	public void OnObjectListItemDoubleClicked(TreeListItem listItem)
	{
		if (!HasProject())
			return;
		if (currentCollection == null)
			return;

		if (listItem.MetaData is Object obj)
		{
			LoadObject(obj);
			obj.ListItem?.Edit();
		}
	}

	public void OnObjectListItemCollapseChanged(TreeListItem listItem, bool collapsed)
	{
		if (!HasProject())
			return;
		if (currentCollection == null)
			return;

		if (listItem.MetaData is Object)
		{
			if (!collapsed)
			{
				FilterObjects();
			}
		}
	}

	public void OnObjectListItemMoved(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex, int newLocalIndex)
	{
		if (currentCollection == null)
			return;

		if (listItem.MetaData is Object obj)
		{
			var oldParentObj = oldParent.MetaData as Object;
			var newParentObj = listItem.Parent?.MetaData as Object;

			PushCommand(new MoveObjectCommand(this, currentCollection, obj, oldParentObj, oldLocalIndex, newParentObj, newLocalIndex));
		}
	}

	private void EnsureUnique(Object obj, Collection collection = null)
	{
		collection ??= currentCollection;

		if (collection == null)
			return;

		var IDs = new List<int>();
		foreach (var item in collection.objects)
		{
			if (item == obj)
				continue;
			if (item.IsType != obj.IsType)
				continue;

			if (item?.Name.text == obj.Name.text)
			{
				IDs.Add(item.Name.id);
			}
		}

		var newID = obj.Name;
		if (IDs.Contains(newID.id))
		{
			newID.id = 2;

			while (IDs.Contains(newID.id))
			{
				newID.id++;
			}
		}

		obj.Name = newID;
	}

	private void EnsureUnique(Collection collection)
	{
		var IDs = new List<int>();
		foreach (var item in Collections)
		{
			if (item == collection)
				continue;

			if (item?.Name.text == collection.Name.text)
			{
				IDs.Add(item.Name.id);
			}
		}

		var newID = collection.Name;
		if (IDs.Contains(newID.id))
		{
			newID.id = 2;

			while (IDs.Contains(newID.id))
			{
				newID.id++;
			}
		}

		collection.Name = newID;
	}

	private void EnsureUnique(Field field, Object obj)
	{
		var IDs = new List<int>();
		foreach (var f in obj.fields)
		{
			if (f == field)
				continue;

			if (f?.Name.text == field.Name.text)
			{
				IDs.Add(f.Name.id);
			}
		}

		// Walk up ancestry
		var parent = obj.Parent;
		while (parent != null)
		{
			foreach (var f in parent.fields)
			{
				if (f == field)
					continue;

				if (f?.Name.text == field.Name.text)
				{
					IDs.Add(f.Name.id);
				}
			}

			parent = parent.Parent;
		}

		// Walk down ancestry
		foreach (var child in obj.Children)
		{
			foreach (var f in child.fields)
			{
				if (f == field)
					continue;

				if (f?.Name.text == field.Name.text)
				{
					IDs.Add(f.Name.id);
				}
			}
		}

		var newID = field.Name;
		if (IDs.Contains(newID.id))
		{
			newID.id = 2;

			while (IDs.Contains(newID.id))
			{
				newID.id++;
			}
		}

		field.Name = newID;
	}

	private void FilterFields()
	{
		var filter = designFilter.Text;

		foreach (var listItem in designList.ListItems)
		{
			if (listItem == null)
				continue;

			if (listItem.MetaData is Field field)
			{
				var filtered = !(filter == "" || field.Name.ToString().Matches(filter));
				listItem.Filtered = filtered;
			}
		}
	}

	private void FilterField(Field field, string filter)
	{
		if (field == null)
			return;

		TreeListItem listItem = null;

		foreach (var li in designList.ListItems)
		{
			var _field = li.MetaData as Field;
			if (_field == field)
			{
				listItem = li;
				break;
			}
		}

		if (listItem == null)
			return;

		var filtered = !(filter == "" || field.Name.ToString().Matches(filter));
		listItem.Filtered = filtered;
	}

	private void FilterCollections(string filter)
	{
		if (!HasProject())
			return;

		foreach (var collection in Collections)
		{
			FilterCollection(collection, filter);
		}
	}

	private void FilterCollection(Collection collection, string filter)
	{
		if (collection.ListItem == null)
			return;

		var filtered = !(filter == "" || collection.Name.ToString().Matches(filter));
		collection.ListItem.Filtered = filtered;
	}

	private void LoadCollections()
	{
		if (HasProject())
		{
			// @TODO
			ClearCollections();

			foreach (var collection in currentProject.collections)
			{
				AddCollection(collection, false);

				// Add dataEditedCallback to all object fields
				foreach (var @object in collection.objects)
				{
					foreach (var field in @object.fields)
					{
						field.DataEditedCallback = OnFieldDataEdited;
					}

					foreach (var fieldOverride in @object.fieldOverrides.Values)
					{
						fieldOverride.DataEditedCallback = OnFieldDataEdited;
					}
				}
			}
		}

		RefreshUI();
	}

	private void ClearCollections()
	{
		UnloadCurrentCollection();

		collectionList?.Clear();

		RefreshUI();
	}

	private void LoadCollection(Collection collection)
	{
		if (!HasProject())
			return;

		UnloadCurrentCollection();

		currentCollection = collection;
		collection.ListItem.Select();

		// Ensure collection's Root has a listItem
		if (collection.Root != null && collection.Root.ListItem == null)
		{
			collection.Root.ListItem = objectList.AddListItem(null, collection.Name, collectionIcon, collectionIcon, true);
			collection.Root.ListItem.CanBeDeleted = false;
			collection.Root.ListItem.CanDragSibling = false;
		}

		// Load objects
		foreach (var obj in collection.objects)
		{
			if (obj == collection.Root)
				continue;

			TreeListItem parent = collection.Root.ListItem;

			if (obj.Parent != null)
			{
				parent = obj.Parent.ListItem;
			}

			if (obj.IsType)
			{
				obj.ListItem = objectList.AddListItem(parent, obj.Name, typeOpenIcon, typeIcon, true);
			}
			else
			{
				obj.ListItem = objectList.AddListItem(parent, obj.Name, objectIcon, null, false);
			}
		}

		ExpandObjects();
		//FilterObjects();

		collection.Root.ListItem.UpdateState();
		objectList.UpdateGraphics();

		RefreshUI();
	}

	private void UnloadCurrentCollection()
	{
		if (currentCollection != null)
		{
			ClearObjects();

			currentCollection.ListItem?.Deselect();
			currentCollection = null;
		}

		RefreshUI();
	}

	private void ClearObjects()
	{
		UnloadCurrentObject();

		if (currentCollection != null)
		{
			foreach (var obj in currentCollection.objects)
			{
				obj.ListItem = null;
			}
		}

		objectList?.Clear();
	}

	private void LoadObject(Object obj)
	{
		UnloadCurrentObject();
		if (obj == null)
			return;

		if (!HasProject())
			return;
		if (currentCollection == null)
			return;

		currentObject = obj;
		obj.ListItem.Select();

		var objects = new List<Object>();
		var objParent = obj;
		while (objParent != null)
		{
			if (objParent != obj || obj.IsType)
			{
				objects.Add(objParent);
			}

			objParent = objParent.Parent;
		}

		objects.Reverse();

		var font = GetThemeDefaultFont().Duplicate() as DynamicFont;
		font.Size += 4;

		// Create a local dictionary of field overrides per object ancestry
		var fieldOverrides = new Dictionary<Field, Field>();
		foreach (var _obj in objects)
		{
			if (_obj == obj)
				continue;

			foreach (var key in _obj.fieldOverrides.Keys)
			{
				fieldOverrides[key] = _obj.fieldOverrides[key];
			}
		}

		foreach (var _obj in objects)
		{
			var inherited = (_obj != obj);

			var label = new Label
			{
				Text = _obj.Name
			};

			fieldList.AddChild(label);

			label.AddFontOverride("font", font);
			label.Modulate = new Color(1.0f, 1.0f, 1.0f, inherited ? 0.5f : 1.0f);

			fieldObjectLabels.Add(_obj, label);

			var iconOpen = typeOpenIcon;
			var iconClosed = typeIcon;

			if (!_obj.IsType)
			{
				iconOpen = objectIcon;
				iconClosed = objectIcon;
			}

			if (_obj == currentCollection.Root)
			{
				iconOpen = collectionIcon;
				iconClosed = collectionIcon;
			}

			var parentItem = designList.AddListItem(null, _obj.Name, iconOpen, iconClosed, true);
			parentItem.CanBeDeleted = false;
			parentItem.CanBeRenamed = !inherited;
			parentItem.CanDragSibling = false;
			parentItem.Inherited = inherited;
			parentItem.MetaData = _obj;

			designListCategories.Add(_obj, parentItem);
			designFieldList.Add(null);

			foreach (var field in _obj.fields)
			{
				if (field == null)
					continue;
				var _field = field;

				// Check if field was overridden
				var wasOverride = false;
				if (obj.fieldOverrides.ContainsKey(field))
				{
					wasOverride = true;
					_field = obj.fieldOverrides[field];
				}
				else if (fieldOverrides.ContainsKey(field))
				{
					_field = fieldOverrides[field];
				}

				if (!inherited)
				{
					EnsureUnique(field, _obj);
				}

				Texture icon = null;
				switch (field.Type)
				{
					default:
						break;
					case Field.FieldType.Text:
						icon = toolTextIcon;
						break;
					case Field.FieldType.String:
						icon = toolStringIcon;
						break;
					case Field.FieldType.Number:
						icon = toolNumberIcon;
						break;
					case Field.FieldType.Boolean:
						icon = toolBooleanIcon;
						break;
					case Field.FieldType.Image:
						icon = toolImageIcon;
						break;
				}

				var listItem = designList.AddListItem(parentItem, field.Name, icon, null, false);
				designFieldList.Add(field);
				field.ListItem = listItem;

				listItem.MetaData = field;
				listItem.Inherited = inherited;
				listItem.Overriding = wasOverride;

				if (inherited)
				{
					listItem.CanBeDeleted = false;
					listItem.CanBeRenamed = false;
				}

				var editor = _field.CreateEditor(inherited);
				_field.SetOverriding(wasOverride);

				if (editor != null)
				{
					fieldList.AddChild(editor);
				}
			}

			if (inherited && _obj.IsType && (obj.IsType || _obj != obj.Parent))
			{
				var separator = new HSeparator();
				fieldList.AddChild(separator);
			}
		}

		designList.UpdateGraphics();
		RefreshUI();
	}

	private void UnloadCurrentObject()
	{
		currentField?.ListItem?.Deselect();
		currentField = null;

		foreach (var field in designFieldList)
		{
			if (field == null)
				continue;

			field.RemoveEditor();
			field.RemoveInspector();
			field.ListItem = null;
		}

		if (currentObject != null)
		{
			currentObject.ListItem?.Deselect();
			currentObject = null;
		}

		designList.Clear();
		designFieldList.Clear();
		designListCategories.Clear();
		fieldObjectLabels.Clear();

		foreach (Control field in fieldList.GetChildren())
		{
			field.QueueFree();
		}

		RefreshUI();
	}

	private void ExpandObjects()
	{
		foreach (var obj in Objects)
		{
			if (obj.ListItem != null && obj.ListItem.CanContainItems)
			{
				obj.ListItem.Collapsed = false;
			}
		}
	}

	private void CollapseObjects()
	{
		foreach (var obj in Objects)
		{
			if (obj.ListItem != null && obj.ListItem.CanContainItems)
			{
				obj.ListItem.Collapsed = true;
			}
		}
	}

	private void FilterObjects()
	{
		foreach (var obj in Objects)
		{
			FilterObject(obj, objectFilter.Text, false);
		}
	}

	private void FilterObject(Object obj, string filter, bool filterParents = true)
	{
		if (obj.ListItem == null)
			return;

		if (filter == "")
		{
			obj.ListItem.Filtered = false;
			return;
		}

		if (obj == currentCollection?.Root)
		{
			obj.ListItem.Filtered = false;
			return;
		}

		var wasFiltered = obj.ListItem.Filtered;
		var filtered = !(filter == "" || obj.Name.ToString().Matches(filter));

		// If unfiltered, ensure parents are visible
		if (!filtered)
		{
			var p = obj;

			while (p.Parent != null)
			{
				p = p.Parent;
				if (p.ListItem == null)
					continue;

				if (p.ListItem.Filtered)
				{
					p.ListItem.Filtered = false;
				}
				else
				{
					// Early-out if an unfiltered parent is found (this means the entire ancestry should be unfiltered already)
					break;
				}
			}
		}
		// If filtered but wasn't already filtered, filter parents
		else if (!wasFiltered && filterParents)
		{
			FilterObject(obj.Parent, filter);
		}

		obj.ListItem.Filtered = filtered;
	}

	private void RefreshUI()
	{
		objAddType.Enabled = (currentCollection != null/* && (currentObject == null || currentObject.IsType)*/);
		objAddObject.Enabled = (currentCollection != null/* && (currentObject == null || currentObject.IsType)*/);

		//toolBar.Visible = (currentCollection != null && currentObject != null);
		//dataWindows.Visible = (currentCollection != null && currentObject != null);

		SetToolsEnabled(currentCollection != null && currentObject != null);

		if (currentObject == null)
		{
			designFilter.Text = "";
			designFilter.Editable = false;
			designFilter.PlaceholderAlpha = 1.0f;

			SetToolsEnabled(false);
		}
		else
		{
			designFilter.Editable = true;
			designFilter.PlaceholderAlpha = 0.6f;

			SetToolsEnabled(currentObject.IsType);
		}

		if (currentCollection == null)
		{
			objectFilter.Text = "";
			objectFilter.Editable = false;
			objectFilter.PlaceholderAlpha = 1.0f;
		}
		else
		{
			objectFilter.Editable = true;
			objectFilter.PlaceholderAlpha = 0.6f;
		}

		if (currentProject == null)
		{
			collectionFilter.Text = "";
			collectionFilter.Editable = false;
			collectionFilter.PlaceholderAlpha = 1.0f;
		}
		else
		{
			collectionFilter.Editable = true;
			collectionFilter.PlaceholderAlpha = 0.6f;
		}
	}
}
