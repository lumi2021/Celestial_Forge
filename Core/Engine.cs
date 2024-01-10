using System.Diagnostics;
using GameEngine.Editor;
using GameEngine.Util;
using GameEngine.Util.Core;
using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace GameEngine.Core;

public class Engine
{

    #pragma warning disable CS8618
    public static IWindow window;
    public static GL gl;
    #pragma warning restore

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

        // configurations //
        gl.Enable(EnableCap.Multisample);
        gl.Enable(EnableCap.ScissorTest);
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        /* configurate project settings */
        projectSettings.projectLoaded = true;
        //projectSettings.projectPath = @"C:/Users/Leonardo/Desktop/pessoal/game engine test project/";
        projectSettings.projectPath = @"C:/Users/Leo/Documents/projetos/myEngine/";
        projectSettings.entryScene = @"res://testScene.sce";

        /* START EDITOR */
        _ = new EditorMain(projectSettings, mainWin);

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
            if (win.IsInitialized)
            {
                DrawService.GlBinded_ShaderProgram = -1;
                win.DoEvents();
                win.DoUpdate();
                win.DoRender();
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
                Console.Title = "fps: " + Math.Round(fpsHistory.ToArray().Average());
                fpsHistory.Clear();
            }
        }
    }

    private void OnClick(object? from, dynamic[]? args)
    {
        var item = from as TreeGraph.TreeGraphItem;

        if (item!.data["type"] == "file")
        {
            var a = "res://" + item!.Path[7..];
            //textField!.Text = new FileReference(a).ReadAllFile();
        }
        else {
            item.Collapsed = !item.Collapsed;
        }
    }

}
