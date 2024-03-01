using GameEngine.Util.Attributes;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

public class Node2D : Node, ICanvasItem
{

    public bool Visible { get; set ; } = true;

    [Inspect] public Vector2<float> position = new(0,0);
    [Inspect] public Vector2<float> scale = new(0,0);


    public void Hide() => Visible = false;
    public void Show() => Visible = true;
}