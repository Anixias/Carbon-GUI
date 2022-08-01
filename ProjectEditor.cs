using Godot;
using System;
using System.Globalization;
using System.Collections.Generic;
using Glint;
using Glint.Collections;
using NativeServices;

public abstract class EditorCommand
{
	protected ProjectEditor editor;
	
	public EditorCommand(ProjectEditor editor)
	{
		this.editor = editor;
	}
	
	public abstract void Execute();
	public abstract void Undo();
	
	public override string ToString()
	{
		return "<Command>";
	}
}

public interface ITestable
{
	public bool Test();
}

// --- Fields ---
public class CreateFieldCommand : EditorCommand
{
	protected Object obj;
	protected Texture icon;
	protected Field field;
	
	public CreateFieldCommand(ProjectEditor editor, Object obj, Texture icon, Field field) : base(editor)
	{
		this.obj = obj;
		this.icon = icon;
		this.field = field;
	}
	
	public override void Execute()
	{
		editor.CreateField(obj, icon, field);
	}
	
	public override void Undo()
	{
		editor.DeleteField(obj, field);
	}
	
	public override string ToString()
	{
		return "Create Field: " + field.Name;
	}
}

public class DeleteFieldCommand : EditorCommand
{
	protected Object obj;
	protected Texture icon;
	protected Field field;
	protected int localIndex;
	
	public DeleteFieldCommand(ProjectEditor editor, Object obj, Texture icon, Field field, int localIndex) : base(editor)
	{
		this.obj = obj;
		this.icon = icon;
		this.field = field;
		this.localIndex = localIndex;
	}
	
	public override void Execute()
	{
		editor.DeleteField(obj, field);
	}
	
	public override void Undo()
	{
		editor.RestoreField(obj, icon, field, localIndex);
	}
	
	public override string ToString()
	{
		return "Delete Field: " + field.Name;
	}
}

public class RenameFieldCommand : EditorCommand, ITestable
{
	protected Object obj;
	protected Field field;
	protected string originalName;
	protected string newName;
	
	public RenameFieldCommand(ProjectEditor editor, Object obj, Field field, string originalName, string newName) : base(editor)
	{
		this.obj = obj;
		this.field = field;
		this.originalName = originalName;
		this.newName = newName;
	}
	
	public override void Execute()
	{
		editor.RenameField(obj, field, newName);
		newName = field.Name; // Update newName to the true newName after EnsureUnique has executed
	}
	
	public override void Undo()
	{
		editor.RenameField(obj, field, originalName);
		originalName = field.Name; // Update newName to the true newName after EnsureUnique has executed
	}
	
	public bool Test()
	{
		Execute();
		return (originalName != newName);
	}
	
	public override string ToString()
	{
		return "Rename Field: '" + originalName + "' => '" + newName + "'";
	}
}

public class MoveFieldCommand : EditorCommand
{
	protected Object obj;
	protected Field field;
	protected int oldLocalIndex;
	protected int newLocalIndex;
	
	public MoveFieldCommand(ProjectEditor editor, Object obj, Field field, int oldLocalIndex, int newLocalIndex) : base(editor)
	{
		this.obj = obj;
		this.field = field;
		this.oldLocalIndex = oldLocalIndex;
		this.newLocalIndex = newLocalIndex;
	}
	
	public override void Execute()
	{
		editor.MoveField(obj, field, newLocalIndex);
	}
	
	public override void Undo()
	{
		editor.MoveField(obj, field, oldLocalIndex);
	}
	
	public override string ToString()
	{
		return "Move Field: '" + field.Name;
	}
}

public class OverrideFieldCommand : EditorCommand
{
	protected Object obj;
	protected Field field;
	protected bool overriding;
	private Field fieldOverride = null;

	public OverrideFieldCommand(ProjectEditor editor, Object obj, Field field, bool overriding) : base(editor)
	{
		this.obj = obj;
		this.field = field;
		this.overriding = overriding;
	}
	
	public override void Execute()
	{
		if (overriding)
		{
			// Override the field
			editor.OverrideField(obj, field, fieldOverride);
		}
		else
		{
			// Remove existing field override
			if (obj.fieldOverrides.ContainsKey(field))
			{
				fieldOverride = obj.fieldOverrides[field];
			}
			else
			{
				fieldOverride = null;
			}
			
			editor.RemoveFieldOverride(obj, field);
		}
	}
	
	public override void Undo()
	{
		if (overriding)
		{
			// Remove new field override
			if (obj.fieldOverrides.ContainsKey(field))
			{
				fieldOverride = obj.fieldOverrides[field];
			}
			else
			{
				fieldOverride = null;
			}
			
			editor.RemoveFieldOverride(obj, field);
		}
		else
		{
			// Restore existing field override
			editor.OverrideField(obj, field, fieldOverride);
		}
	}
	
	public override string ToString()
	{
		return (overriding ? "Override Field" : "Remove Field Override") + ": '" + field.Name;
	}
}

public class EditFieldCommand : EditorCommand
{
	protected Object obj;
	protected Field field;
	protected object oldData;
	protected object newData;
	
	public EditFieldCommand(ProjectEditor editor, Object obj, Field field, object oldData, object newData) : base(editor)
	{
		this.obj = obj;
		this.field = field;
		this.oldData = oldData;
		this.newData = newData;
	}
	
	public override void Execute()
	{
		field.SetData(newData);
	}
	
	public override void Undo()
	{
		field.SetData(oldData);
	}
	
	public override string ToString()
	{
		return "Edit Field: '" + oldData + "' => '" + newData + "'";
	}
}

// --- Collections ---
public class AddCollectionCommand : EditorCommand
{
	protected Collection collection;
	
	public AddCollectionCommand(ProjectEditor editor, Collection collection) : base(editor)
	{
		this.collection = collection;
	}
	
	public override void Execute()
	{
		editor.AddCollection(collection);
	}
	
	public override void Undo()
	{
		editor.RemoveCollection(collection);
	}
	
	public override string ToString()
	{
		return "Add Collection: " + collection.Name;
	}
}

public class DeleteCollectionCommand : EditorCommand
{
	protected Collection collection;
	protected int listIndex = -1;
	
	public DeleteCollectionCommand(ProjectEditor editor, Collection collection, int listIndex) : base(editor)
	{
		this.collection = collection;
		this.listIndex = listIndex;
	}
	
