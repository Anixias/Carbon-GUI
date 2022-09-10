using System;
using System.Collections.Generic;
using Godot;
using Glint;

public class Object
{
	private Guid guid;
	private TreeListItem listItem;
	private Object parent;
	private List<Object> children;
	public readonly bool isType;
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
	
	public void Read(string data)
	{
		
	}
	
	public string Write()
	{
		throw new NotImplementedException();
		
		/*var data = new Dictionary<string, object>();
		
		foreach(var @object in objects)
		{
			data[@object.ID.ToString()] = @object.Write();
		}
		
		return JSON.Print(data, "\t");*/
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

#endregion
