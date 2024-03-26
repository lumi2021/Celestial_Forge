using System.Diagnostics;
using GameEngine.Util.Core;
using GameEngine.Util.Nodes;
using GameEngineEditor.Editor;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace GameEngine.Core;

public class Engine
{

    public static GL gl = null!;

    public static ProjectSettings projectSettings = new();

    private static readonly NodeRoot _root = new();
    public static NodeRoot NodeRoot => _root;

    #region gl info

    public readonly int gl_MaxTextureUnits;

    #endregion

    public Engine()
    {
        /* CREATE MAIN WINDOW AND GL CONTEXT */
        var mainWin = new Util.Nodes.Window();
        _root.AddAsChild(mainWin);

        // get GL info //
        gl_MaxTextureUnits = gl.GetInteger(GLEnum.MaxTextureImageUnits);

        /* configurate project settings */
        projectSettings.projectLoaded = true;
        //projectSettings.projectPath = @"C:/Users/Leonardo/Desktop/pessoal/game engine test project/";
        projectSettings.projectPath = @"C:/Users/Leo/Documents/projetos/myEngine/";

        projectSettings.entryScene = @"res://testScene.sce";
        projectSettings.canvasDefaultSize = new(800, 600);

        /* START EDITOR */
        Editor.StartEditor(projectSettings, mainWin);

        /* START RUN */
        Run();
        
        /* END PROGRAM */
        _root.Free();
        gl.Dispose();
        //Glfw.GetApi().Dispose();
    }

    private void Run()
    {
        /* GAME LOOP PROCESS */

        Stopwatch frameTime = new();
        Stopwatch stopwatch = new();
        frameTime.Start();
        stopwatch.Start();
        List<double> fpsHistory = [];

        while (WindowService.mainWindow != null && !WindowService.mainWindow.IsClosing)
        {

            foreach (var win in WindowService.windows.ToArray())
            if (win.IsInitialized)
            {
                
                try {

                    DrawService.GlBinded_ShaderProgram = -1;

                    win.DoEvents();
                    win.DoUpdate();
                    win.DoRender();

                    if (win != WindowService.mainWindow)
                    win.SwapBuffers();
                
                }
                catch (Exception ex)
                {
                    var oldConsoleCol = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkRed;

                    Console.WriteLine("\nSomething goes wrong!");

                    Console.ForegroundColor = oldConsoleCol;

                    Console.WriteLine($"{ex.GetType()}:");
                    Console.WriteLine($"\"{ex.Message}\"");
                    Console.WriteLine($"Stack Trace: \n{ex.StackTrace}");
                    Console.Beep();
                }

            }

            if (WindowService.mainWindow != null && !WindowService.mainWindow.IsClosing)
            {
                WindowService.mainWindow.MakeCurrent();
                WindowService.mainWindow.SwapBuffers();
            }

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

                var fpsValue = fpsHistory.ToArray().Average();
                var memValue = Process.GetCurrentProcess().WorkingSet64 / (1024.0 * 1024.0);

                Console.Title = $"fps: {fpsValue : 0.00}, {memValue : 0.00} Mb";
                fpsHistory.Clear();
            }

        }
    }

}
