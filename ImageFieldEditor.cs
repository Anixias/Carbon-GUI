using Godot;

public class ImageFieldEditor : FieldEditor
{
	private TextureRect image;
	private TextInputBox textInputBox;
	private Label label;
	
	public new ImageField Field
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
	
	protected new ImageField field;
	
	public override void _Ready()
	{
		label = GetNode<Label>("VBoxContainer/Label");
		image = GetNode<TextureRect>("VBoxContainer/Image");
		textInputBox = GetNode<TextInputBox>("VBoxContainer/Path/TextInputBox");
		
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
		
		if (label != null)
		{
			label.Text = field?.Name ?? "";
			var color = overriding ? new Color(0.11f, 1.0f, 0.61f, 1.0f) : new Color(1.0f, 1.0f, 1.0f, 1.0f);
			label.Modulate = editable ? color : new Color(1.0f, 1.0f, 1.0f, 0.6f);
		}
	}
	
	public void OnBrowseButtonPressed()
	{
		
	}
}
