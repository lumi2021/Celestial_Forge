using GameEngine.Util.Attributes;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

public class Node2D : Node, ICanvasItem
{

    public bool Visible { get; set; } = true;
    public bool GlobalVisible => ((parent as ICanvasItem)?.GlobalVisible ?? true) && Visible;
    public int ZIndex { get; set; } = 0;
    public int GlobalZIndex => ((parent as ICanvasItem)?.GlobalZIndex ?? 0) + ZIndex;

    [Inspect] public Vector2<float> position = new(0,0);
    [Inspect] public Vector2<float> scale = new(0,0);


    public void Hide() => Visible = false;
    public void Show() => Visible = true;
}