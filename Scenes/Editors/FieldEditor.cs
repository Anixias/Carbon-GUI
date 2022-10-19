using Godot;

public abstract class FieldEditor : Control
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

	public bool Overriding
	{
		get => overriding && inherited;
		set
		{
			overriding = value && inherited;
			UpdateState();
		}
	}

	protected Field field;
	protected bool inherited;
	protected bool overriding;

	public override void _ExitTree()
	{
		if (field != null)
		{
			if (field.GetEditor() == this)
			{
				field.RemoveEditor();
			}
		}
	}

	public override void _Ready()
	{

	}

	public abstract void UpdateState();

	protected abstract void FieldChanged();
}
