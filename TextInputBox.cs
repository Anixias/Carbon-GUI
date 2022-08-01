using Godot;
using System;

public class TextInputBox : LineEdit
{
	public override void _Input(InputEvent @event)
    {
        if (!HasFocus()) return;
        
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
		
		// Override Ctrl+Y from paste to redo
		if (@event is InputEventKey keyEvent)
		{
			if (keyEvent.Pressed)
			{
				if (keyEvent.GetScancodeWithModifiers() == ((uint)KeyList.Y | (uint)KeyModifierMask.MaskCtrl))
				{
					AcceptEvent();
					MenuOption((int)LineEdit.MenuItems.Redo);
				}
			}
		}
    }
}
