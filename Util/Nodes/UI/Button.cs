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

    private Color _defaultBackgroundColor = new(100, 100, 100, 0.9f);
    private Color _defaultStrokeColor = new(0, 0, 0, 0.9f);
    private uint _defaultStrokeSize = 0;
    private Vector4<uint> _defaultCornerRadius = new(0,0,0,0);

    [Inspect] public Color DefaultBackgroundColor
    {
        get => _defaultBackgroundColor;
        set
        {
            _defaultBackgroundColor = value;
            OnStyleChange();
        }
    }
    [Inspect] public Color DefaultStrokeColor
    {
        get => _defaultStrokeColor;
        set
        {
            _defaultStrokeColor = value;
            OnStyleChange();
        }
    }
    [Inspect] public uint DefaultStrokeSize
    {
        get => _defaultStrokeSize;
        set
        {
            _defaultStrokeSize = value;
            OnStyleChange();
        }
    }
    [Inspect] public Vector4<uint> DefaultCornerRadius
    {
        get => _defaultCornerRadius;
        set
        {
            _defaultCornerRadius = value;
            OnStyleChange();
        }
    }

    #endregion
    #region hover

    private Color? _hoverBackgroundColor = new(140, 140, 140, 0.9f);
    private Color? _hoverStrokeColor = null;
    private uint? _hoverStrokeSize = null;
    private Vector4<uint>? _hoverCornerRadius = null;

    [Inspect] public Color? HoverBackgroundColor
    {
        get => _hoverBackgroundColor;
        set
        {
            _hoverBackgroundColor = value;
            OnStyleChange();
        }
    }
    [Inspect] public Color? HoverStrokeColor
    {
        get => _hoverStrokeColor;
        set
        {
            _hoverStrokeColor = value;
            OnStyleChange();
        }
    }
    [Inspect] public uint? HoverStrokeSize
    {
        get => _hoverStrokeSize;
        set
        {
            _hoverStrokeSize = value;
            OnStyleChange();
        }
    }
    [Inspect] public Vector4<uint>? HoverCornerRadius
    {
        get => _hoverCornerRadius;
        set
        {
            _hoverCornerRadius = value;
            OnStyleChange();
        }
    }

    #endregion
    #region active

    private Color? _activeBackgroundColor = new(80, 80, 80, 0.9f);
    private Color? _activeStrokeColor = null;
    private uint? _activeStrokeSize = null;
    private Vector4<uint>? _activeCornerRadius = null;

    [Inspect] public Color? ActiveBackgroundColor
    {
        get => _activeBackgroundColor;
        set
        {
            _activeBackgroundColor = value;
            OnStyleChange();
        }
    }
    [Inspect] public Color? ActiveStrokeColor
    {
        get => _activeStrokeColor;
        set
        {
            _activeStrokeColor = value;
            OnStyleChange();
        }
    }
    [Inspect] public uint? ActiveStrokeSize
    {
        get => _activeStrokeSize;
        set
        {
            _activeStrokeSize = value;
            OnStyleChange();
        }
    }
    [Inspect] public Vector4<uint>? ActiveCornerRadius
    {
        get => _activeCornerRadius;
        set
        {
            _activeCornerRadius = value;
            OnStyleChange();
        }
    }

    #endregion
    #region selected

    private Color? _selectedBackgroundColor = new(20, 20, 190, 0.9f);
    private Color? _selectedStrokeColor = null;
    private uint? _selectedStrokeSize = null;
    private Vector4<uint>? _selectedCornerRadius = null;

    [Inspect] public Color? SelectedBackgroundColor
    {
        get => _selectedBackgroundColor;
        set
        {
            _selectedBackgroundColor = value;
            OnStyleChange();
        }
    }
    [Inspect] public Color? SelectedStrokeColor
    {
        get => _selectedStrokeColor;
        set
        {
            _selectedStrokeColor = value;
            OnStyleChange();
        }
    }
    [Inspect] public uint? SelectedStrokeSize
    {
        get => _selectedStrokeSize;
        set
        {
            _selectedStrokeSize = value;
            OnStyleChange();
        }
    }
    [Inspect] public Vector4<uint>? SelectedCornerRadius
    {
        get => _selectedCornerRadius;
        set
        {
            _selectedCornerRadius = value;
            OnStyleChange();
        }
    }

    #endregion
    #region disabled

    private Color? _disabledBackgroundColor = new(20, 20, 20, 0.5f);
    private Color? _disabledStrokeColor = null;
    private uint? _disabledStrokeSize = null;
    private Vector4<uint>? _disabledCornerRadius = null;

    [Inspect] public Color? DisabledBackgroundColor
    {
        get => _disabledBackgroundColor;
        set
        {
            _disabledBackgroundColor = value;
            OnStyleChange();
        }
    }
    [Inspect] public Color? DisabledStrokeColor
    {
        get => _disabledStrokeColor;
        set
        {
            _disabledStrokeColor = value;
            OnStyleChange();
        }
    }
    [Inspect] public uint? DisabledStrokeSize
    {
        get => _disabledStrokeSize;
        set
        {
            _disabledStrokeSize = value;
            OnStyleChange();
        }
    }
    [Inspect] public Vector4<uint>? DisabledCornerRadius
    {
        get => _disabledCornerRadius;
        set
        {
            _disabledCornerRadius = value;
            OnStyleChange();
        }
    }

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
                        OnClick.Emit(this);
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

    protected void OnStyleChange() => OnStateChange(_state);
    protected virtual void OnStateChange(ButtonState state)
    {
        switch (state)
        {
            case ButtonState.Hover:
                _bgColor      = _hoverBackgroundColor ?? _defaultBackgroundColor;
                _strokeColor  = _hoverStrokeColor ?? _defaultStrokeColor;
                _strokeSize   = _hoverStrokeSize ?? _defaultStrokeSize;
                _cornerRadius = _hoverCornerRadius ?? _defaultCornerRadius;
                break;

            case ButtonState.Active:
                _bgColor      = _activeBackgroundColor ?? _defaultBackgroundColor;
                _strokeColor  = _activeStrokeColor ?? _defaultStrokeColor;
                _strokeSize   = _activeStrokeSize ?? _defaultStrokeSize;
                _cornerRadius = _activeCornerRadius ?? _defaultCornerRadius;
                break;
            
            case ButtonState.Selected:
                _bgColor      = _selectedBackgroundColor ?? _defaultBackgroundColor;
                _strokeColor  = _selectedStrokeColor ?? _defaultStrokeColor;
                _strokeSize   = _selectedStrokeSize ?? _defaultStrokeSize;
                _cornerRadius = _selectedCornerRadius ?? _defaultCornerRadius;
                break;

            case ButtonState.Disabled:
                _bgColor      = _disabledBackgroundColor ?? _defaultBackgroundColor;
                _strokeColor  = _disabledStrokeColor ?? _defaultStrokeColor;
                _strokeSize   = _disabledStrokeSize ?? _defaultStrokeSize;
                _cornerRadius = _disabledCornerRadius ?? _defaultCornerRadius;
                break;
            
            default:
                _bgColor      = _defaultBackgroundColor;
                _strokeColor  = _defaultStrokeColor;
                _strokeSize   = _defaultStrokeSize;
                _cornerRadius = _defaultCornerRadius;
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