	public override void Execute()
	{
		editor.RemoveCollection(collection);
	}
	
	public override void Undo()
	{
		editor.RestoreCollection(collection, listIndex);
	}
	
	public override string ToString()
	{
		return "Delete Collection: " + collection.Name;
	}
}

public class MoveCollectionCommand : EditorCommand
{
	protected Collection collection;
	protected int oldListIndex;
	protected int newListIndex;
	
	public MoveCollectionCommand(ProjectEditor editor, Collection collection, int oldListIndex, int newListIndex) : base(editor)
	{
		this.collection = collection;
		this.oldListIndex = oldListIndex;
		this.newListIndex = newListIndex;
	}
	
	public override void Execute()
	{
		editor.MoveCollection(collection, newListIndex);
	}
	
	public override void Undo()
	{
		editor.MoveCollection(collection, oldListIndex);
	}
	
	public override string ToString()
	{
		return "Move Collection: " + oldListIndex + " => " + newListIndex;
	}
}

public class RenameCollectionCommand : EditorCommand, ITestable
{
	protected Collection collection;
	protected string originalName;
	protected string newName;
	
	public RenameCollectionCommand(ProjectEditor editor, Collection collection, string originalName, string newName) : base(editor)
	{
		this.collection = collection;
		this.originalName = originalName;
		this.newName = newName;
	}
	
	public override void Execute()
	{
		editor.RenameCollection(collection, newName);
		newName = collection.Name; // Update newName to the true value after EnsureUnique has executed
	}
	
	public override void Undo()
	{
		editor.RenameCollection(collection, originalName);
		originalName = collection.Name; // Update originalName to the true value after EnsureUnique has executed
	}
	
	public bool Test()
	{
		Execute();
		return (originalName != newName);
	}
	
	public override string ToString()
	{
		return "Rename Collection: '" + originalName + "' => '" + newName + "'";
	}
}

// --- Objects ---
public class AddObjectCommand : EditorCommand
{
	protected Collection collection;
	protected Object parent;
	protected Object obj;
	
	public AddObjectCommand(ProjectEditor editor, Collection collection, Object parent, Object obj) : base(editor)
	{
		this.collection = collection;
		this.parent = parent ?? collection?.root;
		this.obj = obj;
		
		while(this.parent != null && !this.parent.isType)
		{
			this.parent = this.parent.Parent;
		}
	}
	
	public override void Execute()
	{
		editor.AddObject(collection, parent, obj);
	}
	
	public override void Undo()
	{
		editor.RemoveObject(collection, obj);
	}
	
	public override string ToString()
	{
		return "Add Object: " + obj.Name;
	}
}

public class DeleteObjectCommand : EditorCommand
{
	protected Collection collection;
	protected Object parent;
	protected int localIndex;
	protected Object obj;
	
	public DeleteObjectCommand(ProjectEditor editor, Collection collection, Object parent, int localIndex, Object obj) : base(editor)
	{
		this.collection = collection;
		this.parent = parent ?? collection?.root;
		this.localIndex = localIndex;
		this.obj = obj;
	}
	
	public override void Execute()
	{
		editor.RemoveObject(collection, obj);
	}
	
	public override void Undo()
	{
		editor.RestoreObject(collection, parent, localIndex, obj);
	}
	
	public override string ToString()
	{
		return "Delete Object: " + obj.Name;
	}
}

public class RenameObjectCommand : EditorCommand, ITestable
{
	protected Collection collection;
	protected Object obj;
	protected string originalName;
	protected string newName;
	
	public RenameObjectCommand(ProjectEditor editor, Collection collection, Object obj, string originalName, string newName) : base(editor)
	{
		this.collection = collection;
		this.obj = obj;
		this.originalName = originalName;
		this.newName = newName;
	}
	
	public override void Execute()
	{
		editor.RenameObject(collection, obj, newName);
		newName = obj.Name;
	}
	
	public override void Undo()
	{
		editor.RenameObject(collection, obj, originalName);
		originalName = obj.Name;
	}
	
	public bool Test()
	{
		Execute();
		return (originalName != newName);
	}
	
	public override string ToString()
	{
		return "Rename Object: '" + originalName + "' => '" + newName + "'";
	}
}

public class MoveObjectCommand : EditorCommand
{
	protected Collection collection;
	protected Object obj;
	protected Object oldParent;
	protected int oldLocalIndex;
	protected Object newParent;
	protected int newLocalIndex;
	protected Dictionary<Field, Field> fieldOverrides;
	
	public MoveObjectCommand(ProjectEditor editor, Collection collection, Object obj, Object oldParent, int oldLocalIndex, Object newParent, int newLocalIndex) : base(editor)
	{
		this.collection = collection;
		this.obj = obj;
		this.oldParent = oldParent ?? collection?.root;
		this.oldLocalIndex = oldLocalIndex;
		this.newParent = newParent ?? collection?.root;
		this.newLocalIndex = newLocalIndex;
		
		while(this.oldParent != null && !this.oldParent.isType)
		{
			this.oldParent = this.oldParent.Parent;
		}
		
		while(this.newParent != null && !this.newParent.isType)
		{
			this.newParent = this.newParent.Parent;
		}
	}
	
	public override void Execute()
	{
		fieldOverrides = new Dictionary<Field, Field>(obj.fieldOverrides);
		editor.MoveObject(collection, obj, newParent, newLocalIndex);
	}
	
	public override void Undo()
	{
		editor.MoveObject(collection, obj, oldParent, oldLocalIndex, fieldOverrides);
	}
	
	public override string ToString()
	{
		return "Move Object: " + obj.Name;
	}
}

/*public class MoveListItemCommand : EditorCommand
{
	private TreeList treeList;
	private TreeListItem listItem;
	private TreeListItem oldParent;
	private TreeListItem newParent;
	private int oldIndex;
	private int newIndex;
	
	public MoveListItemCommand(ProjectEditor editor, TreeList treeList, TreeListItem listItem, TreeListItem oldParent, int oldIndex, int newIndex) : base(editor)
	{
		this.treeList = treeList;
		this.listItem = listItem;
		this.oldParent = oldParent;
		this.newParent = listItem.Parent;
		this.oldIndex = oldIndex;
		this.newIndex = newIndex;
	}
	
	public override void Execute()
	{
		treeList?.ReparentListItem(listItem, newParent, false);
		treeList?.MoveListItem(listItem, newIndex, false);
	}
	
	public override void Undo()
	{
		treeList?.ReparentListItem(listItem, oldParent, false);
		treeList?.MoveListItem(listItem, oldIndex, false);
	}
	
	public override string ToString()
	{
		return "Move List Item: '" + listItem.Text + "'";
	}
}*/

