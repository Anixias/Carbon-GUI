using Godot;

public class NumberInputBox : SpinBox
{
	[Signal]
	public delegate void ValueEdited(double value);

	[Export]
	public bool CaretBlink
	{
		get => GetLineEdit().CaretBlink;
		set => GetLineEdit().CaretBlink = value;
	}

	[Export]
	public float CaretBlinkSpeed
	{
		get => GetLineEdit().CaretBlinkSpeed;
		set => GetLineEdit().CaretBlinkSpeed = value;
	}

	public override void _Ready()
	{
		var lineEdit = GetLineEdit();

		lineEdit.Connect("text_entered", this, nameof(OnTextEntered));
		lineEdit.Connect("focus_exited", this, nameof(OnTextFocusExited));
	}

	public override void _Input(InputEvent @event)
	{
		var lineEdit = GetLineEdit();

		if (@event is InputEventMouseButton mbEvent)
		{
			if (mbEvent.Pressed)
			{
				if (!GetGlobalRect().HasPoint(GetGlobalMousePosition()))
				{
					lineEdit.ReleaseFocus();
					lineEdit.Deselect();

					if (lineEdit.Text == "")
					{
						ResetValue();
					}
				}
			}
		}

		// Override Ctrl+Y from paste to redo
		if (@event is InputEventKey keyEvent)
		{
			if (lineEdit.HasFocus())
			{
				if (keyEvent.Pressed)
				{
					if (keyEvent.GetScancodeWithModifiers() == ((uint)KeyList.Y | (uint)KeyModifierMask.MaskCtrl))
					{
						AcceptEvent();
						lineEdit.MenuOption((int)LineEdit.MenuItems.Redo);
					}
				}
			}
		}
	}

	private void ResetValue()
	{
		var lineEdit = GetLineEdit();
		lineEdit.Text = Value.ToString();
	}

	private void EnterValue()
	{
		if (GetLineEdit().Text == "")
		{
			ResetValue();
		}
		else
		{
			Apply();
			EmitSignal(nameof(ValueEdited), Value);
		}
	}

	public void OnTextEntered(string newText)
	{
		EnterValue();
	}

	public void OnTextFocusExited()
	{
		EnterValue();
	}
}
