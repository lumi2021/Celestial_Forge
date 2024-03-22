using GameEngine.Util.Attributes;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

public class NodeUI : Node, ICanvasItem, IClipChildren
{
    /* SIGNALS */
    public readonly Signal OnClick = new();
    public readonly Signal OnFocus = new();
    public readonly Signal OnUnfocus = new();

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
    [Inspect] public ANCHOR anchor = ANCHOR.TOP_LEFT;
    [Inspect] public uint padding = 0;
    [Inspect] public uint margin = 0;

    // Parent size and position
    private Vector2<float>? _pPos = null;
    private Vector2<float>? _pSize = null;

    private Vector2<float> ParentPosition {

        get {
            if (_pPos == null)
            {
                Vector2<float> pPos = new();
                
                if (parent != null && parent is NodeUI @p)
                    pPos = @p.Position + new Vector2<float>(@p.padding, @p.padding);

                _pPos = pPos;
            }
            return _pPos.Value;
        }

    }
    private Vector2<float> ParentSize {

        get {
            if (_pSize == null)
            {
                Vector2<float> pSize;
                
                if (parent != null && parent is NodeUI @p)
                    pSize = @p.Size - (new Vector2<float>(@p.padding, @p.padding) * 2);

                else if (Viewport != null)
                    pSize = new Vector2<float>(Viewport!.ContainerSize.X, Viewport!.ContainerSize.Y);
                    
                else pSize = new();

                _pSize = pSize;
            }
            return _pSize.Value;
        }

    }

    // Position
    [Inspect] public Vector2<int> positionPixels = new(0,0);
    [Inspect] public Vector2<float> positionPercent = new(0,0);
    public virtual Vector2<float> Position {
        get {
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

            var marginVec2 = new Vector2<float>(margin, margin);
            return finalPosition + (ParentSize * positionPercent) + ParentPosition + positionPixels + marginVec2;
        }
    }
    
    // Size
    [Inspect] public Vector2<int> sizePixels = new(0,0);
    [Inspect] public Vector2<float> sizePercent = new(1,1);
    public virtual Vector2<float> Size {
        get {
            var marginVec2 = new Vector2<float>(margin, margin);
            var a = ParentSize * sizePercent + sizePixels - marginVec2 * 2;
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

    [Inspect] public bool ClipChildren {get;set;} = false;

    // Mouse options
    public enum MouseFilter {Block, Pass, Ignore}
    [Inspect] public MouseFilter mouseFilter = MouseFilter.Block;

    // CSS and styles
    public List<string> classes = [];


    [Inspect] public bool Visible { get; set; } = true;
    public bool GlobalVisible => ((parent as ICanvasItem)?.GlobalVisible ?? true) && Visible;

    [Inspect] public int ZIndex { get; set; } = 0;
    public int GlobalZIndex
    {
        get {

            int pz = 0;

            if (parent is ICanvasItem @p) pz = @p.GlobalZIndex;

            return ZIndex + pz;

        }
    }

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
        if (Viewport != null && Viewport.FocusedUiNode != this)
        {
            Viewport!.FocusedUiNode = this;
            OnFocus.Emit();
        }
    }
    public void Unfocus()
    {
        if (Viewport != null && Viewport.FocusedUiNode == this)
        {
            Viewport!.FocusedUiNode = null;
            OnUnfocus.Emit();
        }
    }

    public void RunUIInputEvent(InputEvent e) => OnUIInputEvent(e);
    public void RunFocusedUIInputEvent(InputEvent e) => OnFocusedUIInputEvent(e);
    public void RunFocusChanged(bool focused) => OnFocusChanged(focused);

    protected virtual void OnFocusedUIInputEvent(InputEvent e) {}
    protected virtual void OnUIInputEvent(InputEvent e)
    {
        if (e.Is<MouseInputEvent>())
        {
            if (mouseFilter == MouseFilter.Ignore) return;

            if (e.Is<MouseBtnInputEvent>(out var @event) && @event.action == Silk.NET.GLFW.InputAction.Press)
            if (new Rect(Position, Size).Intersects(@event.position + Viewport!.Camera2D.position))
            {
                OnClick.Emit(this);
                if (mouseFilter == MouseFilter.Block)
                {
                    Viewport?.SupressInputEvent();
                    Focus();
                }
            }
            else if (Focused)
            {
                Unfocus();
            }
        }
    }

    protected virtual void OnFocusChanged(bool focused) {}

    public void AddClass(string className) => classes.Add(className);
    public void RemoveClass(string className) => classes.Remove(className);
    public bool HasClass(string className) => classes.Contains(className);

    public void Hide() => Visible = false;
    public void Show() => Visible = true;

    protected override void OnTreeParentChanged()
    {

        _pSize = null;
        _pPos = null;

        RequestUpdateAllChildrens();

        base.OnTreeParentChanged();
    }

}
