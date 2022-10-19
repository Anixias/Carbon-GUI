using Godot;

[Tool]
public class ListItem : Button
{
	[Signal]
	public delegate void ButtonPressed();

	[Signal]
	public delegate void DoubleClicked(int listIndex);

	[Signal]
	public delegate void Renamed(int listIndex, string newName);

	[Signal]
	public delegate void TextInputChanged(int listIndex, string newName);

	[Signal]
	public delegate void Deleted(int listIndex);

	[Export]
	public new string Text
	{
		get => text;
		set
		{
			text = value;

			Refresh();

			if (textLabel != null)
			{
				textLabel.Text = value;
			}

			if (textInputBox != null)
			{
				textInputBox.Text = value;
			}

			HintTooltip = value;
		}
	}

	[Export]
	public new Texture Icon
	{
		get => icon;
		set
		{
			icon = value;

			Refresh();

			if (iconTextureRect != null)
			{
				iconTextureRect.Texture = value;
			}
		}
	}

	[Export]
	public new bool KeepPressedOutside
	{
		get => base.KeepPressedOutside;
		set
		{
			base.KeepPressedOutside = value;
		}
	}

	[Export]
	public bool TintIcon
	{
		get => tintIcon;
		set
		{
			tintIcon = value;
		}
	}

	[Export]
	public bool Enabled
	{
		get => !Disabled;
		set
		{
			Disabled = !value;
		}
	}

	[Export]
	public bool Selected
	{
		get
		{
			if (selectPanel != null)
			{
				return selectPanel.Visible;
			}

			return false;
		}
		set
		{
			if (selectPanel != null)
			{
				selectPanel.Visible = value;
			}
		}
	}

	[Export]
	public bool CanBeRenamed
	{
		get => canBeRenamed;
		set
		{
			if (popupMenu != null)
			{
				popupMenu.SetItemDisabled(0, !value);
			}

			canBeRenamed = value;
		}
	}

	[Export]
	public bool CanBeDeleted
	{
		get => canBeDeleted;
		set
		{
			if (popupMenu != null)
			{
				popupMenu.SetItemDisabled(1, !value);
			}

			canBeDeleted = value;
		}
	}

	public bool Inherited
	{
		get => inherited;
		set
		{
			inherited = value;
			if (!inherited)
				overriding = false;

			UpdateGraphics();
		}
	}

	public bool Overriding
	{
		get => overriding;
		set
		{
			overriding = value && inherited;

			UpdateGraphics();
		}
	}

	public bool Dragging
	{
		get => dragging;
		set
		{
			dragging = value;
			dragPanel.Visible = value;
		}
	}

	public int ListIndex = -1;

	private string text = "";
	private Texture icon = null;
	private bool tintIcon = false;
	private bool canBeRenamed = true;
	private bool canBeDeleted = true;
	private bool inherited = false;
	private bool overriding = false;
	private bool queueEdit = false;
	private bool dragging = false;
	private bool ensureVisible = false;

	private Label textLabel;
	private TextureRect iconTextureRect;
	private MarginContainer marginContainer;
	private HBoxContainer hBoxContainer;
	private Control textControl;
	private TextInputBox textInputBox;
	private Panel selectPanel;
	private Panel dragPanel;
	private PopupMenu popupMenu;

	private void Refresh()
	{
		if (IsInsideTree())
		{
			textLabel = GetNode<Label>("MarginContainer/HBoxContainer/Text/Label");
			iconTextureRect = GetNode<TextureRect>("MarginContainer/HBoxContainer/TextureRect");
			marginContainer = GetNode<MarginContainer>("MarginContainer");
			hBoxContainer = GetNode<HBoxContainer>("MarginContainer/HBoxContainer");
			textInputBox = GetNode<LineEdit>("MarginContainer/HBoxContainer/Text/TextInputBox") as TextInputBox;
			textControl = GetNode<Control>("MarginContainer/HBoxContainer/Text");

			selectPanel = GetNode<Panel>("SelectPanel");
			dragPanel = GetNode<Panel>("DragPanel");
			popupMenu = GetNode<PopupMenu>("PopupMenu");
		}
	}

