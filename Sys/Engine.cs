using System.Diagnostics;
using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
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
        var mainWin = new Util.Nodes.Window();
        window = mainWin.window;
        root.AddAsChild(mainWin);

        mainWin.State = WindowState.Maximized;
        mainWin.Title = "Game Engine";
        gl.ClearColor(1f, 1f, 1f, 1f);

        gl.Enable(EnableCap.Multisample);
        gl.Enable(EnableCap.ScissorTest);
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        var scene = PackagedScene.Load("Data/Screens/editor.json").Instantiate();
        mainWin.AddAsChild(scene);

        var fileMan = scene.GetChild("Main/LeftPannel/FileMananger");

        var a = new TreeGraph() { ClipChildren = true };
        fileMan!.AddAsChild(a);

        var b = new SvgTexture();
        var c = new SvgTexture();
        b.LoadFromFile("Assets/Icons/script.svg", 200, 200);
        c.LoadFromFile("Assets/Icons/closedFolder.svg", 200, 200);

        a.AddItem("", "folder1", c);
        a.AddItem("", "folder2", c);

        a.AddItem("folder1", "script", b);
        a.AddItem("folder1", "script2", b);

        a.AddItem("folder1", "folder", c);
        a.AddItem("folder1/folder", "script", b);
        a.AddItem("folder1/folder", "script2", b);

        a.AddItem("folder2", "script", b);
        a.AddItem("folder2", "script2", b);

        /*
        START RUN
        */
        Run();

        // End program
        root.Free();
        gl.Dispose();
    }

    private void Run()
    {
        /*
        GAME LOOP PROCESS
        */

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        while (WindowService.mainWindow != null && !WindowService.mainWindow.IsClosing)
        {
            foreach (var win in WindowService.windows.ToArray())
            {
                if (win.IsInitialized)
                {
                    win.DoEvents();
                    win.DoUpdate();
                    win.DoRender();
                }
            }

            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            double fps = 1.0 / elapsedSeconds;
            stopwatch.Restart();

            WindowService.CallProcess();

            Console.WriteLine(fps);
        }
    }

}
