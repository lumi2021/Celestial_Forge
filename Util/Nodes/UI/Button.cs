using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

[Icon("./Assets/icons/Nodes/Button.svg")]
public class Button : NodeUI
{
    private bool _wasBeingHovered = false;

    protected enum ButtonState { Disabled, Default, Hover, Active, Selected }
    private ButtonState _state = ButtonState.Default;
    protected ButtonState State
    {
        get { return _state; }
        set {
            _state = value;
            OnStateChange(_state);
        }
    }

    private bool _togleable = false;
    [Inspect] public bool Togleable
    {
        get => _togleable;
        set => _togleable = value;
    }

    protected bool active = false;

    private bool _disabled = false;
    [Inspect] public bool Disabled
    {
        get => _disabled;
        set => _disabled = value;
    }

    public enum ButtonTrigger { LeftMouseButton, RightMouseButton, any }
    [Inspect] public ButtonTrigger buttonTrigger = ButtonTrigger.LeftMouseButton;

    public enum ActionTrigger { press, release }
    [Inspect] public ActionTrigger actionTrigger = ActionTrigger.release;

    private ButtonGroup? _buttonGroup;
    [Inspect] public ButtonGroup? ButtonGroup
    {
        get => _buttonGroup;
        set
        {
            if (value == _buttonGroup) return;
            
            if (_buttonGroup != null)
                _buttonGroup.UnselectAll -= Unactivate;

            _buttonGroup = value;
            
            if (_buttonGroup != null)
                _buttonGroup.UnselectAll += Unactivate;
        }
    }

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
    #region selected

    [Inspect] public Color? selectedBackgroundColor = new(20, 20, 190, 0.9f);
    [Inspect] public Color? selectedStrokeColor = null;
    [Inspect] public uint? selectedStrokeSize = null;
    [Inspect] public Vector4<uint>? selectedCornerRadius = null;

    #endregion
    #region disabled

    [Inspect] public Color? disabledBackgroundColor = new(20, 20, 20, 0.5f);
    [Inspect] public Color? disabledStrokeColor = null;
    [Inspect] public uint? disabledStrokeSize = null;
    [Inspect] public Vector4<uint>? disabledCornerRadius = null;

    #endregion

    public readonly Signal OnPressed = new();
    public readonly Signal OnToggle = new();

    [Inspect] public Material material = new Material2D( Material2D.DrawTypes.SolidColor );

    private Color _bgColor = new(100, 100, 100, 0.9f);
    private Color _strokeColor = new(0, 0, 0, 0.9f);
    private uint _strokeSize = 0;
    private Vector4<uint> _cornerRadius = new(0,0,0,0);

    protected override void Init_()
    {
        float[] v = [ 0.0f,0.0f, 1.0f,0.0f, 1.0f,1.0f, 0.0f,1.0f ];
        float[] uv = [ 0f,0f, 1f,0f, 1f,1f, 0f,1f ];
        uint[] i = [ 0,1,3, 1,2,3 ];

        DrawService.CreateBuffer(NID, "aPosition");
        DrawService.SetBufferData(NID, "aPosition", v, 2);

        DrawService.CreateBuffer(NID, "aTextureCoord");
        DrawService.SetBufferData(NID, "aTextureCoord", uv, 2);
            
        DrawService.SetElementBufferData(NID, i);

        DrawService.EnableAtributes(NID, material);

        material.SetUniform("color", _bgColor);
        material.SetUniform("strokeColor", _strokeColor);
        material.SetUniform("strokeSize", _strokeSize);
        material.SetUniform("cornerRadius", _cornerRadius);
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
                    )
                    {
                        if (!Togleable)
                        {
                            State = ButtonState.Active;
                            OnPressed.Emit(this);
                        }
                        else
                        {
                            if (!active)
                            {
                                ButtonGroup?.InvokeUnselectAll();
                                State = ButtonState.Selected;
                                OnPressed.Emit(this);
                                OnToggle.Emit(this, true);
                                active = true;
                            }
                            else
                            {
                                State = ButtonState.Default;
                                OnToggle.Emit(this, false);
                                active = false;
                            }
                        }

                        Viewport?.SupressInputEvent();
                    }

                    if (mouseFilter == MouseFilter.Block)
                    {
                        Viewport?.SupressInputEvent();
                        Focus();
                    }
                }

                if (@bEvent.action == Silk.NET.GLFW.InputAction.Release)
                {
                    if (!Togleable || (Togleable && !active))
                    {
                        if (new Rect(Position, Size).Intersects(@bEvent.position + Viewport!.Camera2D.position))
                            State = ButtonState.Hover;
                        else
                            State = ButtonState.Default;
                    }
                }
            }
        }
    }

    protected virtual void OnStateChange(ButtonState state)
    {
        switch (state)
        {
            case ButtonState.Hover:
                _bgColor      = hoverBackgroundColor ?? defaultBackgroundColor;
                _strokeColor  = hoverStrokeColor ?? defaultStrokeColor;
                _strokeSize   = hoverStrokeSize ?? defaultStrokeSize;
                _cornerRadius = hoverCornerRadius ?? defaultCornerRadius;
                break;

            case ButtonState.Active:
                _bgColor      = activeBackgroundColor ?? defaultBackgroundColor;
                _strokeColor  = activeStrokeColor ?? defaultStrokeColor;
                _strokeSize   = activeStrokeSize ?? defaultStrokeSize;
                _cornerRadius = activeCornerRadius ?? defaultCornerRadius;
                break;
            
            case ButtonState.Selected:
                _bgColor      = selectedBackgroundColor ?? defaultBackgroundColor;
                _strokeColor  = selectedStrokeColor ?? defaultStrokeColor;
                _strokeSize   = selectedStrokeSize ?? defaultStrokeSize;
                _cornerRadius = selectedCornerRadius ?? defaultCornerRadius;
                break;

            case ButtonState.Disabled:
                _bgColor      = disabledBackgroundColor ?? defaultBackgroundColor;
                _strokeColor  = disabledStrokeColor ?? defaultStrokeColor;
                _strokeSize   = disabledStrokeSize ?? defaultStrokeSize;
                _cornerRadius = disabledCornerRadius ?? defaultCornerRadius;
                break;
            
            default:
                _bgColor      = defaultBackgroundColor;
                _strokeColor  = defaultStrokeColor;
                _strokeSize   = defaultStrokeSize;
                _cornerRadius = defaultCornerRadius;
                break;
        }

        material.SetUniform("color", _bgColor);
        material.SetUniform("strokeColor", _strokeColor);
        material.SetUniform("strokeSize", _strokeSize);
        material.SetUniform("cornerRadius", _cornerRadius);
        
    }

    protected override void Draw(double deltaT)
    {
        material.Use();

        var fPos = Position - new Vector2<float>(_strokeSize, _strokeSize);
        var fSize = Size + new Vector2<float>(_strokeSize * 2, _strokeSize * 2);

        var world = MathHelper.Matrix4x4CreateRect(fPos, fSize) * Viewport!.Camera2D.GetViewOffset();
        var proj = Viewport!.Camera2D.GetProjection();

        material.SetTranslation(world);
        material.SetProjection(proj);

        material.SetUniform("pixel_size", new Vector2<float>(1,1) / fSize);
        material.SetUniform("size_in_pixels", (Vector2<uint>)fSize);

        DrawService.Draw(NID);
    }

    private void Unactivate()
    {
        active = false;
        State = ButtonState.Default;
    }

}
