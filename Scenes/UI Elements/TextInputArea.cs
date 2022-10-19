using Godot;

public class TextInputArea : TextEdit
{
	private VScrollBar vScrollBar;

	public override void _Ready()
	{
		foreach (var child in GetChildren())
		{
			if (child is VScrollBar scrollBar)
			{
				vScrollBar = scrollBar;
				break;
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (!HasFocus())
			return;

		if (@event is InputEventMouseButton mbEvent)
		{
			if (mbEvent.Pressed)
			{
				if (!GetGlobalRect().HasPoint(GetGlobalMousePosition()))
				{
					ReleaseFocus();
					Deselect();
				}
			}
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mbEvent)
		{
			if (mbEvent.Pressed)
			{
				if (vScrollBar != null && vScrollBar.Visible)
				{
					if (mbEvent.ButtonIndex == (int)ButtonList.WheelUp)
					{
						if (ScrollVertical > 0)
						{
							AcceptEvent();
						}
					}
					else if (mbEvent.ButtonIndex == (int)ButtonList.WheelDown)
					{
						if (vScrollBar.Value < (vScrollBar.MaxValue - vScrollBar.Page - 0.001f))
						{
							AcceptEvent();
						}
					}
				}
			}
		}
	}
}
