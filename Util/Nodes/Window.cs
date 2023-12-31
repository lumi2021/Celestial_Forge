using System.Numerics;
using GameEngine.Sys;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Values;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using static GameEngine.Util.Nodes.Window.InputHandler;

namespace GameEngine.Util.Nodes;

public class Window : Node
{

    #pragma warning disable CS8618
    public IWindow window;
    public GL gl;
    #pragma warning restore
    public InputHandler input = new();

    private string _title = "";
    public string Title
    {
        get { return _title; }
        set
        {
            _title = value;
            window.Title = value;
        }
    }

    public Vector2<uint> Size
    {
        get { return new((uint) window.Size.X, (uint) window.Size.Y); }
        set { window.Size = new Vector2D<int> ((int) value.X, (int) value.Y); }
    }

    private WindowState _state = WindowState.Normal;
    public WindowState State
    {
        get { return _state; }
        set
        {
            _state = value;
            window.WindowState = value;
        }
    }


    protected override void Init_()
    {
        window = WindowService.CreateNewWindow( OnLoad );
        _title = window.Title;

        window.Closing += OnClose;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Resize += OnResize;

        input.Start(window, OnInput);

        gl = Engine.gl;

        if (!window.IsInitialized)
            window.Initialize();
    }

    private void OnLoad()
    {
    }

    private void OnClose()
    {
        WindowService.CloseWindow(window);
    }

    private void OnUpdate(double deltaTime)
    {
        List<Node> toUpdate = new();
        toUpdate.AddRange(children);

        while (toUpdate.Count > 0)
        {
            var children = toUpdate[0].children;
            toUpdate[0].RunProcess(deltaTime);
            
            if (toUpdate[0] is not Window)
                toUpdate.AddRange(children);

            toUpdate.RemoveAt(0);
        }

        input.CallProcess();
    }

    private void OnRender(double deltaTime)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        gl.Scissor(0,0, Size.X, Size.Y);

        List<Node> toDraw = new();
        toDraw.AddRange(children);

