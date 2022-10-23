using Godot;
using System;

public abstract class FieldInspector : ScrollContainer
{
	[Signal]
	public delegate void OnDataChanged(FieldEditor editor);

	public Field Field
	{
		get => field;
		set
		{
			field = value;
			FieldChanged();
		}
	}

	public bool Inherited
	{
		get => inherited;
		set
		{
			inherited = value;
			UpdateState();
		}
	}

	protected Field field;
	protected bool inherited;

	public override void _ExitTree()
	{
		if (field != null)
		{
			if (field.GetInspector() == this)
			{
				field.RemoveInspector();
			}
		}
	}

	public override void _Ready()
	{

	}

	public abstract void UpdateState();

	protected abstract void FieldChanged();
}
