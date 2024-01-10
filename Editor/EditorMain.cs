using GameEngine.Core;
using GameEngine.Util.Core;
using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
using Silk.NET.Windowing;
using Window = GameEngine.Util.Nodes.Window;

namespace GameEngine.Editor;

public class EditorMain
{

    private ProjectSettings projectSettings;
    private Window mainWindow;

    public EditorMain(ProjectSettings settings, Window mainWin)
    {
        projectSettings = settings;
        mainWindow = mainWin;

        CreateEditor();
    }

    private void CreateEditor()
    {
        /* CONFIGURATE WINDOW */
        mainWindow.State = WindowState.Maximized;
        mainWindow.Title = "Game Engine";

        /* INSTANTIATE EDITOR */
        var scene = PackagedScene.Load("Data/Screens/editor.json")!.Instantiate();
        mainWindow.AddAsChild(scene);

        /* INSTANTIATE AND CONFIGURATE FILE MANANGER */
        var fileMan = scene.GetChild("Main/LeftPannel/FileMananger");

        var a = new TreeGraph() { ClipChildren = true };
        fileMan!.AddAsChild(a);
        
        var txtFile = new SvgTexture(); txtFile.LoadFromFile("Assets/Icons/textFile.svg", 200, 200);
        var cFolder = new SvgTexture(); cFolder.LoadFromFile("Assets/Icons/closedFolder.svg", 200, 200);
        var eFolder = new SvgTexture(); eFolder.LoadFromFile("Assets/Icons/emptyFolder.svg", 200, 200);
        var unkFile = new SvgTexture(); unkFile.LoadFromFile("Assets/Icons/unknowFile.svg", 200, 200);
        var anvilWk = new SvgTexture(); anvilWk.LoadFromFile("Assets/Icons/AnvilKey.svg", 200, 200);
        var sceFile = new SvgTexture(); sceFile.LoadFromFile("Assets/Icons/scene.svg", 200, 200);

        a.Root.Icon = cFolder;
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
            SvgTexture iconImage = unkFile;
            var type = "file";

            if (i.Extension == "")
            {
                var filesInThisDirectory = FileService.GetDirectory(i.FullName);
                iconImage = filesInThisDirectory.Length == 0 ? eFolder : cFolder;
                itens.AddRange(filesInThisDirectory);
                itens.Sort((a, b) => {
                    if (a.Extension == "" && b.Extension != "") return -1;
                    else if (a.Extension != "" && b.Extension == "") return 1;
                    else return 0;
                });
                type = "folder";
            }
            else if (i.Extension == ".txt")
                iconImage = txtFile;

            else if (i.Extension == ".sce")
                iconImage = sceFile;

            else if (i.Extension == ".forgec")
                iconImage = anvilWk;

            var path = FileService.GetProjRelativePath(i.FullName);
            path = path[6..][..^i.Name.Length];

            var item = a.AddItem(path, i.Name, iconImage);
            item!.Collapsed = type == "folder";
            item!.data.Add("type", type);
            //item!.OnClick.Connect(OnClick);
        }

        RunGame();
    }

    private void RunGame()
    {
        var gameWindow = new Window();

        mainWindow.AddAsChild(gameWindow);

        var gameScene = PackagedScene.Load(projectSettings.entryScene)!.Instantiate();
        gameWindow.AddAsChild(gameScene);
    }

}

