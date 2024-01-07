using System.Diagnostics;
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

    private TextField? textField;

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
        gl_MaxTextureUnits = gl.GetInteger(GLEnum.MaxTextureImageUnits);

        // configurations //
        gl.Enable(EnableCap.Multisample);
        gl.Enable(EnableCap.ScissorTest);
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        /* configurate project settings */
        projectSettings.projectLoaded = true;
        projectSettings.projectPath = @"C:/Users/Leo/Documents/projetos/myEngine/";
        
        var scene = PackagedScene.Load("Data/Screens/editor.json")!.Instantiate();
        mainWin.AddAsChild(scene);

        /* // test here // */

        var fileMan = scene.GetChild("Main/LeftPannel/FileMananger");
        textField = scene.GetChild("Main/Center/Viewport/TextField") as TextField;

        var a = new TreeGraph() { ClipChildren = true };
        fileMan!.AddAsChild(a);

        var b = new SvgTexture(); b.LoadFromFile("Assets/Icons/textFile.svg", 200, 200);
        var c = new SvgTexture(); c.LoadFromFile("Assets/Icons/closedFolder.svg", 200, 200);
        var f = new SvgTexture(); f.LoadFromFile("Assets/Icons/emptyFolder.svg", 200, 200);
        var d = new SvgTexture(); d.LoadFromFile("Assets/Icons/unknowFile.svg", 200, 200);
        var e = new SvgTexture(); e.LoadFromFile("Assets/Icons/AnvilKey.svg", 200, 200);

        a.Root.Icon = c;
        a.Root.Name = "res://";


        List<FileSystemInfo> itens = new();
        itens.AddRange(FileService.GetDirectory("res://"));
        itens.Sort((a, b) => {
            if (a.Extension == "" && b.Extension != "") return -1;
            else if (a.Extension != "" && b.Extension == "") return 1;
            else return 0;
        });

        while (itens.Count > 0)
        {
            var i = itens[0];
            itens.RemoveAt(0);
            SvgTexture iconImage = d;
            var type = "file";

            if (i.Extension == "")
            {
                var filesInThisDirectory = FileService.GetDirectory(i.FullName);
                iconImage = filesInThisDirectory.Length == 0 ? f : c;
                itens.AddRange(filesInThisDirectory);
                itens.Sort((a, b) => {
                    if (a.Extension == "" && b.Extension != "") return -1;
                    else if (a.Extension != "" && b.Extension == "") return 1;
                    else return 0;
                });
                type = "folder";
            }
            else if (i.Extension == ".txt")
                iconImage = b;
            
            else if (i.Extension == ".forgec")
                iconImage = e;

            var path = FileService.GetProjRelativePath(i.FullName);
            path = path[6..][..^i.Name.Length];

            var item = a.AddItem( path, i.Name, iconImage );
            item!.Collapsed = type == "folder";
            item!.data.Add("type", type);
            item!.OnClick.Connect(OnClick);
        }

        /* // test here // */

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

    private void OnClick(object? from, dynamic[]? args)
    {
        var item = from as TreeGraph.TreeGraphItem;

        if (item!.data["type"] == "file")
        {
            var a = "res://" + item!.Path[7..];
            textField!.Text = new FileReference(a).ReadAllFile();
        }
        else {
            item.Collapsed = !item.Collapsed;
        }
    }

}
