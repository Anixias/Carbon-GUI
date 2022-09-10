using Godot;
using System;

public class Interface : Control
{
	private abstract class ApplicationState
	{
		public ApplicationState Transition(ApplicationState target)
		{
			Exit();
			target?.Enter();
			return target;
		}
		
		public static ApplicationState Initialize(ApplicationState target)
		{
			target?.Enter();
			return target;
		}
		
		public abstract void Enter();
		public abstract void Exit();
	}
	
	private class SplashScreenState : ApplicationState
	{
		private Control splashScreen;
		
		public SplashScreenState(Control splashScreen)
		{
			this.splashScreen = splashScreen;
		}
		
		public override void Enter()
		{
			splashScreen.Visible = true;
		}
		
		public override void Exit()
		{
			splashScreen.Visible = false;
		}
	}
	
	private class ProjectEditorState : ApplicationState
	{
		public readonly Project project;
		public readonly ProjectEditor projectEditor;
		
		public ProjectEditorState(Project project, ProjectEditor projectEditor)
		{
			this.project = project;
			this.projectEditor = projectEditor;
		}
		
		public override void Enter()
		{
			projectEditor.SetProject(project);
			projectEditor.Visible = true;
		}
		
		public override void Exit()
		{
			projectEditor.Visible = false;
			projectEditor.SetProject(null);
		}
	}
	
	private ApplicationState state;
	private CenterContainer splashScreen;
	private ProjectEditor projectEditor;
	private MenuBar menuBar;
	
	public override void _Ready()
	{
		OS.WindowMaximized = true;
		
		splashScreen = GetNode<CenterContainer>("MarginContainer/VBoxContainer/SplashScreen");
		projectEditor = GetNode<ProjectEditor>("MarginContainer/VBoxContainer/ProjectEditor");
		menuBar = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/MenuBar") as MenuBar;
		
		menuBar.Connect(nameof(MenuBar.Undo), projectEditor, nameof(ProjectEditor.Undo));
		menuBar.Connect(nameof(MenuBar.Redo), projectEditor, nameof(ProjectEditor.Redo));
		projectEditor.Connect(nameof(ProjectEditor.UpdateProjectStatus), menuBar, nameof(MenuBar.SetProjectOpened));
		projectEditor.Connect(nameof(ProjectEditor.UpdateUndo), menuBar, nameof(MenuBar.SetUndoEnabled));
		projectEditor.Connect(nameof(ProjectEditor.UpdateRedo), menuBar, nameof(MenuBar.SetRedoEnabled));
		
		state = ApplicationState.Initialize(new SplashScreenState(splashScreen));
	}

	public override void _Process(float delta)
	{
		var title = "Carbon GUI";
		
		if (state is ProjectEditorState projectEditorState)
		{
			title = projectEditorState.project.path.GetFile() + (projectEditorState.projectEditor.HasUnsavedChanges ? "*" : "") + " - " + title;
		}
		
		OS.SetWindowTitle(title);
	}

	public override void _Notification(int notification)
	{
		switch(notification)
		{
			default:
				break;
			case NotificationWmMouseExit:
				Input.ParseInputEvent(new InputEventMouseMotion());
				break;
		}
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mbEvent)
		{
			if (mbEvent.Pressed)
			{
				GetFocusOwner()?.ReleaseFocus();
			}
		}
		
		if (@event is InputEventKey keyEvent)
		{
			if (keyEvent.GetScancodeWithModifiers() == (uint)KeyList.Escape)
			{
				GetFocusOwner()?.ReleaseFocus();
			}
		}
	}
	
	public void OnNewProject()
	{
		var project = new Project();
		if (project.SaveAs())
		{
			state = state.Transition(new ProjectEditorState(project, projectEditor));
		}
	}
	
	public void OnOpenProject()
	{
		var project = new Project();
		if (project.Open())
		{
			state = state.Transition(new ProjectEditorState(project, projectEditor));
		}
	}
	
	public void OnCloseProject()
	{
		state = state.Transition(new SplashScreenState(splashScreen));
	}
	
	public void OnSaveProject()
	{
		if (state is ProjectEditorState projectEditorState)
		{
			projectEditorState.project.Save();
			projectEditorState.projectEditor.ProjectSaved();
		}
	}
	
	public void OnSaveProjectAs()
	{
		if (state is ProjectEditorState projectEditorState)
		{
			projectEditorState.project.SaveAs();
			projectEditorState.projectEditor.ProjectSaved();
		}
	}
}