	public override void _Ready()
	{
		Refresh();

		if (marginContainer == null)
		{
			return;
		}

		textLabel.Text = text;
		iconTextureRect.Texture = icon;

		if (textInputBox != null)
		{
			textInputBox.Text = text;
			//textInputBox.GrabFocus();
		}

		UpdateGraphics();

		/*if (!Engine.EditorHint)
		{
			// Rename: Shortcut
			var sc = new ShortCut();
			sc.Shortcut = new InputEventKey();
			
			var SC = ((InputEventKey)sc.Shortcut);
			SC.Command = false;
			SC.Shift = false;
			SC.Alt = false;
			SC.Scancode = (uint)KeyList.F2;
			
			popupMenu.SetItemShortcut(0, sc);
			
			// Delete: Shortcut
			sc = new ShortCut();
			sc.Shortcut = new InputEventKey();
			
			SC = ((InputEventKey)sc.Shortcut);
			SC.Command = false;
			SC.Shift = false;
			SC.Alt = false;
			SC.Scancode = (uint)KeyList.Delete;
			
			popupMenu.SetItemShortcut(1, sc);
		}*/
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (Engine.EditorHint)
			return;

		Refresh();

		if (marginContainer == null)
		{
			return;
		}

		if (@event is InputEventMouseButton mbEvent)
		{
			if (mbEvent.Pressed && mbEvent.ButtonIndex == (int)ButtonList.Right)
			{
				popupMenu.SetGlobalPosition(GetGlobalMousePosition());
				popupMenu.Popup_();
				EnsureVisible();
			}

			if (mbEvent.ButtonIndex == (int)ButtonList.Left)
			{
				if (mbEvent.Doubleclick)
				{
					EmitSignal(nameof(DoubleClicked), ListIndex);
					EnsureVisible();
				}
				else if (mbEvent.Pressed)
				{
					EmitSignal(nameof(ButtonPressed));
					EnsureVisible();
				}
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (Engine.EditorHint)
			return;

		Refresh();

		if (marginContainer == null)
		{
			return;
		}

		if (@event is InputEventKey keyEvent)
		{
			if (keyEvent.GetScancodeWithModifiers() == (uint)KeyList.F2)
			{
				if (HasFocus() && CanBeRenamed)
				{
					AcceptEvent();
					Edit();
				}
			}
			else if (keyEvent.GetScancodeWithModifiers() == (uint)KeyList.Escape)
			{
				if (textInputBox.HasFocus())
				{
					AcceptEvent();
					textInputBox.Text = text;
					textInputBox.Visible = false;
					textLabel.Visible = true;
				}

				if (dragging)
				{
					ForceEndDrag();
				}
			}
		}

		UpdateGraphics();
	}

	public override void _Process(float delta)
	{
		if (Engine.EditorHint)
			return;

		// Refresh min sizes
		RectMinSize = Vector2.Zero;

		if (textControl != null)
		{
			textControl.RectMinSize = new Vector2(128.0f, 24.0f);//Vector2.Zero;

			/*if (textLabel != null)
			{
				var rect = textLabel.GetFont("font").GetStringSize(textInputBox.Text);
				var stylebox = textLabel.GetStylebox("normal");
				
				rect.x += Mathf.Max(0.0f, stylebox.ContentMarginLeft) + Mathf.Max(0.0f, stylebox.ContentMarginRight);
				rect.y += Mathf.Max(0.0f, stylebox.ContentMarginTop) + Mathf.Max(0.0f, stylebox.ContentMarginBottom);
				
				textControl.RectMinSize = rect;
			}*/

			marginContainer.RectSize = Vector2.Zero;
		}

		if (Visible)
		{
			if (marginContainer != null)
			{
				RectMinSize = marginContainer.RectSize;
			}
		}

		if (ensureVisible)
		{
			ensureVisible = false;

			EnsureVisible();
		}

		if (queueEdit)
		{
			queueEdit = false;

			textInputBox.Visible = true;
			textLabel.Visible = false;

			ensureVisible = true;

			textInputBox.GrabFocus();
			textInputBox.SelectAll();
		}
	}

	private void ForceEndDrag()
	{
		var evRelease = new InputEventMouseButton();
		evRelease.Pressed = false;
		evRelease.ButtonIndex = (int)ButtonList.Left;

		var evMotion = new InputEventMouseMotion();

		Input.ParseInputEvent(evRelease);
		Input.ParseInputEvent(evMotion);
	}

	private void UpdateGraphics()
	{
		MouseDefaultCursorShape = ((!Disabled) ? CursorShape.PointingHand : CursorShape.Arrow);

		var color = GetColor("font_color", "Button");
		var stylebox = GetStylebox("normal", "Button");

		if (Disabled)
		{
			color = GetColor("font_color_disabled", "Button");
			stylebox = GetStylebox("disabled", "Button");
		}
		else
		{
			if (IsHovered())
			{
				color = GetColor("font_color_hover", "Button");
				stylebox = GetStylebox("hover", "Button");
			}

			if (base.Pressed)
			{
				if (GetGlobalRect().HasPoint(GetGlobalMousePosition()) || KeepPressedOutside)
				{
					color = GetColor("font_color_pressed", "Button");
					stylebox = GetStylebox("pressed", "Button");

				}
				else
				{
					color = GetColor("font_color", "Button");
					stylebox = GetStylebox("normal", "Button");
				}
			}
		}

		if (inherited)
		{
			color = GetColor("font_color", "Label");
			color.a *= 0.4f;
		}

		if (overriding)
		{
			color = new Color(0.11f, 1.0f, 0.61f, 1.0f);
		}

		if (textLabel != null)
		{
			textLabel.Modulate = color;
		}

		if (tintIcon && iconTextureRect != null)
		{
			iconTextureRect.Modulate = color;
		}

		marginContainer.AddConstantOverride("margin_left", (int)stylebox.GetMargin(Margin.Left));
		marginContainer.AddConstantOverride("margin_right", (int)stylebox.GetMargin(Margin.Right));
		marginContainer.AddConstantOverride("margin_top", (int)stylebox.GetMargin(Margin.Top));
		marginContainer.AddConstantOverride("margin_bottom", (int)stylebox.GetMargin(Margin.Bottom));

		if (HasFont("font", "Button"))
		{
			textLabel?.AddFontOverride("font", GetFont("font", "Button"));
		}
		else
		{
			textLabel?.AddFontOverride("font", null);
		}
	}

	public void EnsureVisible()
	{
		Node current = this;
		MarginScrollContainer scrollContainer = null;

		while (scrollContainer == null)
		{
			current = current.GetParent();
			if (current == null)
				break;

			if (current is MarginScrollContainer SC)
			{
				scrollContainer = SC;
			}
		}

		if (scrollContainer != null)
		{
			scrollContainer.EnsureControlVisible(marginContainer);
		}
	}

	public void DragScroll(Vector2 position)
	{
		Node current = this;
		MarginScrollContainer scrollContainer = null;

		while (scrollContainer == null)
		{
			current = current.GetParent();
			if (current == null)
				break;

			if (current is MarginScrollContainer SC)
			{
				scrollContainer = SC;
			}
		}

		if (scrollContainer != null)
		{
			position += RectGlobalPosition;
			position -= scrollContainer.RectGlobalPosition;

			scrollContainer.DragScroll(position);
		}
	}

	public override object GetDragData(Vector2 position)
	{
		SetDragPreview(MakeDragPreview());

		return this;
	}

	public override bool CanDropData(Vector2 position, object data)
	{
		DragScroll(position);

		return false;
	}

	public override void DropData(Vector2 position, object data)
	{

	}

	public void OnTextEntered(string newText)
	{
		textInputBox.Visible = false;
		textLabel.Visible = true;

		if (newText != "")
		{
			var oText = text;
			Text = textInputBox.Text;

			if (oText != text)
			{
				EmitSignal(nameof(Renamed), ListIndex, text);
			}
		}
		else
		{
			textInputBox.Text = text;
		}
	}

	public void OnTextInputChanged(string newText)
	{
		EmitSignal(nameof(TextInputChanged), ListIndex, newText);
	}

	public void OnTextFocusExited()
	{
		textInputBox.Visible = false;
		textLabel.Visible = true;

		if (textInputBox.Text != "")
		{
			var oText = text;
			Text = textInputBox.Text;

			if (oText != text)
			{
				EmitSignal(nameof(Renamed), ListIndex, text);
			}
		}
		else
		{
			textInputBox.Text = text;
		}

		if (GetFocusOwner() == null)
		{
			GrabFocus();
		}
	}

	public void OnPopupMenuIndexPressed(int index)
	{
		switch (index)
		{
			default:
				break;
			case 0:
				Edit();
				break;
			case 1:
				EmitSignal(nameof(Deleted), ListIndex);
				break;
		}
	}

	public void Edit()
	{
		queueEdit = true;
	}

	public void Select()
	{
		selectPanel.Visible = true;
	}

	public void Deselect()
	{
		selectPanel.Visible = false;
	}

	private Control MakeDragPreview()
	{
		ListItem preview = Duplicate((int)DuplicateFlags.UseInstancing) as ListItem;

		return preview;
	}
}