        while (toDraw.Count > 0)
        {
            Node current = toDraw[0];
            toDraw.RemoveAt(0);

            if (current is Window) continue;

            if (current is ICanvasItem)
            {
                // configurate scissor
                if (current.parent is IClipChildren)
                {
                    var clipRect = (current.parent as IClipChildren)!.GetClippingArea();
                    gl.Scissor((int)clipRect.X, (int)clipRect.Y,(uint)clipRect.Width, (uint)clipRect.Height);
                }

                // checks if it's visible and draw
                if ((current as ICanvasItem)!.Visible)
                    current.RunDraw(deltaTime);
                
                else continue; // Don't draw childrens
            }

            for (int i = current.children.Count - 1; i >= 0; i--)
                toDraw.Insert(0,  current.children[i]);
        }
    }

    private void OnInput(InputEvent e)
    {
        List<Node> toEvent = new();
        toEvent.AddRange(children);

        while (toEvent.Count > 0)
        {
            Node current = toEvent[0];
            toEvent.RemoveAt(0);

            if (current is Window) continue;

            current.RunInputEvent(e);

            for (int i = current.children.Count - 1; i >= 0; i--)
                toEvent.Insert(0,  current.children[i]);
        }
    }

    private void OnResize(Vector2D<int> size)
    {
        gl.Viewport(size);
    }

    public override void Free(bool fromGC = false)
    {
        WindowService.CloseWindow(window);
        base.Free(fromGC);
    }

    public unsafe class InputHandler
    {

        private delegate void InputEventHandler(InputEvent e);
        private event InputEventHandler? InputEventSender;

        private Glfw GLFW = GlfwProvider.GLFW.Value;

        #region key lists
        private readonly List<Keys> keysPressed = new();
        private readonly List<Keys> keysDowned = new();
        private readonly List<Keys> keysReleased = new();

        private readonly List<MouseButton> mousePressed = new();
        private readonly List<MouseButton> mouseDowned = new();
        private readonly List<MouseButton> mouseReleased = new();

        private readonly List<char> _inputedCharList = new();
        #endregion
        public string LastInputedChars
        {
            get
            {
                return new String(_inputedCharList.ToArray());
            }
        }

        private Vector2<int> lastMousePosition = new();
        public Vector2<int> mouseDelta = new();
        private bool mouseMoved = false;

        public void Start(IWindow win, Action<InputEvent> OnEvent)
        {
            GLFW.SetKeyCallback((WindowHandle*) win.Handle, KeyCallback);
            GLFW.SetCharCallback((WindowHandle*) win.Handle, CharCallback);
            GLFW.SetCursorPosCallback((WindowHandle*) win.Handle, CursorPosCallback);
            GLFW.SetMouseButtonCallback((WindowHandle*) win.Handle, MouseButtonCallback);
        
            InputEventSender += new InputEventHandler(OnEvent);
        }

        #region
        // KEYBOARD
        public bool IsActionPressed(Keys key)
        {
            return keysPressed.Contains(key);
        }
        public bool IsActionJustPressed(Keys key)
        {
            return keysDowned.Contains(key);
        }
        public bool IsActionJustReleased(Keys key)
        {
            return keysReleased.Contains(key);
        }

        // MOUSE
        public bool IsActionPressed(MouseButton btn)
        {
            return mousePressed.Contains(btn);
        }
        public bool IsActionJustPressed(MouseButton btn)
        {
            return mouseDowned.Contains(btn);
        }
        public bool IsActionJustReleased(MouseButton btn)
        {
            return mouseReleased.Contains(btn);
        }
        #endregion
        
        public Vector2<int> GetMousePosition()
        {
            return lastMousePosition;
        }

        // FIXME
        //public void SetCursorMode(CursorMode mode)
        //{
        //}
        public unsafe void SetCursorShape(CursorShape shape)
        {
            var cursor = GlfwProvider.GLFW.Value.CreateStandardCursor(shape);
            GlfwProvider.GLFW.Value.SetCursor((WindowHandle*)Engine.window.Handle, cursor);
        }

        public void CallProcess()
        {
            mouseDelta = new();
            keysDowned.Clear();
            keysReleased.Clear();
            mouseDowned.Clear();
            mouseReleased.Clear();
            _inputedCharList.Clear();
        }
        
        // KEYBOARD INPUTS
        private void KeyCallback(WindowHandle* window, Keys key, int scanCode, InputAction action, KeyModifiers mods)
        {
            if (action == InputAction.Press)
            {
                keysPressed.Add(key);
                keysDowned.Add(key);
            }
            else if (action == InputAction.Release)
            {
                keysReleased.Add(key);
                keysPressed.Remove(key);
            }

            var e = new KeyboardInputEvent(
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                action == InputAction.Repeat,
                key, action
            );

            InputEventSender?.Invoke(e);
        }
        private void CharCallback(WindowHandle* window, uint codepoint)
        {
            _inputedCharList.Add(char.ConvertFromUtf32((int)codepoint)[0]);
        }
    
        // MOUSE INPUTS
        private void CursorPosCallback(WindowHandle* window, double x, double y)
        {
            var currentPos = new Vector2<int>((int)x, (int)y);
            if (!mouseMoved)
            {
                lastMousePosition = currentPos;
                mouseMoved = true;
            }
            mouseDelta = currentPos - lastMousePosition;

            var e = new MouseMoveInputEvent(
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                currentPos, lastMousePosition, mouseDelta
            );

            lastMousePosition = currentPos;

            InputEventSender?.Invoke(e);
        }
        private void MouseButtonCallback(WindowHandle* window, MouseButton button, InputAction action, KeyModifiers mods)
        {
            if (action == InputAction.Press)
            {
                mousePressed.Add(button);
                mouseDowned.Add(button);
            }
            else if (action == InputAction.Release)
            {
                mousePressed.Remove(button);
                mouseReleased.Add(button);
            }
            else return;

            var e = new MouseBtnInputEvent(
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                button, action
            );

            InputEventSender?.Invoke(e);
        }
    

        #region INNER CLASSES
        public class InputEvent
        {
            public readonly long timestamp = 0;
            public InputEvent(long timestamp)
            {
                this.timestamp = timestamp;
            }
        }
        public class KeyboardInputEvent : InputEvent
        {
            public readonly bool repeating = false;
            public readonly Keys key;
            public readonly InputAction action;

            public KeyboardInputEvent(
                long timestamp,
                bool repeating,
                Keys key,
                InputAction action
            )
            : base(timestamp)
            {
                this.repeating = repeating;
                this.key = key;
                this.action = action;
            }
        }
        public class MouseInputEvent : InputEvent
        {
            public MouseInputEvent(long timestamp): base(timestamp) {}
        }
        public class MouseBtnInputEvent : MouseInputEvent
        {
            public readonly MouseButton button;
            public readonly InputAction action;

            public MouseBtnInputEvent(long timestamp, MouseButton button, InputAction action)
            : base(timestamp)
            {
                this.button = button;
                this.action = action;
            }
        }
        public class MouseMoveInputEvent : MouseInputEvent
        {
            public readonly Vector2<int> position = new();
            public readonly Vector2<int> lastPosition = new();
            public readonly Vector2<int> positionDelta = new();

            public MouseMoveInputEvent(
                long timestamp,
                Vector2<int> position,
                Vector2<int> lastPosition,
                Vector2<int> positionDelta
            )
            : base(timestamp)
            {
                this.position = position;
                this.lastPosition = lastPosition;
                this.positionDelta = positionDelta;
            }
        }
        #endregion
    }
}