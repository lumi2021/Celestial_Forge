using GameEngine.Util.Attributes;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

[Icon("./Assets/icons/Nodes/Button.svg")]
public class Button : NodeUI
{
    private bool _wasBeingHovered = false;

    protected enum ButtonState { Default, Hover, Active }
    private ButtonState _state = ButtonState.Default;
    protected ButtonState State
    {
        get { return _state; }
        set {
            _state = value;
            OnStateChange(_state);
        }
    }

    public enum ButtonTrigger { LeftMouseButton, RightMouseButton, any }
    public ButtonTrigger buttonTrigger = ButtonTrigger.LeftMouseButton;

    public enum ActionTrigger { press, release }
    public ActionTrigger actionTrigger = ActionTrigger.release;

    private Panel Container = new() { mouseFilter = MouseFilter.Ignore };

    // Styles
    #region default

    [Inspect] public Color defaultBackgroundColor = new(100, 100, 100, 0.9f);
    [Inspect] public Color defaultStrokeColor = new(0, 0, 0, 0.9f);
    [Inspect] public uint defaultStrokeSize = 0;
    [Inspect] public Vector4<uint> defaultCornerRadius = new(0,0,0,0);

    #endregion
    #region hover

    [Inspect] public Color? hoverBackgroundColor = new(140, 140, 140, 0.9f);
    [Inspect] public Color? hoverStrokeColor = null;
    [Inspect] public uint? hoverStrokeSize = null;
    [Inspect] public Vector4<uint>? hoverCornerRadius = null;

    #endregion
    #region active

    [Inspect] public Color? activeBackgroundColor = new(80, 80, 80, 0.9f);
    [Inspect] public Color? activeStrokeColor = null;
    [Inspect] public uint? activeStrokeSize = null;
    [Inspect] public Vector4<uint>? activeCornerRadius = null;

    #endregion

    public readonly Signal OnPressed = new();

    protected override void Init_()
    {
        AddAsGhostChild(Container);
    }

    protected override void OnUIInputEvent(InputEvent e)
    {
        if (e.Is<MouseInputEvent>())
        {
            if (mouseFilter == MouseFilter.Ignore) return;

            if (e.Is<MouseMoveInputEvent>(out var @mEvent))
            {
                if (State <= ButtonState.Hover)
                {
                    if (new Rect(Position, Size).Intersects(@mEvent.position + Viewport!.Camera2D.position))
                    {
                        _wasBeingHovered = true;
                        Input.SetCursorShape(Silk.NET.GLFW.CursorShape.Hand);
                        State = ButtonState.Hover;
                    }
                    else if (_wasBeingHovered)
                    {
                        _wasBeingHovered = false;
                        Input.SetCursorShape(Silk.NET.GLFW.CursorShape.Arrow);
                        State = ButtonState.Default;
                    }
                }
            }

            if (e.Is<MouseBtnInputEvent>(out var @bEvent))
            {

                if (new Rect(Position, Size).Intersects(@bEvent.position + Viewport!.Camera2D.position))
                {
                    if (@bEvent.action == Silk.NET.GLFW.InputAction.Press)
                    {
                        State = ButtonState.Active;
                        onClick.Emit(this);
                    }

                    if (
                        actionTrigger == ActionTrigger.press &&
                        @bEvent.action == Silk.NET.GLFW.InputAction.Press ||
                        actionTrigger == ActionTrigger.release &&
                        @bEvent.action == Silk.NET.GLFW.InputAction.Release
                    ) {
                        Viewport?.SupressInputEvent();
                        OnPressed.Emit(this);
                    }

                    if (mouseFilter == MouseFilter.Block)
                    {
                        Viewport?.SupressInputEvent();
                        Focus();
                    }
                }

                if (@bEvent.action == Silk.NET.GLFW.InputAction.Release)
                {
                    if (new Rect(Position, Size).Intersects(@bEvent.position + Viewport!.Camera2D.position))
                        State = ButtonState.Hover;
                    else
                        State = ButtonState.Default;
                }
            }
        }
    }

    protected virtual void OnStateChange(ButtonState state)
    {
        switch (state)
        {
            case ButtonState.Hover:
                Container.BackgroundColor = hoverBackgroundColor ?? defaultBackgroundColor;
                Container.StrokeColor     = hoverStrokeColor ?? defaultStrokeColor;
                Container.StrokeSize      = hoverStrokeSize ?? defaultStrokeSize;
                Container.CornerRadius    = hoverCornerRadius ?? defaultCornerRadius;
                break;

            case ButtonState.Active:
                Container.BackgroundColor = activeBackgroundColor ?? defaultBackgroundColor;
                Container.StrokeColor     = activeStrokeColor ?? defaultStrokeColor;
                Container.StrokeSize      = activeStrokeSize ?? defaultStrokeSize;
                Container.CornerRadius    = activeCornerRadius ?? defaultCornerRadius;
                break;
            
            default:
                Container.BackgroundColor = defaultBackgroundColor;
                Container.StrokeColor     = defaultStrokeColor;
                Container.StrokeSize      = defaultStrokeSize;
                Container.CornerRadius    = defaultCornerRadius;
                break;
        }
    }

}
