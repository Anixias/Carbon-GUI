using Godot;
using System;

public class ImageFieldEditor : FieldEditor
{
	private TextureRect image;
	private TextInputBox textInputBox;
	
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
	}
	
	public void OnBrowseButtonPressed()
	{
		
	}
}
