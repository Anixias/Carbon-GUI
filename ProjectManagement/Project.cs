using System;
using System.Collections.Generic;
using Godot;
using NativeServices;

public class Project
{
	public static readonly Version version = new Version("0.1");
	public List<Collection> collections;
	public string path { get; private set; } = DefaultPath + "project.carbon";
	
	public static readonly string DefaultPath = OS.GetEnvironment("USERPROFILE") + "\\Documents\\";
	
	public Project()
	{
		collections = new List<Collection>();
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
			file.Open(path, File.ModeFlags.Read);
			existingData = file.GetAsText();
			file.Close();
		}
		
		// Re-open the file for writing
		file.Open(path, File.ModeFlags.Write);
		
		try
		{
			file.StoreString(JSON.Print(Write(), "\t"));
		}
		catch(Exception e)
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