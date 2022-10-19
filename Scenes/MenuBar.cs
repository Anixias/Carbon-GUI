using Godot;
using System.Collections.Generic;

public class MenuBar : HBoxContainer
{
	[Signal]
	public delegate void NewProject();

	[Signal]
	public delegate void OpenProject();

	[Signal]
	public delegate void SaveProject();

	[Signal]
	public delegate void SaveProjectAs();

	[Signal]
	public delegate void CloseProject();

	[Signal]
	public delegate void Undo();

	[Signal]
	public delegate void Redo();

	private Dictionary<string, int> itemIndices;
	private MenuButton mbProject;
	private MenuButton mbEdit;
	private MenuButton mbEditor;

	public override void _Ready()
	{
		itemIndices = new Dictionary<string, int>();

		mbProject = CreateMenuButton("Project", nameof(OnProjectMenuItemSelected));
		mbEdit = CreateMenuButton("Edit", nameof(OnEditMenuItemSelected));

		var padding = new Control();
		padding.SizeFlagsHorizontal |= (int)SizeFlags.Expand;
		padding.MouseFilter = MouseFilterEnum.Ignore;
		AddChild(padding);

		mbEditor = CreateMenuButton("Editor");

		int idx = -1;

		// Project
		idx = AddShortcut(mbProject, "New Project", nameof(OnProjectNew), KeyList.N, true);
		itemIndices.Add("Project::New", idx);
		SetEnabled(mbProject, idx, true); // Always enabled

		idx = AddShortcut(mbProject, "Open Project...", nameof(OnProjectOpen), KeyList.O, true);
		itemIndices.Add("Project::Open", idx);
		SetEnabled(mbProject, idx, true); // Always enabled

		idx = AddShortcut(mbProject, "Save Project", nameof(OnProjectSave), KeyList.S, true);
		itemIndices.Add("Project::Save", idx);

		idx = AddShortcut(mbProject, "Save Project As...", nameof(OnProjectSaveAs), KeyList.S, true, true);
		itemIndices.Add("Project::SaveAs", idx);

		idx = AddShortcut(mbProject, "Close Project", nameof(OnProjectClose), KeyList.W, true, true);
		itemIndices.Add("Project::Close", idx);

		// Edit
		idx = AddShortcut(mbEdit, "Undo", nameof(OnUndo), KeyList.Z, true);
		itemIndices.Add("Edit::Undo", idx);

		idx = AddShortcut(mbEdit, "Redo", nameof(OnRedo), KeyList.Y, true);
		itemIndices.Add("Edit::Redo", idx);
	}

	private MenuButton CreateMenuButton(string text, string callback = null)
	{
		var menuButton = new MenuButton
		{
			SwitchOnHover = true,
			Flat = false,
			Text = text
		};

		AddChild(menuButton);

		if (callback != null)
		{
			menuButton.GetPopup().Connect("index_pressed", this, callback);
		}

		menuButton.GetPopup().HideOnStateItemSelection = false;
		menuButton.GetPopup().HideOnCheckableItemSelection = false;

		return menuButton;
	}

	private int AddShortcut(MenuButton menuButton, string label, string method, KeyList scancode, bool cmd = false, bool shift = false, bool alt = false)
	{
		var popup = menuButton.GetPopup();
		var idx = popup.GetItemCount();

		var sc = new ShortCut
		{
			ResourceName = label,
			Shortcut = new InputEventKey()
		};

		var SC = ((InputEventKey)sc.Shortcut);
		SC.Command = cmd;
		SC.Shift = shift;
		SC.Alt = alt;
		SC.Scancode = (uint)scancode;

		popup.AddShortcut(sc);
		popup.SetItemDisabled(idx, true);
		popup.SetItemMetadata(idx, method);

		return idx;
	}

	private int AddFakeShortcut(MenuButton menuButton, string label, string method, KeyList scancode, bool cmd = false, bool shift = false, bool alt = false)
	{
		var idx = AddShortcut(menuButton, label, method, scancode, cmd, shift, alt);

		var popup = menuButton.GetPopup();
		popup.SetItemShortcutDisabled(idx, true);

		return idx;
	}

	private int AddSeparator(MenuButton menuButton, string label = "")
	{
		var popup = menuButton.GetPopup();
		var idx = popup.GetItemCount();
		popup.AddSeparator(label);

		return idx;
	}

	private void SetDisabled(MenuButton menuButton, int idx, bool disabled)
	{
		menuButton.GetPopup().SetItemDisabled(idx, disabled);
	}

	private void SetDisabled(MenuButton menuButton, string identifier, bool disabled)
	{
		if (itemIndices.ContainsKey(identifier))
		{
			menuButton.GetPopup().SetItemDisabled(itemIndices[identifier], disabled);
		}
	}

	private void SetEnabled(MenuButton menuButton, int idx, bool enabled)
	{
		SetDisabled(menuButton, idx, !enabled);
	}

	private void SetEnabled(MenuButton menuButton, string identifier, bool enabled)
	{
		SetDisabled(menuButton, identifier, !enabled);
	}

	private void Invoke(string methodName, object[] parameters = null)
	{
		var method = this.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

		method?.Invoke(this, parameters);
	}

	private void InvokeMenuItem(MenuButton menuButton, int idx)
	{
		if (menuButton.GetPopup().GetItemMetadata(idx) is string methodName)
		{
			object[] parameters = { };
			Invoke(methodName, parameters);
		}
	}

	private void OnProjectMenuItemSelected(int idx)
	{
		InvokeMenuItem(mbProject, idx);
	}

	private void OnEditMenuItemSelected(int idx)
	{
		InvokeMenuItem(mbEdit, idx);
	}

	private void OnProjectNew()
	{
		EmitSignal(nameof(NewProject));
	}

	private void OnProjectOpen()
	{
		EmitSignal(nameof(OpenProject));
	}

	private void OnProjectSave()
	{
		EmitSignal(nameof(SaveProject));
	}

	private void OnProjectSaveAs()
	{
		EmitSignal(nameof(SaveProjectAs));
	}

	private void OnProjectClose()
	{
		EmitSignal(nameof(CloseProject));
	}

	private void OnUndo()
	{
		EmitSignal(nameof(Undo));
	}

	private void OnRedo()
	{
		EmitSignal(nameof(Redo));
	}

	public void SetProjectOpened(bool projectOpened)
	{
		SetEnabled(mbProject, "Project::Save", projectOpened);
		SetEnabled(mbProject, "Project::SaveAs", projectOpened);
		SetEnabled(mbProject, "Project::Close", projectOpened);
	}

	public void SetUndoEnabled(bool enabled)
	{
		SetEnabled(mbEdit, "Edit::Undo", enabled);
	}

	public void SetRedoEnabled(bool enabled)
	{
		SetEnabled(mbEdit, "Edit::Redo", enabled);
	}
}
