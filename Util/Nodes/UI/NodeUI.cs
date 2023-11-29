using GameEngine.Sys;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

public class NodeUI : Node
{

    //Position
    public Vector2<int> positionPixels = new(0,0);
    public Vector2<float> positionPercent = new(0,0);
    public Vector2<float> Position {
        get {
            Vector2<float> parentPos = new();
            if (parent != null && parent is NodeUI)
                parentPos = (parent as NodeUI)!.Position;

            return Size * positionPercent + parentPos + positionPixels;
        }
    }
    
    //Size
    public Vector2<int> sizePixels = new(0,0);
    public Vector2<float> sizePercent = new(1,1);
    public Vector2<float> Size {
        get {
            Vector2<float> parentSize = new();
            if (parent != null && parent is NodeUI)
                parentSize = (parent as NodeUI)!.Size;
            else
                parentSize = new Vector2<float>(Engine.window.Size.X, Engine.window.Size.Y);

            return parentSize * sizePercent + sizePixels;
        }
    }

    public bool clipChildren = false;

    public Rect getClippingArea()
    {
        return new Rect(Position, new Vector2<float>(100f, 100f));
    }
}