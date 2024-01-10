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

    private Node? editorRoot;

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
        editorRoot = scene;

        /* INSTANTIATE AND CONFIGURATE FILE MANANGER */
        #region
        var filesSection = scene.GetChild("Main/LeftPannel/FileMananger");

        var filesList = new TreeGraph() { ClipChildren = true };
        filesSection!.AddAsChild(filesList);
        
        var txtFile = new SvgTexture(); txtFile.LoadFromFile("Assets/Icons/textFile.svg", 50, 50);
        var cFolder = new SvgTexture(); cFolder.LoadFromFile("Assets/Icons/closedFolder.svg", 50, 50);
        var eFolder = new SvgTexture(); eFolder.LoadFromFile("Assets/Icons/emptyFolder.svg", 50, 50);
        var unkFile = new SvgTexture(); unkFile.LoadFromFile("Assets/Icons/unknowFile.svg", 50, 50);
        var anvilWk = new SvgTexture(); anvilWk.LoadFromFile("Assets/Icons/AnvilKey.svg", 50, 50);
        var sceFile = new SvgTexture(); sceFile.LoadFromFile("Assets/Icons/scene.svg", 50, 50);

        filesList.Root.Icon = cFolder;
        filesList.Root.Name = "res://";

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
            var type = i.Extension != "" ? i.Extension : "folder";

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

            var item = filesList.AddItem(path, i.Name, iconImage);
            item!.Collapsed = type == "folder";
            item!.data.Add("type", type);
            item!.OnClick.Connect(OnFileClicked);
        }
        #endregion

        /* INSTANTIATE AND CONFIGURATE NODE MANANGER */
        #region

        var nodesSection = scene.GetChild("Main/RightPannel/NodeMananger");

        var nodesList = new TreeGraph() { ClipChildren = true };
        nodesSection!.AddAsChild(nodesList);



        #endregion

        /* CONFIGURATE BUTTONS */
        var runButton = scene.GetChild("TopBar/RunButton") as Button;
        runButton?.OnPressed.Connect(RunButtonPressed);
    
        LoadSceneInEditor("res://testScene.sce");

    }

    private void RunButtonPressed(object? from, dynamic[]? args)
    {
        RunGame();
    }
    private void RunGame()
    {
        var gameWindow = new Window();

        mainWindow.AddAsChild(gameWindow);

        var gameScene = PackagedScene.Load(projectSettings.entryScene)!.Instantiate();
        gameWindow.AddAsChild(gameScene);
    }


    private void OnFileClicked(object? from, dynamic[]? args)
    {
        var item = from as TreeGraph.TreeGraphItem;

        var path = "res://" + item!.Path[7..];

        if (item!.data["type"] == "folder")
            item.Collapsed = !item.Collapsed;
        
        else
        {
            switch (item!.data["type"])
            {
                case ".sce":
                    LoadSceneInEditor(path); break;
            }
        }
    }

    private void LoadSceneInEditor(string scenePath)
    {

        var viewport = editorRoot!.GetChild("Main/Center/Viewport");

        viewport!.children = new();
        
        var packScene = PackagedScene.Load(scenePath)!.Instantiate();
        viewport!.AddAsChild(packScene);

    }

}
