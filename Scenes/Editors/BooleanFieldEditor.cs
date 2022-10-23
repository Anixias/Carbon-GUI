using Godot;

public class BooleanFieldEditor : FieldEditor
{
	private VBoxContainer vBoxContainer;
	private CheckButton checkButton;
	private Label label;

	public new BooleanField Field
	{
		get => field;
		set
		{
			field = value;
			FieldChanged();
		}
	}

	public new bool Inherited
	{
		get => inherited;
		set
		{
			inherited = value;
			UpdateState();
		}
	}

	public new bool Overriding
	{
		get => overriding && inherited;
		set
		{
			overriding = value && inherited;
			UpdateState();
		}
	}

	protected new BooleanField field;

	public override void _Ready()
	{
		vBoxContainer = GetNode<VBoxContainer>("VBoxContainer");
		checkButton = GetNode<CheckButton>("VBoxContainer/CheckButton");
		label = GetNode<Label>("VBoxContainer/Label");

		UpdateState();
	}

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

	protected override void FieldChanged()
	{
		UpdateState();
	}

	public override void UpdateState()
	{
		var editable = (!Inherited || Overriding);

		if (checkButton != null)
		{
			var value = field?.Data ?? false;
			if (checkButton.Pressed != value)
			{
				checkButton.Pressed = value;
			}

			checkButton.Disabled = !editable;
			checkButton.MouseDefaultCursorShape = (editable ? CursorShape.PointingHand : CursorShape.Arrow);
		}

		if (label != null)
		{
			label.Text = field?.Name ?? "";
			var color = overriding ? new Color(0.11f, 1.0f, 0.61f, 1.0f) : new Color(1.0f, 1.0f, 1.0f, 1.0f);
			label.Modulate = editable ? color : new Color(1.0f, 1.0f, 1.0f, 0.6f);
		}
	}

	public void OnToggled(bool check)
	{
		SubmitChanges();
	}

	public override void SubmitChanges()
	{
		if (field != null)
		{
			field.Data = checkButton.Pressed;
			EmitSignal(nameof(OnDataChanged), this);
		}
	}

	public override bool HasChanges()
	{
		return (field?.Data != checkButton.Pressed);
	}
}
