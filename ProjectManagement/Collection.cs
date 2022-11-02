using Glint;
using Godot.Conversion;
using System;
using System.Collections.Generic;

public class Collection
{
	private Guid guid;
	private TreeListItem listItem;
	public List<Object> objects;

	private Object root;
	public Object Root
	{
		get => root;
	}

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

		this.root = root ??= new Object(null, null, true);
		root.IsRoot = true;
		objects.Add(root);

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

	public void Read(Dictionary<string, object> data)
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

		// Load objects
		root = null;
		objects.Clear();

		var objectData = Load("objects", new Godot.Collections.Array() { }).ToList<object>(null);
		var objectLookup = new Dictionary<Guid, Object>();
		foreach (var @object in objectData)
		{
			if (@object is Godot.Collections.Dictionary loadedObjectData)
			{
				var loadedObject = new Object();
				loadedObject.Read(this, loadedObjectData.Convert<string, object>(), objectLookup);
				objectLookup[loadedObject.ID] = loadedObject;
				objects.Add(loadedObject);

				if (loadedObject.Parent == null)
				{
					root = loadedObject;
					root.IsRoot = true;
				}
			}
		}
	}

	public Dictionary<string, object> Write()
	{
		var data = new Dictionary<string, object>();
		var objectData = new List<object>();

		foreach (var @object in objects)
		{
			objectData.Add(@object.Write());
		}

		data["name"] = name.ToString();
		data["id"] = ID.ToString();
		data["objects"] = objectData;

		return data;
	}

	public Dictionary<string, object> Export()
	{
		static Dictionary<string, object> ExpandType(Object type)
		{
			if (!type.IsType)
				return null;

			// Export each of its children, recursively expanding types
			var expansion = new Dictionary<string, object>();

			foreach (var child in type.ChildrenDirect)
			{
				expansion[child.Name] = child.IsType ? ExpandType(child) : child.Export();
			}

			return expansion;
		}

		return ExpandType(root);
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