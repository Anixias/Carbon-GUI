using Godot;
using System;

public class TextFieldEditor : FieldEditor
{
    private TextInputArea textInputArea;
    private Label label;
    private VSplitContainer splitContainer;
    private int dragInitialOffset = 0;
    private bool dragging = false;
    private int minSize;
    
    public new TextField Field
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
    
    protected new TextField field;
    
    public override void _Ready()
    {
        textInputArea = GetNode<TextInputArea>("VBoxContainer/TextInputArea");
        label = GetNode<Label>("VBoxContainer/Label");
        splitContainer = GetNode<VSplitContainer>("VBoxContainer/VSplitContainer");
        
        minSize = (int)RectMinSize.y;
        var rect = RectMinSize;
        rect.y += splitContainer.RectSize.y;
        RectMinSize = rect;
        
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
    
    public override void _Process(float delta)
    {
        if (!Input.IsMouseButtonPressed((int)ButtonList.Left))
        {
            dragging = false;
        }
    }
    
    protected override void FieldChanged()
    {
        UpdateState();
    }
    
    public override void UpdateState()
    {
        var editable = (!Inherited || Overriding);
        
        if (textInputArea != null)
        {
            var text = field?.Data ?? "";
            if (textInputArea.Text != text)
            {
                textInputArea.Text = text;
            }
            
            textInputArea.Readonly = !editable;
            textInputArea.SelectingEnabled = editable;
        }
        
        if (label != null)
        {
            label.Text = field?.Name ?? "";
            var color = overriding ? new Color(0.11f, 1.0f, 0.61f, 1.0f) : new Color(1.0f, 1.0f, 1.0f, 1.0f);
            label.Modulate = editable ? color : new Color(1.0f, 1.0f, 1.0f, 0.6f);
        }
    }
    
    public void OnTextInputFocusExited()
    {
        if (field != null)
        {
            field.Data = textInputArea.Text;
			EmitSignal(nameof(OnDataChanged), this);
        }
    }
    
    public void OnResized(int offset)
    {
        if (!dragging)
        {
            dragInitialOffset = (int)(GetGlobalMousePosition().y - splitContainer.RectGlobalPosition.y);
            dragging = true;
        }
        
        if (offset == 0) return;
        
        var splitHeight = (int)splitContainer.RectSize.y;
        var newHeight = Mathf.Clamp(((GetGlobalMousePosition().y - dragInitialOffset) - RectGlobalPosition.y), minSize, minSize * 3);
        
        RectMinSize = new Vector2(RectMinSize.x, (float)newHeight + splitHeight);
    }
}
