using System;
using System.Collections.Generic;
using Godot;
using NativeServices;

public class Project
{
	public static readonly Version version = new Version("0.1");
	public List<Collection> collections;
	public string path { get; private set; } = defaultPath + "project.carbon";
	
	public bool HasUnsavedChanges
	{
		get => lastHashCode != collections.GetHashCode();
	}
	
	private static readonly string defaultPath = OS.GetEnvironment("USERPROFILE") + "\\Documents\\";
	private int lastHashCode;
	
	public Project()
	{
		collections = new List<Collection>();
		lastHashCode = collections.GetHashCode();
	}
	
	public bool SaveAs()
	{
		var newPath = NativeFileDialog.SaveFileDialog("Save project as...", path, new[] { "*.carbon" }, "Carbon Project");
		if (newPath == null || newPath == "") return false;
		
		var baseDirectory = newPath.GetBaseDir();
		
		var dir = new Directory();
		if (!dir.DirExists(baseDirectory))
		{
			dir.MakeDirRecursive(baseDirectory);
		}
		
		path = newPath;
		Save();
		
		return true;
	}
	
	public void Save()
	{
		var file = new File();
		file.Open(path, File.ModeFlags.Write);
		
		file.StoreString(Write());
		
		file.Close();
		lastHashCode = collections.GetHashCode();
	}
	
	public bool Open()
	{
		var newPaths = NativeFileDialog.OpenFileDialog("Open project...", defaultPath, new[] { "*.carbon" }, "Carbon Project");
		if (newPaths == null) return false;
		
		var newPath = newPaths[0];
		if (newPath == null || newPath == "") return false;
		
		var file = new File();
		if (file.FileExists(newPath))
		{
			path = newPath;
			
			file.Open(path, File.ModeFlags.Read);
			Read(file.GetAsText());
			file.Close();
			
			lastHashCode = collections.GetHashCode();
			
			return true;
		}
		
		return false;
	}
	
	protected void Read(string data)
	{
		collections.Clear();
	}
	
	protected Dictionary<string, object> Write()
	{
		var data = new Dictionary<string, object>();
		var collectionData = new Dictionary<string, object>();
		
		foreach(var collection in collections)
		{
			collectionData[collection.ID.ToString()] = collection.Write();
		}
		
		data["version"] = version.ToString();
		data["collections"] = collectionData;
		
		return data;
	}
}