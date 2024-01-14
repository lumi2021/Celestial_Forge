using GameEngine.Util.Attributes;
using GameEngine.Util.Values;
using static GameEngine.Util.Nodes.Window.InputHandler;

namespace GameEngine.Util.Nodes;

public class ScrollBar : NodeUI
{
    
    private readonly Pannel _scrollButton = new()
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

    protected override void Init_()
    {
        base.Init_();

        AddAsChild(_scrollButton);
    }

    protected override void OnUIInputEvent(InputEvent e)
    {
        base.OnUIInputEvent(e);
        
        if (e is MouseBtnInputEvent @btnEvent)
        {
            if (new Rect(_scrollButton.Position, _scrollButton.Size).Intersects(@btnEvent.position))
            if (@btnEvent.action == Silk.NET.GLFW.InputAction.Press)
                _holding = true;
        }
        if (e is MouseMoveInputEvent @moveEvent)
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
                * (target.Size.Y / Container.Size.Y * 1.5));
            }
            
            if (new Rect(_scrollButton.Position, _scrollButton.Size).Intersects(@moveEvent.position))
                Col = holdingColor;
            else Col = defaultColor;
        }
    
        if (Container == null || target == null)
        {
            _scrollButton.Hide();
        }
        else {
            if (Container.Size.Y > target.Size.Y)
                _scrollButton.Hide();
            else
            {
                _scrollButton.Show();
                _scrollButton.sizePercent.Y = 1f / (target.Size.Y / Container.Size.Y);

               //_scrollButton.positionPixels.Y = (int)(Container.Size.Y - _scrollButton.Size.Y)-1;
            }
        }
    }

    protected override void OnInputEvent(InputEvent e)
    {
        if (e is MouseBtnInputEvent @event && _holding
        && @event.action == Silk.NET.GLFW.InputAction.Release)
            _holding = false;
    }
}
