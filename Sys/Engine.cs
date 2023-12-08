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

        window = Silk.NET.Windowing.Window.Create(options);

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
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        var a = PackagedScene.Load("Data/Screens/editor.json").Instantiate();
        root.AddAsChild(a);

        var b = new Util.Nodes.Window();
        root.AddAsChild(b);

    }

    private static void OnClose()
    {
        gl.Dispose();
    }

    private static void OnUpdate(double deltaTime)
    {
        List<Node> toUpdate = new() {root};

        while (toUpdate.Count > 0)
        {
            var children = toUpdate[0].children;
            toUpdate[0].RunProcess(deltaTime);
            toUpdate.RemoveAt(0);
            toUpdate.AddRange(children);
        }

        Input.CallProcess();
    }

    private static unsafe void OnRender(double deltaTime)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        List<Node> toDraw = new();
        toDraw.Add(root);

        while (toDraw.Count > 0)
        {
            Node current = toDraw[0];
            toDraw.RemoveAt(0);

            current.RunDraw(deltaTime);

            for (int i = current.children.Count - 1; i >= 0; i--)
                toDraw.Insert(0,  current.children[i]);
            

        }
    
        gl.Disable(EnableCap.ScissorTest);
    }

    private static void OnResize(Vector2D<int> size)
    {
        gl.Viewport(size);
    }

}
