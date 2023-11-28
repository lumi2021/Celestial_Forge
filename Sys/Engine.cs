using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace GameEngine.Sys;

public class Engine
{
    #pragma warning disable CS8618
    public static IWindow window;
    public static GL gl;
    #pragma warning restore

    public static NodeRoot root = new();

    public Engine()
    {
        WindowOptions options = WindowOptions.Default with
        {
            Title = "Game Engine",
            Size = new Vector2D<int>(800, 600),
            WindowState = WindowState.Maximized,
            Samples = 4
        };

        window = Window.Create(options);

        window.Load += OnLoad;
        window.Closing += OnClose;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Resize += OnResize;

        window.Run();
    }

    private static unsafe void OnLoad()
    {
        window.Center();

        gl = window.CreateOpenGL();
        gl.Viewport(window.Size);

        gl.ClearColor(1f, 1f, 1f, 1f);

        gl.Enable(EnableCap.Multisample);

        var a = new Pannel();
        root.AddAsChild(a);
        a.backgroundColor = new(80, 58, 101);
        a.sizePercent.X = 0.25f;
        a.RunInit();
    }

    private static void OnClose()
    {
        gl.Dispose();
    }

    private static void OnUpdate(double deltaTime)
    {
        
    }

    private static unsafe void OnRender(double deltaTime)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit);

        foreach (var i in root.children)
        {
            i.RunDraw(deltaTime);
        }
    }

    private static void OnResize(Vector2D<int> size)
    {
        gl.Viewport(size);
    }

}
