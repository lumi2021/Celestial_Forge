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
    [Inspect]
    public uint padding = 0;

    //Get parent size
    private Vector2<float> ParentSize {
        get {
            Vector2<float> pSize;
            
            if (parent != null && parent is NodeUI @p)
                pSize = @p.Size - new Vector2<float>(@p.padding*2, @p.padding*2);

            else if (Viewport != null)
                pSize = new Vector2<float>(Viewport!.ContainerSize.X, Viewport!.ContainerSize.Y+1);
                
            else pSize = new();

            return pSize;
        }
    }

    //Position
    [Inspect] public Vector2<int> positionPixels = new(0,0);
    [Inspect] public Vector2<float> positionPercent = new(0,0);
    public virtual Vector2<float> Position {
        get {
            Vector2<float> parentPos = new(0, -1);

            if (parent != null && parent is NodeUI p)
                parentPos = p.Position + new Vector2<int>((int)p.padding, (int)p.padding);

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
        get { return this == Viewport?.FocusedUiNode; }
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

    // CSS and styles
    public List<string> classes = [];

    public Rect GetClippingArea()
    {
        var rect = new Rect(
            -Viewport!.Camera2D.position,
            (Vector2<float>) Viewport!.Size
        );
        
        if (ClipChildren)
        {
            rect.Position = Position - Viewport.Camera2D.position;
            rect.Size = Size;
            rect /= Viewport.Camera2D.zoom;
        }
        
        if (parent is IClipChildren p)
        {
            rect = rect.Intersection(p.GetClippingArea());
        }
        
        return rect;
    }

    public void Focus()
    {
        Viewport!.FocusedUiNode = this;
        onFocus.Emit();
    }
    public void Unfocus()
    {
        if (Viewport != null && Viewport.FocusedUiNode == this)
        {
            Viewport!.FocusedUiNode = null;
            onUnfocus.Emit();
        }
    }

    public void RunUIInputEvent(InputEvent e) => OnUIInputEvent(e);
    public void RunFocusedUIInputEvent(InputEvent e) => OnFocusedUIInputEvent(e);
    public void RunFocusChanged(bool focused) => OnFocusChanged(focused);

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
            if (new Rect(Position, Size).Intersects(@event.position + Viewport!.Camera2D.position))
            {
                onClick.Emit(this);
                if (mouseFilter == MouseFilter.Block)
                {
                    Viewport?.SupressInputEvent();
                    Focus();
                }
            }
        }
    }

    protected virtual void OnFocusChanged(bool focused) {}

    public void AddClass(string className) => classes.Add(className);
    public void RemoveClass(string className) => classes.Remove(className);
    public bool HasClass(string className) => classes.Contains(className);

}
