using Godot;
using System;
using System.Collections.Generic;

public class RelationshipLines : Control
{
    public TreeList ContainingTreeList = null;
    
    public override void _Draw()
    {
        if (ContainingTreeList == null) return;
        
        var color = new Color(0.22f, 0.27f, 0.3f);
        var points = new List<Vector2>();
        
        foreach(var item in ContainingTreeList.ListItems)
        {
            if (item.RelationshipPoints.Count >= 2)
            {
                var localPos = item.RectGlobalPosition - RectGlobalPosition;
                for(var p = 0; p < item.RelationshipPoints.Count; p++)
                {
                    points.Add(item.RelationshipPoints[p] + localPos);
                }
            }
        }
        
        if (points.Count >= 2)
        {
            DrawMultiline(points.ToArray(), color);
        }
    }
}
