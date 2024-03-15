using GameEngine.Util.Attributes;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

public class ScrollBar : NodeUI
{
    
    private readonly Panel _scrollButton = new()
    {
        BackgroundColor = new(255, 0, 0),
        sizePercent = new(0,0),
        sizePixels = new(15, 30),
        anchor = ANCHOR.TOP_RIGHT,
        mouseFilter = MouseFilter.Ignore
    };

    [Inspect]
    public NodeUI? target = null;

    [Inspect] public Color defaultColor = new(0.3f, 0.3f, 0.3f);
    [Inspect] public Color holdingColor = new(0.8f, 0.8f, 0.8f);

    private bool _holding = false;
    private Color Col
    { set { _scrollButton.BackgroundColor = value; } }

    protected NodeUI? Container {get { return (NodeUI) parent!; }}
    protected Vector2<float> TSize
    {
        get
        {
            if (target is TextField @Tf) return @Tf.Size;
            else if (target is NodeUI @Nui)
            {
                return @Nui.ContentSize;
            }
            else return new();
        }
    }

    protected override void Init_()
    {
        base.Init_();

        AddAsGhostChild(_scrollButton);
    }

    protected override void OnUIInputEvent(InputEvent e)
    {
        base.OnUIInputEvent(e);

        if (!_scrollButton.Visible) return;
        
        if (e.Is<MouseBtnInputEvent>(out var @btnEvent))
        {
            if (new Rect(_scrollButton.Position, _scrollButton.Size).Intersects(@btnEvent.position))
            if (@btnEvent.action == Silk.NET.GLFW.InputAction.Press)
                _holding = true;
        }

        if (e.Is<MouseMoveInputEvent>(out var @moveEvent))
        {
            if (_holding)
            {
                float d = 0f;

                if (@moveEvent.positionDelta.Y > 0 &&
                _scrollButton.Position.Y < Container?.Position.Y + Container?.Size.Y - _scrollButton.Size.Y)
                    d = @moveEvent.positionDelta.Y
                    + MathF.Min(Container!.Position.Y + Container.Size.Y
                    - (_scrollButton.Position.Y + _scrollButton.Size.Y + @moveEvent.positionDelta.Y), 0);

                else if (@moveEvent.positionDelta.Y < 0 && _scrollButton.Position.Y > Container?.Position.Y)
                    d = @moveEvent.positionDelta.Y
                    + MathF.Max(Container!.Position.Y - _scrollButton.Position.Y
                    - @moveEvent.positionDelta.Y, 0);
                
                _scrollButton.positionPixels.Y += (int) d;
                target!.positionPixels.Y = (int) ((-_scrollButton.Position.Y + Container!.Position.Y)
                * (TSize.Y / Container.Size.Y));
            }
            
            if (new Rect(_scrollButton.Position, _scrollButton.Size).Intersects(@moveEvent.position))
                Col = holdingColor;
            else Col = defaultColor;
        }
    }

    protected override void OnInputEvent(InputEvent e)
    {
        if (e.Is<MouseBtnInputEvent>(out var @event) && _holding
        && @event.action == Silk.NET.GLFW.InputAction.Release)
            _holding = false;

        if (Container == null || target == null)
        {
            _scrollButton.Hide();
        }
        else {
            if (Container.Size.Y > TSize.Y)
                _scrollButton.Hide();
            else
            {
                _scrollButton.Show();
                _scrollButton.sizePercent.Y = 1f / (TSize.Y / Container.Size.Y);
                _scrollButton.sizePixels.Y = -1;


                if(_scrollButton.positionPixels.Y + _scrollButton.Size.Y
                > Container.Position.Y + Container.Size.Y)
                    _scrollButton.positionPixels.Y -= (int) (_scrollButton.positionPixels.Y
                    + _scrollButton.Size.Y - Container.Position.Y + Container.Size.Y);
            }
        }
    }

}
