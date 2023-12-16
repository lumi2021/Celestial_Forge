using System.Numerics;
using GameEngine.Sys;
using GameEngine.Util.Values;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

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

    private Vector2<int> _size = new(800, 600);
    public Vector2<int> Size
    {
        get { return _size; }
        set { _size = value; }
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

        input.Start(window);

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

        List<Node> toDraw = new();
        toDraw.AddRange(children);

        while (toDraw.Count > 0)
        {
            Node current = toDraw[0];
            toDraw.RemoveAt(0);

            if (current is Window) continue;

            current.RunDraw(deltaTime);

            for (int i = current.children.Count - 1; i >= 0; i--)
                    toDraw.Insert(0,  current.children[i]);
        }
    }

    private void OnResize(Vector2D<int> size)
    {
        gl.Viewport(size);
    }


    public class InputHandler
    {

        private IInputContext? _input;

        #region key lists
        private readonly List<Key> keysPressed = new();
        private readonly List<Key> keysDowned = new();
        private readonly List<Key> keysReleased = new();

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

        private Vector2<float> lastMousePosition;
        public Vector2<float> mouseDelta;
        private bool mouseMoved = false;

        public void Start(IWindow win)
        {
            _input = win.CreateInput();
            for (int i = 0; i < _input.Keyboards.Count; i++)
            {
                var kboard = _input.Keyboards[i];
                kboard.KeyDown += KeyDown;
                kboard.KeyUp += KeyUp;
                kboard.KeyChar += CharInput;
            }
            for (int i = 0; i < _input.Mice.Count; i++)
            {
                _input.Mice[i].MouseDown += MouseDown;
                _input.Mice[i].MouseUp += MouseUp;
                _input.Mice[i].MouseMove += MouseMove;
            }
        }

        // KEYBOARD
        public bool IsActionPressed(Key key)
        {
            return keysPressed.Contains(key);
        }
        public bool IsActionJustPressed(Key key)
        {
            return keysDowned.Contains(key);
        }
        public bool IsActionJustReleased(Key key)
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
        
        
        public Vector2<float> GetMousePosition()
        {
            return lastMousePosition;
        }

        public void SetCursorMode(CursorMode mode)
        {
            for (int i = 0; i < _input!.Mice.Count; i++)
            {
                _input.Mice[i].Cursor.CursorMode = mode;
            }
        }
        public unsafe void SetCursorShape(Silk.NET.GLFW.CursorShape shape)
        {
            var cursor = Silk.NET.GLFW.GlfwProvider.GLFW.Value.CreateStandardCursor(shape);
            Silk.NET.GLFW.GlfwProvider.GLFW.Value.SetCursor((Silk.NET.GLFW.WindowHandle*)Engine.window.Handle, cursor);
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
        
        // KEYBOARD
        private void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            keysPressed.Add(key);
            keysDowned.Add(key);
        }
        private void KeyUp(IKeyboard keyboard, Key key, int keyCode)
        {
            keysPressed.Remove(key);
            keysReleased.Add(key);
        }
        private void CharInput(IKeyboard keyboard, char c)
        {
            _inputedCharList.Add(c);
        }

        // MOUSE
        private void MouseDown(IMouse mouse, MouseButton button)
        {
            mousePressed.Add(button);
            mouseDowned.Add(button);
        }
        private void MouseUp(IMouse mouse, MouseButton button)
        {
            mousePressed.Remove(button);
            mouseReleased.Add(button);
        }
        private void MouseMove(IMouse mouse, Vector2 numericsPos)
        {
            if (!mouseMoved)
            {
                lastMousePosition = new(numericsPos);
                mouseMoved = true;
            }

            Vector2<float> position = new(numericsPos);

            mouseDelta = position - lastMousePosition;

            lastMousePosition = position;
        }
    }

}