public class Project
{
	public static string defaultPath = OS.GetEnvironment("USERPROFILE") + "\\Documents\\";
	public List<Collection> collections;
	public string path { get; private set; }
	
	public Project()
	{
		collections = new List<Collection>();
	}
	
	public bool SaveAs()
	{
		var path = NativeFileDialog.SaveFileDialog("Save project as...", defaultPath + "project.carbon", new[] { "*.carbon" }, "Carbon Project");
		if (path == "") return false;
		
		var dir = new Directory();
		if (!dir.DirExists(path.GetBaseDir()))
		{
			dir.MakeDirRecursive(path.GetBaseDir());
		}
		
		var file = new File();
		file.Open(path, File.ModeFlags.Write);
		file.Close();
		
		this.path = path;
		
		return true;
	}
}

public abstract class Field
{
	public enum FieldType
	{
		None,
		String,
		Text,
		Number,
		Boolean,
		Image // @TODO change to "asset"?
		// @TODO ObjectReference
	}

	public delegate void DataEdited(Field field, object previousData);
	protected DataEdited dataEditedCallback;

	public FieldType Type
	{
		get => type;
	}
	
	public object Data
	{
		get => data;
		set
		{
			var prevData = Data;
			data = value;
			
			UpdateEditor();
			
			if (Data != prevData && dataEditedCallback != null)
			{
				dataEditedCallback(this, prevData);
			}
		}
	}
	
	public virtual void SetData(object data)
	{
		this.data = data;
		UpdateEditor();
	}
	
	public string ID
	{
		get => id;
	}
	
	public Glint.UniqueName Name
	{
		get => name;
		set
		{
			name = value;
			
			GenerateID();
			
			foreach(var link in linkedFields)
			{
				link.Name = name;
			}
			
			UpdateDisplay();
			UpdateEditor();
		}
	}
	
	public TreeListItem ListItem
	{
		get => listItem;
		set
		{
			listItem = value;
			UpdateDisplay();
		}
	}
	
	public virtual FieldEditor GetEditor()
	{
		return (HasEditor() ? editor : null);
	}
	
	private void GenerateID()
	{
		if (Name == "")
		{
			id = "";
			return;
		}
		
		TextInfo textInfo = new CultureInfo("en-us", false).TextInfo;
		var _id = textInfo.ToTitleCase(Name);
		_id = Char.ToLowerInvariant(_id[0]) + _id.Substring(1);
		_id = _id.Replace("_", string.Empty);
		_id = _id.Replace(" ", string.Empty);
		
		id = _id;
	}
	
	private void UpdateDisplay()
	{
		if (listItem != null && Godot.Object.IsInstanceValid(listItem))
		{
			listItem.Text = name;
		}
	}
	
	public virtual bool HasEditor()
	{
		return (editor != null && Godot.Object.IsInstanceValid(editor));
	}
	
	public virtual void UpdateEditor()
	{
		if (HasEditor())
		{
			editor.UpdateState();
		}
	}
	
	public virtual void RemoveEditor()
	{
		editor?.QueueFree();
		editor = null;
	}
	
	protected object data;
	protected Glint.UniqueName name;
	protected string id;
	
	private TreeListItem listItem = null;
	protected FieldType type = FieldType.None;
	protected FieldEditor editor;
	protected List<Field> linkedFields = new List<Field>();
	
	public abstract FieldEditor CreateEditor(bool inherited);
	public abstract void SetEditorOverriding(bool overriding);
	public abstract Field Duplicate();
	
	~Field()
	{
		RemoveEditor();
	}
	
	public void AddLink(Field link)
	{
		linkedFields.Add(link);
	}
	
	public void RemoveLink(Field link)
	{
		linkedFields.Remove(link);
	}
}

public class StringField : Field
{
	public new string Data
	{
		get
		{
			if (data != null)
			{
				return data.ToString();
			}
			
			return "";
		}
		set
		{
			var prevData = Data;
			data = value;
			
			UpdateEditor();
			
			if (Data != prevData && dataEditedCallback != null)
			{
				dataEditedCallback(this, prevData);
			}
		}
	}
	
	public List<string> Options;
	
	public StringField(string name, string data = "", DataEdited callback = null)
	{
		Name = name;
		this.data = data;
		type = FieldType.String;
		
		Options = new List<string>();
		dataEditedCallback = callback;
	}
	
	protected new StringFieldEditor editor;
	
	public override bool HasEditor()
	{
		return (editor != null && Godot.Object.IsInstanceValid(editor));
	}
	
	public override FieldEditor GetEditor()
	{
		return (HasEditor() ? editor : null);
	}
	
	public override FieldEditor CreateEditor(bool inherited)
	{
		RemoveEditor();
		
		var editorScene = ResourceLoader.Load<PackedScene>("res://StringFieldEditor.tscn");
		var editorInstance = editorScene.Instance<StringFieldEditor>();
		editorInstance.Field = this;
		editorInstance.Inherited = inherited;
		
		editor = editorInstance;
		return editor;
	}
	
	public override void SetEditorOverriding(bool overriding)
	{
		if (HasEditor())
		{
			editor.Overriding = overriding;
		}
	}
	
	public override void UpdateEditor()
	{
		if (HasEditor())
		{
			editor.UpdateState();
		}
	}
	
	public override void RemoveEditor()
	{
		editor?.QueueFree();
		editor = null;
	}

	public override Field Duplicate()
	{
		var field = new StringField(name);
		field.data = data;
		field.dataEditedCallback = dataEditedCallback;

		return field;
	}
}

public class TextField : Field
{
	public new string Data
	{
		get
		{
			if (data != null)
			{
				return data.ToString();
			}
			
			return "";
		}
		set
		{
			var prevData = Data;
			data = value;
			
			UpdateEditor();
			
			if (Data != prevData && dataEditedCallback != null)
			{
				dataEditedCallback(this, prevData);
			}
		}
	}
	
