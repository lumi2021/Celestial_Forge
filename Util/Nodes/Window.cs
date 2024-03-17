using GameEngine.Core;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

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
            if (window != null)
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

        if (!window.IsInitialized)
            window.Initialize();
    }

    private unsafe void OnLoad(IWindow win)
    {
        input.Start(win, OnInput);
        Size = new((uint)win.Size.X, (uint)win.Size.Y);
    }

    private void OnClose()
    {
        WindowService.CloseWindow(window);
    }

    private void OnUpdate(double deltaTime)
    {
        input.CallQueuedInputs();

        List<Node> toUpdate = [.. children];

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

        Dictionary<int, List<Node>> toEventIndexes = [];

        // Get ordered nodes list
        while (toIterate.Count > 0)
        {
            Node current = toIterate[0];
            toIterate.RemoveAt(0);

            if (current is Window) continue;

            var zindex = (current as ICanvasItem)?.GlobalZIndex ?? 0;

            if (!toEventIndexes.ContainsKey(zindex)) toEventIndexes.Add(zindex, []);
            toEventIndexes[zindex].Add(current);

            var childrenToAdd = current.GetAllChildren;
            for (int i = childrenToAdd.Count - 1; i >= 0; i--)
                toIterate.Insert(0, childrenToAdd[i]);
        }

        List<Node> toEvent = [];

        foreach (var i in toEventIndexes)
            toEvent.AddRange(i.Value);

        // invert and iterate from top to bottom
        toEvent.Reverse();

        // UI event
        proceedInput = true;
        foreach(var i in toEvent.Where(e => e is NodeUI))
        {
            var a = i as NodeUI;
            if (a!.GlobalVisible && !i.Freeled)
                a!.RunUIInputEvent(e);
            
            if (!proceedInput) break;
        }
        
        // Focused UI event
        _focusedUiNode?.RunFocusedUIInputEvent(e);

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
        private readonly List<Keys> keysPressed = [];
        private readonly List<Keys> keysDowned = [];
        private readonly List<Keys> keysReleased = [];

        private readonly List<MouseButton> mousePressed = [];
        private readonly List<MouseButton> mouseDowned = [];
        private readonly List<MouseButton> mouseReleased = [];

        private readonly List<char> _inputedCharList = [];
        #endregion
        public string LastInputedChars => new(_inputedCharList.ToArray());
        public List<InputEvent> LastInputs = [];
        

        private Vector2<int> lastMousePosition = new();
        public Vector2<int> mouseDelta = new();
        private bool mouseMoved = false;

        public void Start(IWindow win, Action<InputEvent> OnEvent)
        {
            GLFW.SetKeyCallback((WindowHandle*) win.Handle, KeyCallback);
            GLFW.SetCharCallback((WindowHandle*) win.Handle, CharCallback);
            GLFW.SetCursorPosCallback((WindowHandle*) win.Handle, CursorPosCallback);
            GLFW.SetMouseButtonCallback((WindowHandle*) win.Handle, MouseButtonCallback);
            GLFW.SetScrollCallback((WindowHandle*) win.Handle, MouseScrollCallback);
        
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

            var eo = new KeyboardKeyInputEvent(action == InputAction.Repeat,key, action);
            var e = new InputEvent(DateTime.Now.TimeOfDay, eo);

            LastInputs.Add(e);
        }
        private void CharCallback(WindowHandle* window, uint codepoint)
        {
            var eo = new KeyboardCharInputEvent(char.ConvertFromUtf32((int)codepoint)[0]);
            var e = new InputEvent(DateTime.Now.TimeOfDay, eo);

            LastInputs.Add(e);
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

            var eo = new MouseMoveInputEvent(currentPos, lastMousePosition, mouseDelta);
            var e = new InputEvent(DateTime.Now.TimeOfDay, eo);

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

            var eo = new MouseBtnInputEvent(button, action, GetMousePosition());
            var e = new InputEvent(DateTime.Now.TimeOfDay, eo);

            LastInputs.Add(e);
        }
        private void MouseScrollCallback(WindowHandle* window, double offsetX, double offsetY)
        {
            var eo = new MouseScrollInputEvent(new( offsetX, offsetY ));
            var e = new InputEvent(DateTime.Now.TimeOfDay, eo);

            LastInputs.Add(e);
        }
    }

}