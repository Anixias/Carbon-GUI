using Godot;
using NativeServices;

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
		image = GetNode<TextureRect>("%Image");
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
		
		if (textInputBox != null)
		{
			textInputBox.Text = field?.Data ?? "";
		}
		
		if (image != null)
		{
			if (field?.Image != null)
			{
				var imgTex = new ImageTexture();
				imgTex.CreateFromImage(field?.Image, 0);
				image.Texture = imgTex;
			}
			else
			{
				image.Texture = null;
			}
		}
		
		if (label != null)
		{
			label.Text = field?.Name ?? "";
			var color = overriding ? new Color(0.11f, 1.0f, 0.61f, 1.0f) : new Color(1.0f, 1.0f, 1.0f, 1.0f);
			label.Modulate = editable ? color : new Color(1.0f, 1.0f, 1.0f, 0.6f);
		}
	}
	
	public void OnBrowseButtonPressed()
	{
		if (field == null) return;
		
		var paths = NativeFileDialog.OpenFileDialog("Load Image", Project.defaultPath, new[] { "*.png" }, "Image Files", false);
		if (paths == null || paths.Length == 0) return;
		
		var path = paths[0];
		if (path == "" || path == null) return;
		
		field.Data = path;
		EmitSignal(nameof(OnDataChanged), this);
	}
	
	public void OnTextInputEntered(string newText)
	{
		if (field != null)
		{
			field.Data = newText;
			EmitSignal(nameof(OnDataChanged), this);
		}
	}
	
	public void OnTextFocusExited()
	{
		if (field != null)
		{
			field.Data = textInputBox.Text;
			EmitSignal(nameof(OnDataChanged), this);
		}
	}
}
