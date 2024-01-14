using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using static GameEngine.Util.Nodes.Window.InputHandler;

namespace GameEngine.Util.Nodes;

public class NodeUI : Node, IClipChildren
{
    /* SIGNALS */
    public readonly Signal onClick = new();
    public readonly Signal onFocus = new();
    public readonly Signal onUnfocus = new();

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
    [Inspect]
    public ANCHOR anchor = ANCHOR.TOP_LEFT;

    //Get parent size
    private Vector2<float> ParentSize {
        get {
            Vector2<float> parentSize;
            
            if (parent != null && parent is NodeUI)
                parentSize = (parent as NodeUI)!.Size;
            else
                parentSize = new Vector2<float>(ParentWindow!.Size.X, ParentWindow!.Size.Y+1);

            return parentSize;
        }
    }

    //Position
    [Inspect] public Vector2<int> positionPixels = new(0,0);
    [Inspect] public Vector2<float> positionPercent = new(0,0);
    public virtual Vector2<float> Position {
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
    [Inspect] public Vector2<int> sizePixels = new(0,0);
    [Inspect] public Vector2<float> sizePercent = new(1,1);
    public virtual Vector2<float> Size {
        get {
            var a = ParentSize * sizePercent + sizePixels;
            return new(MathF.Max(0f, a.X), MathF.Max(0f, a.Y));
        }
    }

    public Vector2<float> ContentSize
    {
        get
        {
            var resultRect = new Rect();

            foreach (var i in children.Where(e => e is NodeUI))
            {
                var j = (NodeUI)i!;
                resultRect = resultRect.FitInside(new(new(j.positionPixels.X, j.positionPixels.Y), j.Size));
            }

            return resultRect.Size;
        }
    }

    public bool Focused
    {
        get { return this == ParentWindow?.FocusedUiNode; }
        set
        {
            if (value) Focus();
            else Unfocus();
        }
    }

    [Inspect]
    public bool ClipChildren {get;set;} = false;

    // Mouse options
    public enum MouseFilter {Block, Pass, Ignore}
    public MouseFilter mouseFilter = MouseFilter.Block;

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

    public void Focus()
    {
        ParentWindow!.FocusedUiNode = this;
        onFocus.Emit();
    }
    public void Unfocus()
    {
        if (ParentWindow != null && ParentWindow.FocusedUiNode == this)
        {
            ParentWindow!.FocusedUiNode = null;
            onUnfocus.Emit();
        }
    }

    public void RunUIInputEvent(InputEvent e)
    { OnUIInputEvent(e); }
    public void RunFocusedUIInputEvent(InputEvent e)
    { OnFocusedUIInputEvent(e); }
    public void RunFocusChanged(bool focused)
    { OnFocusChanged(focused); }

    protected virtual void OnFocusedUIInputEvent(InputEvent e)
    {
        if (e is MouseInputEvent)
        {
            if (mouseFilter == MouseFilter.Ignore) return;

            if (e is MouseBtnInputEvent @event && @event.action == Silk.NET.GLFW.InputAction.Press)
            if (!new Rect(Position, Size).Intersects(@event.position))
            {
                Unfocus();
            }
        }
    }
    protected virtual void OnUIInputEvent(InputEvent e)
    {
        if (e is MouseInputEvent)
        {
            if (mouseFilter == MouseFilter.Ignore) return;

            if (e is MouseBtnInputEvent @event && @event.action == Silk.NET.GLFW.InputAction.Press)
            if (new Rect(Position, Size).Intersects(@event.position))
            {
                Focus();
                onClick.Emit(this);
                if (mouseFilter == MouseFilter.Block)
                    ParentWindow?.SupressInputEvent();
            }
        }
    }

    protected virtual void OnFocusChanged(bool focused) {}
}
