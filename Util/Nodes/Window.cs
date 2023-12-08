using System.Threading;
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

    private Vector2<int> _size = new(800, 600);
    public Vector2<int> Size {
        get { return _size; }
        set { _size = value; }
    }

    protected override void Init_()
    {

        WindowOptions options = WindowOptions.Default with
        {
            Title = "new Window",
            Size = _size.GetAsSilkInt(),
            WindowState = WindowState.Normal,
            Samples = 4
        };

        window = Silk.NET.Windowing.Window.Create(options);

        window.Load += OnLoad;
        window.Closing += OnClose;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Resize += OnResize;

        new Thread(window.Run).Start();

    }

    private void OnLoad()
    {
        gl = window.CreateOpenGL();

        gl.ClearColor(1, 0, 0, 1);
    }

    private void OnClose()
    {
        gl.Dispose();
    }

    private void OnUpdate(double deltaTime)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    private void OnRender(double deltaTime)
    {

    }

    private void OnResize(Vector2D<int> size)
    {

    }

}