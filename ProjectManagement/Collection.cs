using System;
using System.Collections.Generic;
using Godot;
using Glint;

public class Collection
{
	private Guid guid;
	private TreeListItem listItem;
	public List<Object> objects;
	public readonly Object root;
	
	public ref readonly Guid ID { get => ref guid; }
	
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
	
	private UniqueName name = "New collection";
	
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
	
	public void Read(string data)
	{
		objects.Clear();
	}
	
	public string Write()
	{
		var data = new Dictionary<string, object>();
		var objectData = new Dictionary<string, object>();
		
		foreach(var @object in objects)
		{
			objectData[@object.ID.ToString()] = @object.Write();
		}
		
		data["name"] = name;
		data["objects"] = objectData;
		
		return JSON.Print(data, "\t");
	}
}

#region Editor Commands

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

#endregion