	public TextField(Glint.UniqueName name, string data = "", DataEdited callback = null)
	{
		this.name = name;
		this.data = data;
		type = FieldType.Text;
		dataEditedCallback = callback;
	}
	
	protected new TextFieldEditor editor;
	
	public override bool HasEditor()
	{
		return (editor != null && Godot.Object.IsInstanceValid(editor));
	}
	
	public override FieldEditor GetEditor()
	{
		return (HasEditor() ? editor : null);
	}
	
	public override FieldEditor CreateEditor(bool inherited)
	{
		RemoveEditor();
		
		var editorScene = ResourceLoader.Load<PackedScene>("res://TextFieldEditor.tscn");
		var editorInstance = editorScene.Instance<TextFieldEditor>();
		editorInstance.Field = this;
		editorInstance.Inherited = inherited;
		
		editor = editorInstance;
		return editor;
	}
	
	public override void UpdateEditor()
	{
		if (HasEditor())
		{
			editor.UpdateState();
		}
	}
	
	public override void RemoveEditor()
	{
		editor?.QueueFree();
		editor = null;
	}
	
	public override void SetEditorOverriding(bool overriding)
	{
		if (HasEditor())
		{
			editor.Overriding = overriding;
		}
	}

	public override Field Duplicate()
	{
		var field = new TextField(name);
		field.data = data;
		field.dataEditedCallback = dataEditedCallback;
		
		return field;
	}
}

public class NumberField : Field
{
	public new double Data
	{
		get
		{
			if (data != null)
			{
				return (double)data;
			}
			
			return 0d;
		}
		set
		{
			var prevData = Data;
			data = value;
			
			UpdateEditor();
			
			if (Data != prevData && dataEditedCallback != null)
			{
				dataEditedCallback(this, prevData);
			}
		}
	}
	
	public NumberField(Glint.UniqueName name, double data = 0d, DataEdited callback = null)
	{
		this.name = name;
		this.data = data;
		type = FieldType.Number;
		dataEditedCallback = callback;
	}
	
	protected new NumberFieldEditor editor;
	
	public override FieldEditor GetEditor()
	{
		return (HasEditor() ? editor : null);
	}
	
	public override bool HasEditor()
	{
		return (editor != null && Godot.Object.IsInstanceValid(editor));
	}
	
	public override FieldEditor CreateEditor(bool inherited)
	{
		RemoveEditor();
		
		var editorScene = ResourceLoader.Load<PackedScene>("res://NumberFieldEditor.tscn");
		var editorInstance = editorScene.Instance<NumberFieldEditor>();
		editorInstance.Field = this;
		editorInstance.Inherited = inherited;
		
		editor = editorInstance;
		return editor;
	}
	
	public override void UpdateEditor()
	{
		if (HasEditor())
		{
			editor.UpdateState();
		}
	}
	
	public override void RemoveEditor()
	{
		editor?.QueueFree();
		editor = null;
	}
	
	public override void SetEditorOverriding(bool overriding)
	{
		if (HasEditor())
		{
			editor.Overriding = overriding;
		}
	}

	public override Field Duplicate()
	{
		var field = new NumberField(name);
		field.data = data;
		field.dataEditedCallback = dataEditedCallback;
		
		return field;
	}
}

public class BooleanField : Field
{
	public new bool Data
	{
		get
		{
			if (data != null)
			{
				return (bool)data;
			}
			
			return false;
		}
		set
		{
			var prevData = Data;
			data = value;
			
			UpdateEditor();
			
			if (Data != prevData && dataEditedCallback != null)
			{
				dataEditedCallback(this, prevData);
			}
		}
	}
	
	public BooleanField(Glint.UniqueName name, bool data = false, DataEdited callback = null)
	{
		this.name = name;
		this.data = data;
		type = FieldType.Boolean;
		dataEditedCallback = callback;
	}
	
	protected new BooleanFieldEditor editor;
	
	public override bool HasEditor()
	{
		return (editor != null && Godot.Object.IsInstanceValid(editor));
	}
	
	public override FieldEditor GetEditor()
	{
		return (HasEditor() ? editor : null);
	}
	
	public override FieldEditor CreateEditor(bool inherited)
	{
		RemoveEditor();
		
		var editorScene = ResourceLoader.Load<PackedScene>("res://BooleanFieldEditor.tscn");
		var editorInstance = editorScene.Instance<BooleanFieldEditor>();
		editorInstance.Field = this;
		editorInstance.Inherited = inherited;
		
		editor = editorInstance;
		return editor;
	}
	
	public override void UpdateEditor()
	{
		if (HasEditor())
		{
			editor.UpdateState();
		}
	}
	
	public override void RemoveEditor()
	{
		editor?.QueueFree();
		editor = null;
	}
	
	public override void SetEditorOverriding(bool overriding)
	{
		if (HasEditor())
		{
			editor.Overriding = overriding;
		}
	}

	public override Field Duplicate()
	{
		var field = new BooleanField(name);
		field.data = data;
		field.dataEditedCallback = dataEditedCallback;
		
		return field;
	}
}

public class ImageField : Field
{
	// Path
	public new Image Data
	{
		get
		{
			if (data != null)
			{
				return data as Image;
			}
			
			return null;
		}
		set
		{
			var prevData = Data;
			data = value;
			
			UpdateEditor();
			
			if (Data != prevData && dataEditedCallback != null)
			{
				dataEditedCallback(this, prevData);
			}
		}
	}
	
	public ImageField(Glint.UniqueName name, Image data = null, DataEdited callback = null)
	{
		this.name = name;
		this.data = data;
		type = FieldType.Image;
		dataEditedCallback = callback;
	}
	
	public void LoadImage(string path)
	{
		if (ResourceLoader.Exists(path, "Image"))
		{
			data = ResourceLoader.Load<Image>(path, "Image");
		}
		else
		{
			data = null;
		}
	}
	
	public override FieldEditor CreateEditor(bool inherited)
	{
		RemoveEditor();
		
		var editor = ResourceLoader.Load<PackedScene>("res://StringFieldEditor.tscn");
		return editor.Instance<StringFieldEditor>();
	}
	
	public override void SetEditorOverriding(bool overriding)
	{
		if (HasEditor())
		{
			editor.Overriding = overriding;
		}
	}

	public override Field Duplicate()
	{
		var field = new ImageField(name);
		field.data = data;
		field.dataEditedCallback = dataEditedCallback;
		
		return field;
	}
}

