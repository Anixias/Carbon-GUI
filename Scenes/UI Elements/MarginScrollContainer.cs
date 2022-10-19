using Godot;
using System;

[Tool]
public class MarginScrollContainer : ScrollContainer
{
    public Action CustomDraw = null;
    
    public TreeList ContainingTreeList
    {
        get => containingTreeList;
        set
        {
            containingTreeList = value;
            
            if (relationshipLines != null)
            {
                relationshipLines.ContainingTreeList = value;
            }
        }
    }
    
    private TreeList containingTreeList = null;
    private RelationshipLines relationshipLines;
    private VScrollBar vScrollBar;
    private HScrollBar hScrollBar;
    private MarginContainer marginContainer;
    private Panel inputBlocker;
    
    private float scrollHorizontalAcc = 0.0f;
    private float scrollVerticalAcc = 0.0f;
    
    public override void _Ready()
    {
        vScrollBar = GetVScrollbar();
        hScrollBar = GetHScrollbar();
        marginContainer = GetNode<MarginContainer>("MarginContainer");
        inputBlocker = GetNode<Panel>("MarginContainer/Control/InputBlocker");
        relationshipLines = GetNode<Control>("MarginContainer/RelationshipLines") as RelationshipLines;
        
        vScrollBar.Connect("visibility_changed", this, nameof(OnVScrollBarVisibilityChanged));
        hScrollBar.Connect("visibility_changed", this, nameof(OnHScrollBarVisibilityChanged));
    }
    
    public override void _Process(float delta)
    {
        if (Engine.EditorHint) return;
        
        var maxX = Math.Max(0, marginContainer.RectSize.x - RectSize.x) + vScrollBar.RectSize.x;
        var maxY = Math.Max(0, marginContainer.RectSize.y - RectSize.y) + hScrollBar.RectSize.y;
        
        inputBlocker.RectPosition = new Vector2(-(maxX - ScrollHorizontal), -(maxY - ScrollVertical));
        inputBlocker.Visible = hScrollBar.Visible && vScrollBar.Visible;
        
        while (Math.Abs(scrollVerticalAcc) >= 1.0f)
        {
            if (scrollVerticalAcc < 0.0f)
            {
                scrollVerticalAcc += 1.0f;
                ScrollVertical -= 1;
            }
            else if (scrollVerticalAcc > 0.0f)
            {
                scrollVerticalAcc -= 1.0f;
                ScrollVertical += 1;
            }
        }
        
        while (Math.Abs(scrollHorizontalAcc) >= 1.0f)
        {
            if (scrollHorizontalAcc < 0.0f)
            {
                scrollHorizontalAcc += 1.0f;
                ScrollHorizontal -= 1;
            }
            else if (scrollHorizontalAcc > 0.0f)
            {
                scrollHorizontalAcc -= 1.0f;
                ScrollHorizontal += 1;
            }
        }
    }
    
    public void Refresh()
    {
        relationshipLines.Update();
    }
    
    public void DragScroll(Vector2 position)
    {
        var scrollBorder = 28.0f * 1.5f;
        var scrollSpeed = 1.0f * 1.5f;
        
        if (position.y <= scrollBorder)
        {
            var diff = Math.Abs(position.y - scrollBorder);
            scrollVerticalAcc -= scrollSpeed * (diff / scrollBorder);
        }
        else if (position.y >= RectSize.y - scrollBorder)
        {
            var diff = Math.Abs(position.y - (RectSize.y - scrollBorder));
            scrollVerticalAcc += scrollSpeed * (diff / scrollBorder);
        }
        
        if (position.x <= scrollBorder)
        {
            var diff = Math.Abs(position.x - scrollBorder);
            scrollHorizontalAcc -= scrollSpeed * (diff / scrollBorder);
        }
        else if (position.x >= RectSize.x - scrollBorder)
        {
            var diff = Math.Abs(position.x - (RectSize.x - scrollBorder));
            scrollHorizontalAcc += scrollSpeed * (diff / scrollBorder);
        }
    }
    
    /*public override bool CanDropData(Vector2 position, object data)
    {
        DragScroll(position);
        
        return false;
    }*/
    
    public void OnVScrollBarVisibilityChanged()
    {
        if (vScrollBar.Visible)
        {
            var rect = inputBlocker.RectMinSize;
            rect.x = vScrollBar.RectSize.x + 1.0f;
            inputBlocker.RectMinSize = rect;
        }
        else
        {
            var rect = inputBlocker.RectMinSize;
            rect.x = 0.0f;
            inputBlocker.RectMinSize = rect;
        }
    }
    
    public void OnHScrollBarVisibilityChanged()
    {
        if (hScrollBar.Visible)
        {
            var rect = inputBlocker.RectMinSize;
            rect.y = hScrollBar.RectSize.y + 1.0f;
            inputBlocker.RectMinSize = rect;
        }
        else
        {
            var rect = inputBlocker.RectMinSize;
            rect.y = 0.0f;
            inputBlocker.RectMinSize = rect;
        }
    }
}
