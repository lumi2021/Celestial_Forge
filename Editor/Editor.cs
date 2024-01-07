using GameEngine.Core;
using GameEngine.Util.Core;
using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;

namespace GameEngine.Editor;

public class EditorMain
{
    private Window window;
    private ProjectSettings projectSettings;

    public EditorMain(Window mainWindow, ProjectSettings projSettings)
    {
        projectSettings = projSettings;
        window = mainWindow;
        StartEditor();
    }

    private void StartEditor()
    {
        /* configurate project settings */
        projectSettings.projectLoaded = true;
        projectSettings.projectPath = @"C:/Users/Leo/Documents/projetos/myEngine/";
        projectSettings.entryPointPath = @"res://testScene.sce";
        
        var scene = PackagedScene.Load("Data/Screens/editor.json")!.Instantiate();
        window.AddAsChild(scene);

        /* load project directory */
        var fileMan = scene.GetChild("Main/LeftPannel/FileMananger");

        var a = new TreeGraph() { ClipChildren = true };
        fileMan!.AddAsChild(a);

        var b = new SvgTexture(); b.LoadFromFile("Assets/Icons/textFile.svg", 200, 200);
        var c = new SvgTexture(); c.LoadFromFile("Assets/Icons/closedFolder.svg", 200, 200);
        var f = new SvgTexture(); f.LoadFromFile("Assets/Icons/emptyFolder.svg", 200, 200);
        var d = new SvgTexture(); d.LoadFromFile("Assets/Icons/unknowFile.svg", 200, 200);
        var e = new SvgTexture(); e.LoadFromFile("Assets/Icons/anvilKey.svg", 200, 200);

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
            item!.OnClick.Connect(OnClickItem);
        }

        StartGame();

    }

    private void StartGame()
    {
        var gameWindow = new Window();
        var scene = PackagedScene.Load(projectSettings.entryPointPath)!.Instantiate();
        gameWindow.AddAsChild(scene);

        window.AddAsChild(window);
    }

    /* file mananger actions */
    private void OnClickItem(object? from, dynamic[]? args)
    {
        var item = from as TreeGraph.TreeGraphItem;

        if (item!.data["type"] == "file")
        {
        }
        else {
            item.Collapsed = !item.Collapsed;
        }
    }

}