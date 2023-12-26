using System.ComponentModel;
using System.Diagnostics;
using GameEngine.Util.Core;
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

    public static ProjectSettings projectSettings = new();

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

        /* configurate project settings */
        projectSettings.projectLoaded = true;
        projectSettings.projectPath = @"C:/Users/Leo/Documents/projetos/myEngine/";

        
        var scene = PackagedScene.Load("Data/Screens/editor.json")!.Instantiate();
        mainWin.AddAsChild(scene);

        var fileMan = scene.GetChild("Main/LeftPannel/FileMananger");

        var a = new TreeGraph() { ClipChildren = true };
        fileMan!.AddAsChild(a);

        var b = new SvgTexture();
        var c = new SvgTexture();
        var d = new SvgTexture();
        b.LoadFromFile("Assets/Icons/textFile.svg", 200, 200);
        c.LoadFromFile("Assets/Icons/closedFolder.svg", 200, 200);
        d.LoadFromFile("Assets/Icons/unknowFile.svg", 200, 200);

        /* // test here // */

        List<FileSystemInfo> itens = new();
        itens.AddRange(FileService.GetDirectory("res://"));

        while (itens.Count > 0)
        {
            var i = itens[0];
            itens.RemoveAt(0);

            SvgTexture iconImage = d;

            if (i.Extension == "")
            {
                iconImage = c;
                itens.AddRange(FileService.GetDirectory(i.FullName));
            }
            else if (i.Extension == ".txt")
                iconImage = b;

            var path = FileService.GetProjRelativePath(i.FullName);
            path = path[6..][..^i.Name.Length];

            Console.WriteLine(path);

            a.AddItem(
                path,
                i.Name,
                iconImage
            );
        }

        /* // test here // */

        /*
        a.AddItem("", "folder1", c);
        a.AddItem("", "folder2", c);

        a.AddItem("folder1", "script", b);
        a.AddItem("folder1", "script2", b);

        a.AddItem("folder1", "folder", c);
        a.AddItem("folder1/folder", "script", b);
        a.AddItem("folder1/folder", "script2", b);

        a.AddItem("folder2", "script", b);
        a.AddItem("folder2", "script2", b);
        */

        /* START RUN */
        Run();

         /* END PROGRAM */
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
