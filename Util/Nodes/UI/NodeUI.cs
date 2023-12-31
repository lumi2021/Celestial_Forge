using GameEngine.Sys;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

public class NodeUI : Node, IClipChildren
{

    public enum ANCHOR {
        TOP_LEFT,
        TOP_CENTER,
        TOP_RIGHT,

        CENTER_LEFT,
        CENTER_CENTER,
        CENTER_RIGHT,

        BOTTOM_LEFT,
        BOTTOM_CENTER,
        BOTTOM_RIGHT,
    }
    public ANCHOR anchor = ANCHOR.TOP_LEFT;

    //Get parent size
    private Vector2<float> ParentSize {
        get {
            Vector2<float> parentSize;
            
            if (parent != null && parent is NodeUI)
                parentSize = (parent as NodeUI)!.Size;
            else
                parentSize = new Vector2<float>(Engine.window.Size.X, Engine.window.Size.Y+1);

            return parentSize;
        }
    }

    //Position
    public Vector2<int> positionPixels = new(0,0);
    public Vector2<float> positionPercent = new(0,0);
    public Vector2<float> Position {
        get {
            Vector2<float> parentPos = new(0, -1);

            if (parent != null && parent is NodeUI)
                parentPos = (parent as NodeUI)!.Position;

            Vector2<float> finalPosition = new();

            // anchor X
            switch (anchor)
            {
                case ANCHOR.TOP_LEFT:
                case ANCHOR.CENTER_LEFT:
                case ANCHOR.BOTTOM_LEFT:
                    break;

                case ANCHOR.TOP_CENTER:
                case ANCHOR.CENTER_CENTER:
                case ANCHOR.BOTTOM_CENTER:
                    finalPosition.X = (ParentSize.X/2) - (Size.X/2);
                    break;
                
                case ANCHOR.TOP_RIGHT:
                case ANCHOR.CENTER_RIGHT:
                case ANCHOR.BOTTOM_RIGHT:
                    finalPosition.X = ParentSize.X - Size.X;
                    break;
            }
            
            // anchor Y
            switch (anchor)
            {
                case ANCHOR.TOP_LEFT:
                case ANCHOR.TOP_CENTER:
                case ANCHOR.TOP_RIGHT:
                    break;

                case ANCHOR.CENTER_LEFT:
                case ANCHOR.CENTER_CENTER:
                case ANCHOR.CENTER_RIGHT:
                    finalPosition.Y = (ParentSize.Y/2) - (Size.Y/2);
                    break;
                
                case ANCHOR.BOTTOM_LEFT:
                case ANCHOR.BOTTOM_CENTER:
                case ANCHOR.BOTTOM_RIGHT:
                    finalPosition.Y = ParentSize.Y - Size.Y;
                    break;
            }

            return finalPosition + (ParentSize * positionPercent) + parentPos + positionPixels;
        }
    }
    
    //Size
    public Vector2<int> sizePixels = new(0,0);
    public Vector2<float> sizePercent = new(1,1);
    public Vector2<float> Size {
        get {
            var a = ParentSize * sizePercent + sizePixels;
            return new(MathF.Max(0f, a.X), MathF.Max(0f, a.Y));
        }
    }

    public bool ClipChildren {get;set;} = false;

    public Rect GetClippingArea()
    {
        var rect = new Rect( 0, 0, Engine.window.Size.X, Engine.window.Size.Y );
        
        if (ClipChildren)
        {
            rect.Position = Position;
            rect.Size = Size;
        }
        
        if (parent is IClipChildren)
            rect = rect.Intersection((parent as IClipChildren)!.GetClippingArea());
        
        return rect;
    }

}
