using Glint;
using Godot.Conversion;
using System;
using System.Collections.Generic;
using GDC = Godot.Collections;

public class Object
{
	private Guid guid;
	private TreeListItem listItem;
	private Object parent;
	private readonly List<Object> children;
	public bool IsRoot;

	private bool isType;
	public bool IsType
	{
		get => isType;
	}

	public List<Field> fields;
	public Dictionary<Field, Field> fieldOverrides;

	public ref readonly Guid ID { get => ref guid; }

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
		set
		{
			if (parent != null)
			{
				parent.children.Remove(this);
				parent.children.Insert(value, this);
			}
		}
	}

	public int ChildCount
	{
		get
		{
			var count = 0;

			foreach (var child in children)
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

			foreach (var child in children)
			{
				list.Add(child);
				list.AddRange(child.Children);
			}

			return list;
		}
	}

	public List<Object> ChildrenDirect
	{
		get
		{
			var list = new List<Object>();

			foreach (var child in children)
			{
				list.Add(child);
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

	public UniqueName Name
	{
		get => name;
		set
		{
			name = value;
			UpdateDisplay();
		}
	}

	private UniqueName name;

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

	public void SetParent(Object parent, int localIndex = -1)
	{
		this.parent?.children.Remove(this);
		this.parent = parent;

		if (localIndex < 0)
		{
			parent?.children.Add(this);
		}
		else
		{
			parent?.children.Insert(localIndex, this);
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
		while (objParent != null)
		{
			if (objParent.fieldOverrides.ContainsKey(field))
			{
				return objParent.fieldOverrides[field];
			}

			objParent = objParent.parent;
		}

		return field;
	}

	public Field FindField(Guid guid, bool includeOverrides = true)
	{
		foreach (var field in fields)
		{
			if (field.ID == guid)
			{
				return field;
			}
		}

		if (includeOverrides)
		{
			foreach (var field in fieldOverrides.Values)
			{
				if (field.ID == guid)
				{
					return field;
				}
			}
		}

		return null;
	}

	public void Read(Collection collection, Dictionary<string, object> data, in Dictionary<Guid, Object> objectLookup = null)
	{
		T Load<T>(string key, T defaultValue)
		{
			if (data.ContainsKey(key) && data[key] is T value)
			{
				return value;
			}

			return defaultValue;
		}

		name = new UniqueName(Load("name", ""));
		Guid.TryParse(Load("id", ""), out guid);
		isType = Load("is_type", true);

		// Parent
		Guid.TryParse(Load("parent", ""), out Guid parentID);

		if (objectLookup != null && objectLookup.ContainsKey(parentID))
		{
			Parent = objectLookup[parentID];
		}

		Parent ??= collection.Root;

		// Fields
		fields.Clear();
		var fieldData = Load("fields", new GDC.Array() { }).ToList<object>(null);

		foreach (var field in fieldData)
		{
			if (field is GDC.Dictionary loadedGDFieldData)
			{
				var loadedFieldData = loadedGDFieldData.Convert<string, object>();
				var loadedField = Field.Read(loadedFieldData);

				if (loadedField != null)
				{
					fields.Add(loadedField);
				}
			}
		}

		// Field Overrides
		fieldOverrides.Clear();
		var fieldOverrideList = Load("field_overrides", new GDC.Array() { }).ToArray(new GDC.Dictionary());

		foreach (var fieldOverrideEntry in fieldOverrideList)
		{
			var fieldOverrideData = fieldOverrideEntry.Convert<string, object>();

			if (fieldOverrideData.ContainsKey("override") && fieldOverrideData["override"] is string inheritedID)
			{
				if (fieldOverrideData.ContainsKey("data"))
				{
					if (Guid.TryParse(inheritedID, out Guid id))
					{
						// Find original Field
						var obj = parent;
						Field originalField = null;
						while (originalField == null && obj != null)
						{
							originalField = obj.FindField(id, false);
							obj = obj.parent;
						}

						if (originalField != null)
						{
							var overrideField = originalField.Duplicate();
							overrideField.SetData(fieldOverrideData["data"]);
							originalField.AddLink(overrideField);
							fieldOverrides[originalField] = overrideField;
						}
					}
				}
			}
		}
	}

	public Dictionary<string, object> Write()
	{
		var data = new Dictionary<string, object>();
		var fieldData = new List<object>();
		var fieldOverrideData = new List<object>();

		foreach (var field in fields)
		{
			fieldData.Add(field.Write());
		}

		foreach (var fieldOverride in fieldOverrides)
		{
			var inheritId = fieldOverride.Key.ID.ToString();
			fieldOverrideData.Add(new Dictionary<string, object>()
			{
				{ "override", inheritId },
				{ "data", fieldOverride.Value.WriteData() }
			});
		}

		string parentID = null;
		if (parent != null)
		{
			parentID = parent.ID.ToString();
		}

		data["name"] = name.ToString();
		data["id"] = ID.ToString();
		data["parent"] = parentID;
		data["is_type"] = IsType;
		data["fields"] = fieldData;
		data["field_overrides"] = fieldOverrideData;

		return data;
	}

	public Dictionary<string, object> Export()
	{
		// Get object's ancestry
		var ancestors = new List<Object>();
		var objParent = this;
		while (objParent != null)
		{
			ancestors.Add(objParent);
			objParent = objParent.Parent;
		}

		ancestors.Reverse();

		// Create a list of fields with correct overrides as well as track types as tags
		var tagList = new List<string>();
		var fieldOriginList = new List<Field>();
		var fieldFinalList = new List<Field>();
		foreach (var obj in ancestors)
		{
			if (!obj.IsRoot && obj != this)
			{
				tagList.Add(obj.Name);
			}

			// Add fields
			foreach (var field in obj.fields)
			{
				fieldOriginList.Add(field);
				fieldFinalList.Add(field);
			}

			// Override inherited fields
			foreach (var fieldOverride in obj.fieldOverrides)
			{
				var index = fieldOriginList.FindIndex(f => f == fieldOverride.Key);

				if (index != -1)
					fieldFinalList[index] = fieldOverride.Value;
			}
		}

		// Export fields
		var fieldData = new Dictionary<string, object>();
		foreach (var field in fieldFinalList)
		{
			fieldData[field.Name] = field.WriteData();
		}

		return fieldData;
	}
}

#region Editor Commands

public class AddObjectCommand : EditorCommand
{
	protected Collection collection;
	protected Object parent;
	protected Object obj;

	public AddObjectCommand(ProjectEditor editor, Collection collection, Object parent, Object obj) : base(editor)
	{
		this.collection = collection;
		this.parent = parent ?? collection?.Root;
		this.obj = obj;

		while (this.parent != null && !this.parent.IsType)
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
		this.parent = parent ?? collection?.Root;
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
		this.oldParent = oldParent ?? collection?.Root;
		this.oldLocalIndex = oldLocalIndex;
		this.newParent = newParent ?? collection?.Root;
		this.newLocalIndex = newLocalIndex;

		while (this.oldParent != null && !this.oldParent.IsType)
		{
			this.oldParent = this.oldParent.Parent;
		}

		while (this.newParent != null && !this.newParent.IsType)
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

#endregion
