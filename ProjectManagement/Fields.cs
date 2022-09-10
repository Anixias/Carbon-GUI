using System;
using System.Globalization;
using System.Collections.Generic;
using Godot;
using Glint;

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
	
	private Guid guid = Guid.NewGuid();
	public ref readonly Guid ID { get => ref guid; }
	
	public UniqueName Name
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
	protected UniqueName name;
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
	
	public Dictionary<string, object> Write()
	{
		var data = new Dictionary<string, object>();
		
		data["name"] = name.ToString();
		data["type"] = type.ToString();
		data["data"] = WriteData();
		
		return data;
	}
	
	public virtual object WriteData()
	{
		return data.ToString();
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
	
	public override object WriteData()
	{
		return Data;
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
	
	public override object WriteData()
	{
		return Data;
	}
	
	public TextField(UniqueName name, string data = "", DataEdited callback = null)
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
	
	public override object WriteData()
	{
		return Data;
	}
	
	public NumberField(UniqueName name, double data = 0d, DataEdited callback = null)
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
	
	public override object WriteData()
	{
		return Data;
	}
	
	public BooleanField(UniqueName name, bool data = false, DataEdited callback = null)
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
	
	public override object WriteData()
	{
		return null;
	}
	
	public ImageField(UniqueName name, Image data = null, DataEdited callback = null)
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

#region Editor Commands

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

#endregion
