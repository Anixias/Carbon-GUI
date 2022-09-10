using System;

public abstract class EditorCommand
{
	protected readonly Guid guid;
	protected ProjectEditor editor;
	
	public EditorCommand(ProjectEditor editor)
	{
		guid = Guid.NewGuid();
		this.editor = editor;
	}
	
	public abstract void Execute();
	public abstract void Undo();
	
	public override string ToString()
	{
		return "<Command>";
	}
}

public interface ITestable
{
	public bool Test();
}