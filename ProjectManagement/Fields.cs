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
	public DataEdited DataEditedCallback;

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
			
			if (Data != prevData && DataEditedCallback != null)
			{
				DataEditedCallback(this, prevData);
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
	
	public static Field Read(Dictionary<string, object> data)
	{
		T Load<T>(string key, T defaultValue)
		{
			if (data.ContainsKey(key) && data[key] is T value)
			{
				return value;
			}
			
			return defaultValue;
		}
		
		var name = Load<string>("name", "");
		var id = Guid.Empty;
		Guid.TryParse(Load<string>("id", ""), out id);
		var loadedData = Load<string>("data", null);
		
		Field output = null;
		
		var loadedType = FieldType.None;
		if (FieldType.TryParse(Load<string>("type", FieldType.None.ToString()), true, out loadedType))
		{
			switch(loadedType)
			{
				default:
					return null;
				case FieldType.String:
					output = new StringField(name, loadedData);
					output.guid = id;
					break;
				case FieldType.Text:
					output = new TextField(name, loadedData);
					output.guid = id;
					break;
				case FieldType.Number:
					var loadedDouble = 0.0;
					Double.TryParse(loadedData, out loadedDouble);
					output = new NumberField(name, loadedDouble);
					output.guid = id;
					break;
				case FieldType.Boolean:
					var loadedBool = false;
					Boolean.TryParse(loadedData, out loadedBool);
					output = new BooleanField(name, loadedBool);
					output.guid = id;
					break;
				case FieldType.Image:
					output = new ImageField(name, loadedData);
					output.guid = id;
					break;
			}
		}
		
		return output;
	}
	
	public Dictionary<string, object> Write()
	{
		var data = new Dictionary<string, object>();
		
		data["name"] = name.ToString();
		data["id"] = ID.ToString();
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
			
			if (Data != prevData && DataEditedCallback != null)
			{
				DataEditedCallback(this, prevData);
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
		DataEditedCallback = callback;
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
		field.DataEditedCallback = DataEditedCallback;

		return field;
	}

	public override void SetData(object data)
	{
		if (data is string)
		{
			this.data = data;
		}
		else
		{
			this.data = data.ToString();
		}
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
			
			if (Data != prevData && DataEditedCallback != null)
			{
				DataEditedCallback(this, prevData);
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
		DataEditedCallback = callback;
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
		field.DataEditedCallback = DataEditedCallback;
		
		return field;
	}
	
	public override void SetData(object data)
	{
		if (data is string)
		{
			this.data = data;
		}
		else
		{
			this.data = data.ToString();
		}
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
			
			if (Data != prevData && DataEditedCallback != null)
			{
				DataEditedCallback(this, prevData);
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
		DataEditedCallback = callback;
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
		field.DataEditedCallback = DataEditedCallback;
		
		return field;
	}
	
	public override void SetData(object data)
	{
		if (data is double)
		{
			this.data = data;
		}
		else if (data is string)
		{
			double value;
			if (Double.TryParse((string)data, out value))
			{
				this.data = value;
			}
		}
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
			
			if (Data != prevData && DataEditedCallback != null)
			{
				DataEditedCallback(this, prevData);
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
		DataEditedCallback = callback;
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
		field.DataEditedCallback = DataEditedCallback;
		
		return field;
	}
	
	public override void SetData(object data)
	{
		if (data is bool)
		{
			this.data = data;
		}
		else if (data is string)
		{
			bool value;
			if (Boolean.TryParse((string)data, out value))
			{
				this.data = value;
			}
		}
	}
}

public class ImageField : Field
{
	// Path
	public new string Data
	{
		get
		{
			if (data != null)
			{
				return data as string;
			}
			
			return null;
		}
		set
		{
			var prevData = Data;
			data = value;
			
			LoadImage(value);
			UpdateEditor();
			
			if (Data != prevData && DataEditedCallback != null)
			{
				DataEditedCallback(this, prevData);
			}
		}
	}
	
	// Image
	public Image Image
	{
		get => image;
	}
	
	protected new ImageFieldEditor editor;
	protected Image image;
	
	public override bool HasEditor()
	{
		return (editor != null && Godot.Object.IsInstanceValid(editor));
	}
	
	public override FieldEditor GetEditor()
	{
		return (HasEditor() ? editor : null);
	}
	
	public ImageField(Glint.UniqueName name, string path = null, Image image = null, DataEdited callback = null)
	{
		this.name = name;
		this.data = path;
		this.image = image;
		type = FieldType.Image;
		DataEditedCallback = callback;
	}
	
	public override void SetData(object data)
	{
		if (data is string || data == null)
		{
			this.data = data;
			LoadImage(this.data as string);
			UpdateEditor();
		}
	}
	
	public bool LoadImage(string path)
	{
		image = null;
		
		if (path == null || path == "") return false;
		path = path.Replace("\\", "/");
		
		var file = new File();
		if (!file.FileExists(path)) return false;
		
		image = new Image();
		if (image.Load(path) == Error.Ok)
		{
			var size = Image.GetSize();
			
			// Limit size to 256x256, scaling up if less than half that amount
			const float targetSize = 256.0f;
			var interpolation = Image.Interpolation.Nearest;
			var axis = Math.Max(size.x, size.y);
			var scale = 1.0f;
			
			while (axis * scale < targetSize)
			{
				if (axis * scale * 2.0f > targetSize) break;
				scale *= 2.0f;
			}
			
			while (axis * scale > targetSize)
			{
				interpolation = Image.Interpolation.Lanczos;
				scale /= 2.0f;
			}
			
			image.Resize((int)(size.x * scale), (int)(size.y * scale), interpolation);
			data = path;
			return true;
		}
		
		return false;
	}
	
	public override FieldEditor CreateEditor(bool inherited)
	{
		RemoveEditor();
		
		var editorScene = ResourceLoader.Load<PackedScene>("res://ImageFieldEditor.tscn");
		var editorInstance = editorScene.Instance<ImageFieldEditor>();
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
		var field = new ImageField(name);
		field.data = data;
		field.image = (Image)image?.Duplicate(true);
		field.DataEditedCallback = DataEditedCallback;
		
		return field;
	}
	
	public override object WriteData()
	{
		return Data;
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
		return "Move Field: '" + field.Name + "'";
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
		return (overriding ? "Override Field" : "Remove Field Override") + ": '" + field.Name + "'";
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
