using Godot;
using System;

public class NumberFieldEditor : FieldEditor
{
	private NumberInputBox numberInputBox;
	private Label label;
	
	public new NumberField Field
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
	
	protected new NumberField field;
	
	public override void _Ready()
	{
		numberInputBox = GetNode<NumberInputBox>("VBoxContainer/NumberInputBox");
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
		
		if (numberInputBox != null)
		{
			var value = field?.Data ?? 0d;
			
			if (numberInputBox.Value != value)
			{
				numberInputBox.Value = value;
			}
			
			numberInputBox.Editable = editable;
			numberInputBox.GetLineEdit().SelectingEnabled = editable;
		}
		
		if (label != null)
		{
			label.Text = field?.Name ?? "";
			var color = overriding ? new Color(0.11f, 1.0f, 0.61f, 1.0f) : new Color(1.0f, 1.0f, 1.0f, 1.0f);
			label.Modulate = editable ? color : new Color(1.0f, 1.0f, 1.0f, 0.6f);
		}
	}
	
	public void OnValueEdited(double value)
	{
		if (field != null)
		{
			field.Data = value;
			EmitSignal(nameof(OnDataChanged), this);
		}
	}
}
