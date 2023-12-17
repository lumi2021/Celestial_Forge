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
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);;

        var scene = PackagedScene.Load("Data/Screens/editor.json").Instantiate();
        mainWin.AddAsChild(scene);

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