public class Object
{
	private Guid guid;
	private TreeListItem listItem;
	private Object parent;
	private List<Object> children;
	public readonly bool isType;
	public List<Field> fields;
	public Dictionary<Field, Field> fieldOverrides;
	
	public Object Parent
	{
		get => parent;
		set
		{
			parent?.children.Remove(this);
			parent = value;
			parent?.children.Add(this);
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
	
	public int ChildCount
	{
		get
		{
			var count = 0;
			
			foreach(var child in children)
			{
				count += 1 + child.ChildCount;
			}
			
			return count;
		}
	}
	
	public List<Object> Children
	{
		get
		{
			var list = new List<Object>();
			
			foreach(var child in children)
			{
				list.Add(child);
				list.AddRange(child.Children);
			}
			
			return list;
		}
	}
	
	public TreeListItem ListItem
	{
		get => listItem;
		set
		{
			listItem = value;
			
			if (listItem != null)
			{
				listItem.MetaData = this;
			}
		}
	}
	
	public Glint.UniqueName Name
	{
		get => name;
		set
		{
			name = value;
			UpdateDisplay();
		}
	}
	
	private Glint.UniqueName name;
	
	// Editor-only
	public bool editorCollapsed = false;
	
	public Object(TreeListItem listItem = null, Object parent = null, bool isType = false)
	{
		guid = Guid.NewGuid();
		fields = new List<Field>();
		fieldOverrides = new Dictionary<Field, Field>();
		children = new List<Object>();
		
		ListItem = listItem;
		Parent = parent;
		this.isType = isType;
		
		if (listItem != null)
		{
			name = listItem.Text;
		}
		else
		{
			name = isType ? "New type" : "New object";
		}
	}
	
	~Object()
	{
		listItem?.QueueFree();
	}
	
	private void UpdateDisplay()
	{
		if (listItem != null)
		{
			listItem.Text = Name;
		}
	}
	
	public bool IsAncestorOf(Object obj)
	{
		var objParent = obj.Parent;
		
		while (objParent != null)
		{
			if (objParent == this)
			{
				return true;
			}
			
			objParent = objParent.Parent;
		}
		
		return false;
	}
	
	public Field GetFieldAncestor(Field field)
	{
		var objParent = parent;
		while(objParent != null)
		{
			if (objParent.fieldOverrides.ContainsKey(field))
			{
				return objParent.fieldOverrides[field];
			}
			
			objParent = objParent.parent;
		}

		return field;
	}
}

public class Collection
{
	private Guid guid;
	private TreeListItem listItem;
	public List<Object> objects;
	public readonly Object root;
	
	public TreeListItem ListItem
	{
		get => listItem;
		set
		{
			listItem = value;
			
			if (listItem != null)
			{
				listItem.MetaData = this;
			}
		}
	}
	
	public Glint.UniqueName Name
	{
		get => name;
		set
		{
			name = value;
			UpdateDisplay();
		}
	}
	
	private Glint.UniqueName name = "New collection";
	
	public Collection(TreeListItem listItem = null, Object root = null)
	{
		guid = Guid.NewGuid();
		
		ListItem = listItem;
		objects = new List<Object>();
		
		this.root = root ?? new Object(null, null, true);
		objects.Add(this.root);
		
		if (listItem != null)
		{
			name = listItem.Text;
		}
	}
	
	~Collection()
	{
		if (listItem != null && Godot.Object.IsInstanceValid(listItem))
		{
			if (!listItem.IsQueuedForDeletion())
			{
				listItem.QueueFree();
			}
		}
	}
	
	private void UpdateDisplay()
	{
		if (listItem != null)
		{
			listItem.Text = name;
		}
		
		if (root != null)
		{
			root.Name = name;
		}
	}
}

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
	private History<EditorCommand> commands = new History<EditorCommand>();
	 
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
	private Panel toolBar;
	private HBoxContainer toolList;
	private VSplitContainer dataWindows;
	private ScrollContainer workArea;
	private VBoxContainer fieldList;
	private MarginContainer fieldMarginContainer;
	private TreeList designList;
	private TextInputBox designFilter;
	private Dictionary<Object, TreeListItem> designListCategories;
	private List<Field> designFieldList;
	private Dictionary<Object, Label> fieldObjectLabels;
	
	private List<IconButton> tools;
	
	public override void _Ready()
	{
		currentProject = null;
		currentCollection = null;
		currentObject = null;
		
		treeListItem = ResourceLoader.Load<PackedScene>("res://TreeListItem.tscn");
		iconButton = ResourceLoader.Load<PackedScene>("res://IconButton.tscn");
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
		toolBar = GetNode<Panel>("HSplitContainer/VBoxContainer/ToolBar");
		toolList = GetNode<HBoxContainer>("HSplitContainer/VBoxContainer/ToolBar/MarginContainer/ToolList");
		workArea = GetNode<ScrollContainer>("HSplitContainer/VBoxContainer/WorkArea");
		fieldMarginContainer = GetNode<MarginContainer>("HSplitContainer/VBoxContainer/WorkArea/MarginContainer");
		fieldList = GetNode<VBoxContainer>("HSplitContainer/VBoxContainer/WorkArea/MarginContainer/FieldList");
		dataWindows = GetNode<VSplitContainer>("HSplitContainer/DataWindows");
		designList = GetNode<TreeList>("HSplitContainer/DataWindows/DesignPanel/MarginContainer/VBoxContainer/Panel/TreeList");
		designFilter = GetNode<TextInputBox>("HSplitContainer/DataWindows/DesignPanel/MarginContainer/VBoxContainer/Filter");
		
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
		if (project == currentProject) return;
		
		UnloadCurrentObject();
		UnloadCurrentCollection();
		ClearCollections();
		commands.Clear();
		
		currentProject = project;
		
		if (currentProject != null)
		{
			LoadCollections();
		}
		
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
			if (execute) command.Execute();
			
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
		foreach(var tool in tools)
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
		if (obj == null) return;
		
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
		if (obj == null) return;
		
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
		if (obj == null) return;
		if (field == null) return;
		
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
		if (obj == null) return;
		if (field == null) return;
		
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
		
		fieldOverride.SetEditorOverriding(true);
		
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
		designFilter.Text = "";
	}
	
	public void OnToolTextPressed()
	{
		var field = new TextField("Text", "", OnFieldDataEdited);
		PushCommand(new CreateFieldCommand(this, currentObject, toolTextIcon, field));
		field.ListItem?.Edit();
		designFilter.Text = "";
	}
	
	public void OnToolNumberPressed()
	{
		var field = new NumberField("Number", 0, OnFieldDataEdited);
		PushCommand(new CreateFieldCommand(this, currentObject, toolNumberIcon, field));
		field.ListItem?.Edit();
		designFilter.Text = "";
	}
	
	public void OnToolBooleanPressed()
	{
		var field = new BooleanField("Boolean", false, OnFieldDataEdited);
		PushCommand(new CreateFieldCommand(this, currentObject, toolBooleanIcon, field));
		field.ListItem?.Edit();
		designFilter.Text = "";
	}
	
	public void OnToolImagePressed()
	{
		var field = new ImageField("Image", null, OnFieldDataEdited);
		PushCommand(new CreateFieldCommand(this, currentObject, toolImageIcon, field));
		field.ListItem?.Edit();
		designFilter.Text = "";
	}
	
	public void OnDesignListItemDeleted(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex)
	{
		if (!HasProject()) return;
		if (currentObject == null) return;
		
		var field = listItem.MetaData as Field;
		if (field == null) return;
		
		PushCommand(new DeleteFieldCommand(this, currentObject, listItem.Icon, field, oldLocalIndex), false);
		DeleteField(currentObject, field, false);
	}
	
	public void OnDesignListItemRenamed(TreeListItem listItem, string newName)
	{
		if (!HasProject()) return;
		if (currentObject == null) return;
		
		var field = listItem.MetaData as Field;
		if (field == null)
		{
			var obj = listItem.MetaData as Object;
			if (obj != null && obj.Name != newName)
			{
				PushCommand(new RenameObjectCommand(this, currentCollection, obj, obj.Name, newName));
			}
			
			return;
		}
		
		if (field.Name != newName)
		{
			PushCommand(new RenameFieldCommand(this, currentObject, field, field.Name, newName));
		}
	}
	
	public void OnDesignListItemPressed(TreeListItem listItem)
	{
		if (!HasProject()) return;
		if (currentObject == null) return;
		
		// @TODO
	}
	
	public void OnDesignListItemDoubleClicked(TreeListItem listItem)
	{
		if (!HasProject()) return;
		if (currentObject == null) return;
		
		listItem.Edit();
	}
	
	public void OnDesignListItemMoved(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex, int newLocalIndex)
	{
		if (!HasProject()) return;
		if (currentObject == null) return;
		
		var field = listItem.MetaData as Field;
		if (field == null) return;
		
		PushCommand(new MoveFieldCommand(this, currentObject, field, oldLocalIndex, newLocalIndex));
	}
	
	public void OnDesignListItemOverridingChanged(TreeListItem listItem, bool overriding)
	{
		if (!HasProject()) return;
		if (currentObject == null) return;
		
		var field = listItem.MetaData as Field;
		if (field == null) return;
		
		PushCommand(new OverrideFieldCommand(this, currentObject, field, overriding));
	}
	
	public void OnDesignFilterTextChanged(string newText)
	{
		FilterFields();
	}
	
	private void ReplaceNode(Node current, Node target)
	{
		if (current == null || !Godot.Object.IsInstanceValid(current)) return;
		if (target == null || !Godot.Object.IsInstanceValid(target)) return;
		
		var parent = current.GetParent();
		if (parent == null) return;
		
		var pos = current.GetPositionInParent();
		parent.RemoveChild(current);
		parent.AddChild(target);
		parent.MoveChild(target, pos);
	}
	
	public void AddCollection(Collection collection)
	{
		var btn = collectionList.AddListItem(null, collection.Name, collectionIcon, null, false);
		btn.MetaData = collection;
		collection.ListItem = btn;
		Collections.Insert(btn.ListIndex, collection);
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
		if (!HasProject()) return;
		
		var collection = new Collection();
		PushCommand(new AddCollectionCommand(this, collection));

		collectionFilter.Text = "";
		collection.ListItem?.Edit();
	}
	
	public void OnCollectionListItemDeleted(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex)
	{
		if (!HasProject()) return;
		
		var collection = listItem.MetaData as Collection;
		
		PushCommand(new DeleteCollectionCommand(this, collection, oldLocalIndex), false);
		RemoveCollection(collection, false);
	}
	
	public void OnCollectionListItemMoved(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex, int newLocalIndex)
	{
		var collection = listItem.MetaData as Collection;
		if (collection == null) return;
		
		Collections.Remove(collection);
		Collections.Insert(newLocalIndex, collection);
		
		PushCommand(new MoveCollectionCommand(this, collection, oldLocalIndex, newLocalIndex), false);
	}
	
	public void OnCollectionListItemRenamed(TreeListItem listItem, string newName)
	{
		if (!HasProject()) return;
		
		var collection = listItem.MetaData as Collection;
		if (collection == null) return;
		
		if (collection.Name != newName)
		{
			PushCommand(new RenameCollectionCommand(this, collection, collection.Name, newName));
		}
	}
	
	public void OnCollectionListItemDoubleClicked(TreeListItem listItem)
	{
		if (!HasProject()) return;
		
		var collection = listItem.MetaData as Collection;
		if (collection != null && collection != currentCollection)
		{
			LoadCollection(collection);
		}
	}
	
	public void OnCollectionsFilterTextChanged(string newText)
	{
		FilterCollections(newText);
	}
	
	public void AddObject(Collection collection, Object parent, Object @object)
	{
		if (!HasProject()) return;
		
		@object.ListItem = null;
		@object.Parent = parent;
		
		if (currentCollection == collection)
		{
			var parentItem = parent.ListItem;
			
			var btn = @object.isType ? objectList.AddListItem(parentItem, @object.Name, typeOpenIcon, typeIcon, true)
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
		if (!HasProject()) return;
		if (@object == null) return;
		
		parent ??= collection.root;
		@object.ListItem = null;
		
		if (currentCollection == collection)
		{
			var parentItem = parent?.ListItem;
			
			var btn = @object.isType ? objectList.AddListItem(parentItem, @object.Name, typeOpenIcon, typeIcon, true)
									 : objectList.AddListItem(parentItem, @object.Name, objectIcon, null, false);
			parentItem.Collapsed = false;
			@object.ListItem = btn;
			
			objectList.MoveListItem(btn, localIndex, false);
		
			foreach(var child in @object.Children)
			{
				parentItem = child.Parent?.ListItem;
				
				btn = child.isType ? objectList.AddListItem(parentItem, child.Name, typeOpenIcon, typeIcon, true)
								   : objectList.AddListItem(parentItem, child.Name, objectIcon, null, false);
				parentItem.Collapsed = false;
				child.ListItem = btn;
			}
			
			objectList.UpdateGraphics();
		}
		
		@object.Parent = parent;
		EnsureUnique(@object);
		
		if (collection != null)
		{
			var idx = -1;
			
			if (parent != null)
			{
				idx = collection.objects.FindIndex(match => match == parent);
			}
			
			var objIndex = -1;
			
			if (idx >= 0)
			{
				var currentIndex = idx;
				var remaining = localIndex + 1;
				
				while(remaining > 0 && currentIndex < idx + parent.ChildCount && currentIndex < (collection.objects.Count - 1))
				{
					var obj = collection.objects[++currentIndex];
					if (obj.Parent == parent)
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
		if (@object == null) return;
		
		if (@object == currentObject || @object.Children.Contains(currentObject))
		{
			UnloadCurrentObject();
		}
		
		if (removeFromTree && currentCollection == collection)
		{
			objectList.RemoveListItem(@object.ListItem);
			objectList.UpdateGraphics();
		}
		
		foreach(var child in @object.Children)
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
		if (!HasProject()) return;
		if (@object == null) return;
		
		var oldParent = @object.Parent ?? collection.root;
		newParent ??= collection.root;
		
		var parentChanged = (oldParent != newParent);
		
		if (parentChanged)
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
				
				while(remaining > 0 && currentIndex < newidx + newParent.ChildCount && currentIndex < (collection.objects.Count - 1))
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
		}
		
		if (parentChanged)
		{
			// Restore field overrides
			if (newFieldOverrides != null)
			{
				foreach(var field in newFieldOverrides.Keys)
				{
					@object.fieldOverrides[field] = newFieldOverrides[field];
				}
			}
			
			// Remove field overrides that are not valid in new ancestry
			var keys = new List<Field>();
			foreach(var key in @object.fieldOverrides.Keys)
			{
				keys.Add(key);
			}
			
			foreach(var field in keys)
			{
				var fieldValid = false;
				
				var objParent = @object.Parent;
				while(!fieldValid && objParent != null)
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
			
			if (@object == currentObject || @object.IsAncestorOf(currentObject))
			{
				// Reload currentObject because its ancestry changed
				LoadObject(currentObject);
			}
		}
		
		if (objectFilter.Text != "")
		{
			FilterObjects();
		}
		
		RefreshUI();
	}
	
	public void RenameObject(Collection collection, Object @object, string newName)
	{
		if (collection.root == @object)
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
		if (!HasProject()) return;
		if (currentCollection == null) return;
		
		var @object = new Object(null, null, true);
		PushCommand(new AddObjectCommand(this, currentCollection, currentObject, @object));
		
		LoadObject(@object);
		@object.ListItem?.Edit();
	}
	
	public void OnAddObjectPressed()
	{
		if (!HasProject()) return;
		if (currentCollection == null) return;
		
		var @object = new Object(null, null, false);
		PushCommand(new AddObjectCommand(this, currentCollection, currentObject, @object));
		
		LoadObject(@object);
		@object.ListItem?.Edit();
	}
	
	public void OnObjectsFilterTextChanged(string newText)
	{
		if (!HasProject()) return;
		if (Objects == null) return;
		
		if (newText != null)
		{
			//ExpandObjects();
			FilterObjects();
		}
	}
	
	public void OnObjectListItemDeleted(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex)
	{
		if (!HasProject()) return;
		
		var obj = listItem.MetaData as Object;
		if (obj == null) return;
		
		PushCommand(new DeleteObjectCommand(this, currentCollection, oldParent.MetaData as Object, oldLocalIndex, obj), false);
		RemoveObject(currentCollection, obj, false);
		
		RefreshUI();
	}
	
	public void OnObjectListItemRenamed(TreeListItem listItem, string newName)
	{
		if (!HasProject()) return;
		
		var obj = listItem.MetaData as Object;
		if (obj == null) return;
		
		if (obj.Name != newName)
		{
			PushCommand(new RenameObjectCommand(this, currentCollection, obj, obj.Name, newName));
		}
	}
	
	public void OnObjectListNoItemPressed()
	{
		UnloadCurrentObject();
	}
	
	public void OnObjectListItemPressed(TreeListItem listItem)
	{
		if (!HasProject()) return;
		if (currentCollection == null) return;
		
		var obj = listItem.MetaData as Object;
		if (obj != null)
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
		if (!HasProject()) return;
		if (currentCollection == null) return;
		
		var obj = listItem.MetaData as Object;
		if (obj != null)
		{
			LoadObject(obj);
			
			obj.ListItem?.Edit();
		}
	}
	
	public void OnObjectListItemCollapseChanged(TreeListItem listItem, bool collapsed)
	{
		if (!HasProject()) return;
		if (currentCollection == null) return;
		
		var obj = listItem.MetaData as Object;
		if (obj != null && !collapsed)
		{
			FilterObjects();
		}
	}
	
	public void OnObjectListItemMoved(TreeListItem listItem, TreeListItem oldParent, int oldLocalIndex, int newLocalIndex)
	{
		if (currentCollection == null) return;
		
		var @object = listItem.MetaData as Object;
		if (@object == null) return;
		
		var oldParentObj = oldParent.MetaData as Object;
		var newParentObj = listItem.Parent?.MetaData as Object;
		
		PushCommand(new MoveObjectCommand(this, currentCollection, @object, oldParentObj, oldLocalIndex, newParentObj, newLocalIndex));
	}
	
	private void EnsureUnique(Object obj, Collection collection = null)
	{
		collection ??= currentCollection;
		
		if (collection == null) return;
		
		var IDs = new List<int>();
		foreach(var item in collection.objects)
		{
			if (item == obj) continue;
			if (item.isType != obj.isType) continue;
			
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
		foreach(var item in Collections)
		{
			if (item == collection) continue;
			
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
		foreach(var f in obj.fields)
		{
			if (f == field) continue;
			
			if (f?.Name.text == field.Name.text)
			{
				IDs.Add(f.Name.id);
			}
		}
		
		// Walk up ancestry
		var parent = obj.Parent;
		while(parent != null)
		{
			foreach(var f in parent.fields)
			{
				if (f == field) continue;
				
				if (f?.Name.text == field.Name.text)
				{
					IDs.Add(f.Name.id);
				}
			}
			
			parent = parent.Parent;
		}
		
		// Walk down ancestry
		foreach(var child in obj.Children)
		{
			foreach(var f in child.fields)
			{
				if (f == field) continue;
				
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
		
		foreach(var listItem in designList.ListItems)
		{
			if (listItem == null) continue;
			
			var field = listItem.MetaData as Field;
			if (field == null) continue;
			
			var filtered = !(filter == "" || field.Name.ToString().Matches(filter));
			listItem.Filtered = filtered;
		}
	}
	
	private void FilterField(Field field, string filter)
	{
		if (field == null) return;
		
		TreeListItem listItem = null;
		
		foreach(var li in designList.ListItems)
		{
			var _field = li.MetaData as Field;
			if (_field == field)
			{
				listItem = li;
				break;
			}
		}
		
		if (listItem == null) return;
		
		var filtered = !(filter == "" || field.Name.ToString().Matches(filter));
		listItem.Filtered = filtered;
	}
	
	private void FilterCollections(string filter)
	{
		if (!HasProject()) return;
		
		foreach(var collection in Collections)
		{
			FilterCollection(collection, filter);
		}
	}
	
	private void FilterCollection(Collection collection, string filter)
	{
		if (collection.ListItem == null) return;
		
		var filtered = !(filter == "" || collection.Name.ToString().Matches(filter));
		collection.ListItem.Filtered = filtered;
	}
	
	private void LoadCollections()
	{
		if (HasProject())
		{
			// @TODO
		}
		
		RefreshUI();
	}
	
	private void ClearCollections()
	{
		UnloadCurrentCollection();
		
		collectionList?.Clear();
		Collections?.Clear();
		
		RefreshUI();
	}
	
	private void LoadCollection(Collection collection)
	{
		if (!HasProject()) return;
		
		UnloadCurrentCollection();
		
		currentCollection = collection;
		collection.ListItem.Select();
		
		// Ensure collection's root has a listItem
		if (collection.root != null && collection.root.ListItem == null)
		{
			collection.root.ListItem = objectList.AddListItem(null, collection.Name, collectionIcon, collectionIcon, true);
			collection.root.ListItem.CanBeDeleted = false;
			collection.root.ListItem.CanDragSibling = false;
		}
		
		// Load objects
		foreach(var obj in collection.objects)
		{
			if (obj == collection.root) continue;
			
			TreeListItem parent = collection.root.ListItem;
			
			if (obj.Parent != null)
			{
				parent = obj.Parent.ListItem;
			}
			
			if (obj.isType)
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
		
		collection.root.ListItem.UpdateState();
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
			foreach(var obj in currentCollection.objects)
			{
				obj.ListItem = null;
			}
		}
		
		objectList?.Clear();
	}
	
	private void LoadObject(Object obj)
	{
		UnloadCurrentObject();
		if (obj == null) return;
		
		if (!HasProject()) return;
		if (currentCollection == null) return;
		
		currentObject = obj;
		obj.ListItem.Select();
		
		var objects = new List<Object>();
		var objParent = obj;
		while(objParent != null)
		{
			if (objParent != obj || obj.isType)
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
		foreach(var _obj in objects)
		{
			if (_obj == obj) continue;
			
			foreach(var key in _obj.fieldOverrides.Keys)
			{
				fieldOverrides[key] = _obj.fieldOverrides[key];
			}
		}
		
		foreach(var _obj in objects)
		{
			var inherited = (_obj != obj);
			
			var label = new Label();
			label.Text = _obj.Name;
			fieldList.AddChild(label);
			
			label.AddFontOverride("font", font);
			label.Modulate = new Color(1.0f, 1.0f, 1.0f, inherited ? 0.5f : 1.0f);
			
			fieldObjectLabels.Add(_obj, label);
			
			var iconOpen = typeOpenIcon;
			var iconClosed = typeIcon;
			
			if (!_obj.isType)
			{
				iconOpen = objectIcon;
				iconClosed = objectIcon;
			}
			
			if (_obj == currentCollection.root)
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
			
			foreach(var field in _obj.fields)
			{
				if (field == null) continue;
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
				switch(field.Type)
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
				_field.SetEditorOverriding(wasOverride);
				
				if (editor != null)
				{
					fieldList.AddChild(editor);
				}
			}
			
			if (inherited && _obj.isType && (obj.isType || _obj != obj.Parent))
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
		foreach(var field in designFieldList)
		{
			if (field == null) continue;
			
			field.RemoveEditor();
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
		
		foreach(Control field in fieldList.GetChildren())
		{
			field.QueueFree();
		}
		
		RefreshUI();
	}
	
	private void ExpandObjects()
	{
		foreach(var obj in Objects)
		{
			if (obj.ListItem != null && obj.ListItem.CanContainItems)
			{
				obj.ListItem.Collapsed = false;
			}
		}
	}
	
	private void CollapseObjects()
	{
		foreach(var obj in Objects)
		{
			if (obj.ListItem != null && obj.ListItem.CanContainItems)
			{
				obj.ListItem.Collapsed = true;
			}
		}
	}
	
	private void FilterObjects()
	{
		foreach(var obj in Objects)
		{
			FilterObject(obj, objectFilter.Text, false);
		}
	}
	
	private void FilterObject(Object obj, string filter, bool filterParents = true)
	{
		if (obj.ListItem == null) return;
		
		if (filter == "")
		{
			obj.ListItem.Filtered = false;
			return;
		}
		
		if (obj == currentCollection?.root)
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
			
			while(p.Parent != null)
			{
				p = p.Parent;
				if (p.ListItem == null) continue;

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
		objAddType.Enabled = (currentCollection != null/* && (currentObject == null || currentObject.isType)*/);
		objAddObject.Enabled = (currentCollection != null/* && (currentObject == null || currentObject.isType)*/);
		
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
			
			SetToolsEnabled(currentObject.isType);
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
