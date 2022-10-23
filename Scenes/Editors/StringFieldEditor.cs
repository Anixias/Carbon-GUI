using Godot;

public class StringFieldEditor : FieldEditor
{
	private TextInputBox textInputBox;
	private OptionButton optionButton;
	private Label label;

	public new StringField Field
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

	protected new StringField field;

	public override void _Ready()
	{
		textInputBox = GetNode<TextInputBox>("VBoxContainer/TextInputBox");
		optionButton = GetNode<OptionButton>("VBoxContainer/OptionButton");
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

		if (textInputBox != null)
		{
			var text = field?.Data ?? "";
			if (textInputBox.Text != text)
			{
				textInputBox.Text = text;
			}

			textInputBox.Editable = editable;
			textInputBox.SelectingEnabled = editable;
			textInputBox.Visible = ((field?.Options.Count ?? 0) == 0);
		}

		if (optionButton != null)
		{
			optionButton.Clear();

			if (field != null)
			{
				foreach (var option in field.Options)
				{
					optionButton.AddItem(option);
				}

				optionButton.Disabled = !editable;
				optionButton.Visible = (field.Options.Count > 0);
			}
		}

		if (label != null)
		{
			label.Text = field?.Name ?? "";
			var color = overriding ? new Color(0.11f, 1.0f, 0.61f, 1.0f) : new Color(1.0f, 1.0f, 1.0f, 1.0f);
			label.Modulate = editable ? color : new Color(1.0f, 1.0f, 1.0f, 0.6f);
		}
	}

	public void OnTextInputEntered(string newText)
	{
		SubmitChanges();
	}

	public void OnTextFocusExited()
	{
		SubmitChanges();
	}

	public override void SubmitChanges()
	{
		if (field != null && field.Data != textInputBox.Text)
		{
			field.Data = textInputBox.Text;
			EmitSignal(nameof(OnDataChanged), this);
		}
	}

	public override bool HasChanges()
	{
		return (field?.Data != textInputBox.Text);
	}
}
