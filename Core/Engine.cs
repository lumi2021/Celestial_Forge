using System.Diagnostics;
using GameEngine.Editor;
using GameEngine.Util;
using GameEngine.Util.Core;
using GameEngine.Util.Nodes;
using OpenGL;

namespace GameEngine.Core;

public class Engine
{

    #pragma warning disable CS8618
    public static IWindow window;
    public static Gl gl;
    #pragma warning restore

    public static ProjectSettings projectSettings = new();

    public static NodeRoot root = new();

    #region gl info

    public readonly int gl_MaxTextureUnits;

    #endregion

    public Engine()
    {
        var mainWin = new Util.Nodes.Window();
        window = mainWin.window;
        root.AddAsChild(mainWin);

        mainWin.State = WindowState.Maximized;
        mainWin.Title = "Game Engine";
        gl.ClearColor(1f, 1f, 1f, 1f);

        // get GL info //
        gl.GetInteger(GetPName.MaxTextureImageUnits, out gl_MaxTextureUnits);

        // configurations //
        gl.Enable(EnableCap.Multisample);
        gl.Enable(EnableCap.ScissorTest);
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _ = new EditorMain(mainWin, projectSettings);

        /* START RUN */
        Run();

        /* END PROGRAM */
        root.Free();
        gl.Dispose();
    }

    private void Run()
    {
        /* GAME LOOP PROCESS */

        Stopwatch frameTime = new();
        Stopwatch stopwatch = new();
        frameTime.Start();
        stopwatch.Start();
        List<double> fpsHistory = new();

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

            WindowService.CallProcess();

            /* FPS COUNTER */

            double elapsedSeconds = frameTime.Elapsed.TotalSeconds;
            double fps = 1.0 / elapsedSeconds;
            fpsHistory.Add(fps);
            frameTime.Restart();

            if (stopwatch.Elapsed.TotalSeconds >= 1)
            {
                stopwatch.Restart();
                Console.Title = "fps: " + Math.Round(fpsHistory.ToArray().Average());
                fpsHistory.Clear();
            }
        }
    }

}
