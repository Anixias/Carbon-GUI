using Godot;
using Godot.Conversion;
using NativeServices;
using System;
using System.Collections.Generic;

public class Project
{
	public static readonly Version version = new Version("0.1");
	public List<Collection> collections;
	public string Path { get; private set; } = DefaultPath + "project.carbon";

	public static readonly string DefaultPath = OS.GetEnvironment("USERPROFILE") + "\\Documents\\";

	public Project()
	{
		collections = new List<Collection>();
	}

	public bool SaveAs()
	{
		var newPath = NativeFileDialog.SaveFileDialog("Save project as...", Path, new[] { "*.carbon" }, "Carbon Project");
		if (newPath == null || newPath == "")
			return false;

		var baseDirectory = newPath.GetBaseDir();

		var dir = new Directory();
		if (!dir.DirExists(baseDirectory))
		{
			dir.MakeDirRecursive(baseDirectory);
		}

		Path = newPath;
		Save(false);

		return true;
	}

	public void Save()
	{
		Save(true);
	}

	private void Save(bool saveExistingData)
	{
		var file = new File();
		var existingData = "";

		// First, read the contents in case there is a crash during the save
		if (saveExistingData)
		{
			file.Open(Path, File.ModeFlags.Read);
			existingData = file.GetAsText();
			file.Close();
		}

		// Re-open the file for writing
		file.Open(Path, File.ModeFlags.Write);

		try
		{
			file.StoreString(JSON.Print(Write(), "\t"));
		}
		catch (Exception e)
		{
			GD.PrintErr(e.Message);

			if (saveExistingData)
			{
				file.StoreString(existingData);
			}
		}
		finally
		{
			file.Close();
		}
	}

	public bool Open()
	{
		var newPaths = NativeFileDialog.OpenFileDialog("Open project...", DefaultPath, new[] { "*.carbon" }, "Carbon Project");
		if (newPaths == null)
			return false;

		var newPath = newPaths[0];
		if (newPath == null || newPath == "")
			return false;

		var file = new File();
		if (file.FileExists(newPath))
		{
			Path = newPath;

			file.Open(Path, File.ModeFlags.Read);
			var fileContents = file.GetAsText();
			file.Close();

			var jsonResult = JSON.Parse(fileContents);
			if (jsonResult.Result is Godot.Collections.Dictionary data)
			{
				Read(data.Convert<string, object>());
			}

			return true;
		}

		return false;
	}

	protected void Read(Dictionary<string, object> data)
	{
		T Load<T>(string key, T defaultValue)
		{
			if (data.ContainsKey(key) && data[key] is T value)
			{
				return value;
			}

			return defaultValue;
		}

		var dataVersion = version;
		try
		{
			var loadedVersion = Load<string>("version", null);

			if (loadedVersion != null)
			{
				dataVersion = new Version(loadedVersion);
			}
		}
		catch (Exception e)
		{
			GD.PrintErr(e.Message);
		}

		// Load collections
		collections.Clear();

		var collectionData = Load<Godot.Collections.Array>("collections", new Godot.Collections.Array() { }).ToList<object>(null);
		foreach (var collection in collectionData)
		{
			if (collection is Godot.Collections.Dictionary loadedCollectionData)
			{
				var loadedCollection = new Collection();
				loadedCollection.Read(loadedCollectionData.Convert<string, object>());

				collections.Add(loadedCollection);
			}
		}
	}

	protected Dictionary<string, object> Write()
	{
		var data = new Dictionary<string, object>();
		var collectionData = new List<object>();

		foreach (var collection in collections)
		{
			collectionData.Add(collection.Write());
		}

		data["version"] = version.ToString();
		data["collections"] = collectionData;

		return data;
	}
}