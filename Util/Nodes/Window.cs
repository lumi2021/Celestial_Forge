using System.Threading;
using GameEngine.Sys;
using GameEngine.Util.Values;
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

    private String _title = "";
    public String Title
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

}