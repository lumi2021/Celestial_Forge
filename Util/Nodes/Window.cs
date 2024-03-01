using GameEngine.Core;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using static GameEngine.Util.Nodes.Window.InputHandler;

namespace GameEngine.Util.Nodes;

public class Window : Viewport
{

    public IWindow window = null!;
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

    public override Vector2<uint> Size 
    {
        get {
            return _size;
        }
        set {
            _size = value;
            window.Size = new((int)value.X, (int)value.Y);
        }
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

    private ManualResetEvent renderWaitHandle = new ManualResetEvent(false);

    #region rendering things
    private uint _vao;
    private uint _vbo;
    private Material _mat = null!;
    #endregion

    private bool _firstFrame = true;

    protected override void Init_()
    {
        window = WindowService.CreateNewWindow( OnLoad );
        _title = window.Title;

        window.Closing += OnClose;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Resize += OnResize;

        if(window == WindowService.mainWindow)
            input.Start(window, OnInput);

        if (!window.IsInitialized)
            window.Initialize();
    }

    private unsafe void OnLoad()
    {
    }

    private void OnClose()
    {
        WindowService.CloseWindow(window);
    }

    private void OnUpdate(double deltaTime)
    {
        input.CallQueuedInputs();

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
        if  (_firstFrame) unsafe {
        {
            _mat = new Material2D( Material2D.DrawTypes.Texture );

            var gl = Engine.gl;

            _vao = gl.GenVertexArray();
            _vbo = gl.GenBuffer();

            gl.BindVertexArray(_vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            float[] v = [
                0f, 0f, 0f, 0f,
                1f, 0f, 1f, 0f,
                1f, 1f, 1f, 1f,
                0f, 1f, 0f, 1f
            ];

            fixed(float* buf = v)
            gl.BufferData(BufferTargetARB.ArrayBuffer,(nuint)v.Length*sizeof(float),buf,BufferUsageARB.StaticDraw);

            gl.VertexAttribPointer((uint)_mat.GetALocation("aPosition"), 2, GLEnum.Float, false, 2*sizeof(float), (void*) 0);
            gl.VertexAttribPointer((uint)_mat.GetALocation("aTextureCoord"), 2, GLEnum.Float, false, 2*sizeof(float), (void*) 2);
        }}

        Render(_size, deltaTime);

        /* DRAW BUFFER IN THE SCREEN */
        Engine.gl.BindVertexArray(_vao);
        Use();
        _mat.Use();

        Engine.gl.Viewport(0,0, Size.X, Size.Y);
        Engine.gl.Scissor(0,0, Size.X, Size.Y);

        Engine.gl.DrawArrays(PrimitiveType.TriangleFan, 0, 6);
        
        Engine.gl.BindVertexArray(0);
    }

    private void OnInput(InputEvent e)
    {

        List<Node> toIterate = [.. children];

        List<Node> toEvent = new();

        // Get ordered nodes list
        while (toIterate.Count > 0)
        {
            Node current = toIterate[0];
            toIterate.RemoveAt(0);

            if (current is Window) continue;

            toEvent.Add(current);

            for (int i = current.children.Count - 1; i >= 0; i--)
                toIterate.Insert(0,  current.children[i]);
        }
    
        // invert and iterate from top to bottom
        toEvent.Reverse();

        // Focused UI event
        _focusedUiNode?.RunFocusedUIInputEvent(e);

        // UI event
        proceedInput = true;
        foreach(var i in toEvent.Where(e => e is NodeUI))
        {
            var a = i as NodeUI;
            if (a is not ICanvasItem || (a as ICanvasItem)!.Visible && !a.Freeled)
                a!.RunUIInputEvent(e);
            
            if (!proceedInput) break;
        }
        
        // default unhandled event
        proceedInput = true;
        foreach(var i in toEvent)
        {
            if (!i.Freeled) i.RunInputEvent(e);
            if (!proceedInput) break;
        }

    }

    private void OnResize(Vector2D<int> size)
    {
        Size = new((uint)size.X, (uint)size.Y);
    }

    public override void Free()
    {
        WindowService.CloseWindow(window);
        base.Free();
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
        public List<InputEvent> LastInputs = new();
        

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

        public void CallQueuedInputs()
        {
            var inputsToNotify = LastInputs.ToArray();
            LastInputs.Clear();
            foreach (var i in inputsToNotify) InputEventSender?.Invoke(i);
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

            LastInputs.Add(e);
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

            LastInputs.Add(e);
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
                button, action, GetMousePosition()
            );

            LastInputs.Add(e);
        }
    

        #region INNER CLASSES
        public class InputEvent
        {
            public readonly long timestamp = 0;
            public InputEvent(long timestamp)
            {
                this.timestamp = timestamp;
            }

            protected virtual string GetDataAsString()
            {
                return string.Format("timestamp:\t{0};", timestamp);
            }

            public override string ToString()
            {
                return string.Format("{0}(\n{1}\n)",GetType().Name , GetDataAsString());
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

            protected override string GetDataAsString()
            {
                string bd = base.GetDataAsString();
                return string.Format(
                    "repeating:\t{0};\n" +
                    "key:\t{1};\n" +
                    "action:\t{2};\n",
                    repeating, key, action
                    ) + bd;
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
            public readonly Vector2<int> position;

            public MouseBtnInputEvent(long timestamp, MouseButton button, InputAction action, Vector2<int> position)
            : base(timestamp)
            {
                this.button = button;
                this.action = action;
                this.position = position;
            }

            protected override string GetDataAsString()
            {
                string bd = base.GetDataAsString();
                return string.Format(
                    "button:\t{0};\n" +
                    "action:\t{1};\n" +
                    "position:\t{2};\n",
                    button, action, position
                    ) + bd;
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
        
            protected override string GetDataAsString()
            {
                string bd = base.GetDataAsString();
                return string.Format(
                    "position:\t{0};\n" +
                    "last position:\t{1};\n",
                    "delta:\t{2};\n",
                    position, lastPosition, positionDelta
                    ) + bd;
            }

        }
        #endregion
    }
}