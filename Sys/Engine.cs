using GameEngine.Util.Nodes;
using GameEngine.Util.Values;
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
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


        #region nodes instnace

        var screen = new Pannel();
        root.AddAsChild(screen);
        screen.backgroundColor = new(0.06f, 0f, 0.2f);

        var topBar = new Pannel();
        topBar.backgroundColor = new(80, 58, 101);
        topBar.sizePercent.Y = 0f;
        topBar.sizePixels.Y = 48;
        topBar.anchor = NodeUI.ANCHOR.TOP_CENTER;

        var main = new NodeUI();
        main.sizePixels.Y = - 48 - 16;
        main.anchor = NodeUI.ANCHOR.BOTTOM_LEFT;


        var leftPannel = new Pannel();
        leftPannel.backgroundColor = new(80, 58, 101);
        leftPannel.sizePercent = new Vector2<float>(0.2f,1f);
        leftPannel.anchor = NodeUI.ANCHOR.BOTTOM_LEFT;

        var rightPannel = new Pannel();
        rightPannel.backgroundColor = new(80, 58, 101);
        rightPannel.sizePercent = new Vector2<float>(0.2f,1f);
        rightPannel.anchor = NodeUI.ANCHOR.BOTTOM_RIGHT;

        #region game & debug

        var center = new NodeUI();
        center.positionPercent.X = 0.2f;
        center.positionPixels.X = 16;
        center.sizePercent.X = 1f - 0.4f;
        center.sizePixels.X = -32;
        center.anchor = NodeUI.ANCHOR.BOTTOM_LEFT;

        var bottomBar = new Pannel();
        bottomBar.backgroundColor = new(60, 42, 77);
        bottomBar.sizePercent.Y = 0f;
        bottomBar.sizePixels.Y = 48;
        bottomBar.anchor = NodeUI.ANCHOR.BOTTOM_CENTER;

        var centerPannel = new Pannel();
        centerPannel.backgroundColor = new(255, 255, 255);
        centerPannel.sizePixels.Y = -48 - 16;
        centerPannel.anchor = NodeUI.ANCHOR.TOP_CENTER;

        #endregion

        var dragHandlerl = new DragHandler();
        dragHandlerl.positionPercent.X = 0.2f;
        dragHandlerl.nodeA = leftPannel;
        dragHandlerl.nodeB = center;
        dragHandlerl.nodeBSizeMin = 300;
        dragHandlerl.defaultColor = new(1f, 1f, 1f, 0f);
        dragHandlerl.holdingColor = new(1f, 1f, 1f, 0.3f);

        var dragHandlerr = new DragHandler();
        dragHandlerr.positionPercent.X = 0.8f;
        dragHandlerr.positionPixels.X = -16;
        dragHandlerr.nodeA = center;
        dragHandlerr.nodeB = rightPannel;
        dragHandlerr.nodeASizeMin = 300;
        dragHandlerr.defaultColor = new(1f, 1f, 1f, 0f);
        dragHandlerr.holdingColor = new(1f, 1f, 1f, 0.3f);


        screen.AddAsChild(topBar);
        screen.AddAsChild(main);

        main.AddAsChild(leftPannel);
        main.AddAsChild(rightPannel);
        main.AddAsChild(center);

        main.AddAsChild(dragHandlerl);
        main.AddAsChild(dragHandlerr);

        center.AddAsChild(bottomBar);
        center.AddAsChild(centerPannel);

        #endregion
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
        gl.Clear(ClearBufferMask.ColorBufferBit);

        List<Node> toDraw = new() {root};

        while (toDraw.Count > 0)
        {
            var children = toDraw[0].children;
            toDraw[0].RunDraw(deltaTime);
            toDraw.RemoveAt(0);
            toDraw.AddRange(children);
        }
    
        gl.Disable(EnableCap.ScissorTest);
    }

    private static void OnResize(Vector2D<int> size)
    {
        gl.Viewport(size);
    }

}
