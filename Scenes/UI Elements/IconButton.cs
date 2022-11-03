using Godot;

[Tool]
public class IconButton : Control
{
	[Signal]
	public delegate void Pressed();

	[Export]
	public string Text
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
		}
	}

	[Export]
	public Texture Icon
	{
		get => icon;
		set
		{
			icon = value;

			Refresh();
		}
	}

	[Export]
	public bool KeepPressedOutside
	{
		get => keepPressedOutside;
		set
		{
			keepPressedOutside = value;

			Refresh();

			if (button != null)
			{
				button.KeepPressedOutside = value;
			}
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
		get => enabled;
		set
		{
			enabled = value;

			Refresh();

			if (button != null)
			{
				button.Disabled = !value;
			}
		}
	}

	private string text = "";
	private Texture icon = null;
	private bool keepPressedOutside = false;
	private bool tintIcon = false;
	private bool enabled = true;

	private Button button;
	private Label textLabel;
	private TextureRect iconTextureRect;
	private MarginContainer marginContainer;

	private void OnPressed()
	{
		EmitSignal(nameof(Pressed));
	}

	private void Refresh()
	{
		if (IsInsideTree())
		{
			button = GetNode<Button>("Button");
			textLabel = GetNode<Label>("MarginContainer/HBoxContainer/Label");
			iconTextureRect = GetNode<TextureRect>("MarginContainer/HBoxContainer/TextureRect");
			marginContainer = GetNode<MarginContainer>("MarginContainer");

			if (textLabel != null)
			{
				textLabel.Text = text;
			}

			if (iconTextureRect != null)
			{
				iconTextureRect.Texture = icon;
				iconTextureRect.RectMinSize = (icon == null) ? Vector2.Zero : icon.GetSize();
			}
		}
	}

	public override void _Ready()
	{
		Refresh();

		if (button == null)
		{
			return;
		}

		textLabel.Text = text;
		iconTextureRect.Texture = icon;
	}

	public override void _Process(float delta)
	{
		Refresh();

		if (button == null)
		{
			return;
		}

		button.Disabled = !enabled;
		button.MouseDefaultCursorShape = (enabled ? CursorShape.PointingHand : CursorShape.Arrow);
		button.HintTooltip = HintTooltip;

		var color = Theme.GetColor("font_color", "Button");
		var stylebox = Theme.GetStylebox("normal", "Button");

		if (button.Disabled)
		{
			color = Theme.GetColor("font_color_disabled", "Button");
			stylebox = Theme.GetStylebox("disabled", "Button");
		}
		else
		{
			if (button.IsHovered())
			{
				color = Theme.GetColor("font_color_hover", "Button");
				stylebox = Theme.GetStylebox("hover", "Button");
			}

			if (button.Pressed)
			{
				if (button.GetGlobalRect().HasPoint(GetGlobalMousePosition()) || keepPressedOutside)
				{
					color = Theme.GetColor("font_color_pressed", "Button");
					stylebox = Theme.GetStylebox("pressed", "Button");

				}
				else
				{
					color = Theme.GetColor("font_color", "Button");
					stylebox = Theme.GetStylebox("normal", "Button");
				}
			}
		}

		textLabel.Modulate = color;

		if (tintIcon)
		{
			iconTextureRect.Modulate = color;
		}

		marginContainer.AddConstantOverride("margin_left", (int)stylebox.GetMargin(Margin.Left));
		marginContainer.AddConstantOverride("margin_right", (int)stylebox.GetMargin(Margin.Right));
		marginContainer.AddConstantOverride("margin_top", (int)stylebox.GetMargin(Margin.Top));
		marginContainer.AddConstantOverride("margin_bottom", (int)stylebox.GetMargin(Margin.Bottom));

		if (Theme.HasFont("font", "Button"))
		{
			textLabel.AddFontOverride("font", Theme.GetFont("font", "Button"));
		}
		else
		{
			textLabel.AddFontOverride("font", Theme.DefaultFont);
		}
	}
}
