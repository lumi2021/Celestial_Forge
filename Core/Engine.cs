using System.Diagnostics;
using GameEngine.Editor;
using GameEngine.Util.Core;
using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;


namespace GameEngine.Core;

public class Engine
{

    public static IWindow window = null!;
    public static GL gl = null!;

    public static ProjectSettings projectSettings = new();

    public static NodeRoot root = new();

    #region gl info

    public readonly int gl_MaxTextureUnits;

    #endregion

    public Engine()
    {
        /* CREATE MAIN WINDOW AND GL CONTEXT */
        var mainWin = new Util.Nodes.Window();
        window = mainWin.window;
        root.AddAsChild(mainWin);

        // get GL info //
        gl_MaxTextureUnits = gl.GetInteger(GLEnum.MaxTextureImageUnits);

        /* configurate project settings */
        projectSettings.projectLoaded = true;
        projectSettings.projectPath = @"C:/Users/Leonardo/Desktop/pessoal/game engine test project/";
        //projectSettings.projectPath = @"C:/Users/Leo/Documents/projetos/myEngine/";
        projectSettings.entryScene = @"res://testScene.sce";

        projectSettings.canvasDefaultSize = new(800, 600);

        CascadingStyleSheet.Load("Data/Styles/Editor.css");

        /* START EDITOR */
        _ = new EditorMain(projectSettings, mainWin);

        /* START RUN */
        try {
            Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something goes very wrong!");
            Console.WriteLine("Exeption:\n{0}", ex);
            Console.ReadKey();
        }

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
            if (win.IsInitialized)
            {
                DrawService.GlBinded_ShaderProgram = -1;
                win.DoEvents();
                win.DoUpdate();
                win.DoRender();

                if (win != WindowService.mainWindow)
                win.SwapBuffers();
            }

            if (WindowService.mainWindow != null && !WindowService.mainWindow.IsClosing)
            WindowService.mainWindow.SwapBuffers();

            WindowService.CallProcess();
            ResourceHeap.CallProcess();

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
