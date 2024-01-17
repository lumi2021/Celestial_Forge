using System.Reflection;
using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Core;
using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using Silk.NET.Windowing;
using Window = GameEngine.Util.Nodes.Window;

namespace GameEngine.Editor;

public class EditorMain
{

    private ProjectSettings projectSettings;
    private Window mainWindow;

    /* IMPORTANT NODES */
    private Node? editorRoot;
    private TreeGraph? filesList;
    private TreeGraph? nodesList;

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
        mainWindow.Title = "Celestial Forge";

        /* INSTANTIATE EDITOR */
        var scene = PackagedScene.Load("Data/Screens/editor.json")!.Instantiate();
        mainWindow.AddAsChild(scene);
        editorRoot = scene;

        /* INSTANTIATE AND CONFIGURATE FILE MANANGER */
        #region
        var filesSection = scene.GetChild("Main/LeftPannel/FileMananger");

        filesList = new TreeGraph() { ClipChildren = true };
        filesSection!.AddAsChild(filesList);
        
        var txtFile = new SvgTexture() { Filter = false }; txtFile.LoadFromFile("Assets/Icons/Files/textFile.svg", 20, 20);
        var cFolder = new SvgTexture() { Filter = false }; cFolder.LoadFromFile("Assets/Icons/Files/closedFolder.svg", 20, 20);
        var eFolder = new SvgTexture() { Filter = false }; eFolder.LoadFromFile("Assets/Icons/Files/emptyFolder.svg", 20, 20);
        var unkFile = new SvgTexture() { Filter = false }; unkFile.LoadFromFile("Assets/Icons/Files/unknowFile.svg", 20, 20);
        var anvilWk = new SvgTexture() { Filter = false }; anvilWk.LoadFromFile("Assets/Icons/Files/AnvilKey.svg", 20, 20);
        var sceFile = new SvgTexture() { Filter = false }; sceFile.LoadFromFile("Assets/Icons/Files/scene.svg", 20, 20);

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

        nodesList = new TreeGraph();
        nodesSection!.AddAsChild(nodesList);


        var sb = new ScrollBar()
        {
            anchor = NodeUI.ANCHOR.TOP_RIGHT,
            sizePercent = new(0, 1),
            sizePixels = new(15, 0)
        };
        nodesSection.AddAsChild(sb);
        sb!.target = nodesList;
        #endregion

        /* CONFIGURATE BUTTONS */
        var runButton = scene.GetChild("TopBar/RunButton") as Button;
        runButton?.OnPressed.Connect(RunButtonPressed);
    
        //LoadSceneInEditor("res://testScene.sce");

    }

    private void RunButtonPressed(object? from, dynamic[]? args)
    {
        RunGame();
    }
    private void RunGame()
    {
        var gameWindow = new Window() {
            Size = (Vector2<uint>) projectSettings.canvasDefaultSize
        };

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
    
    private void OnNodeClicked(object? from, dynamic[]? args)
    {
        var item = from as TreeGraph.TreeGraphItem;
        var node = item!.data["NodeRef"] as Node;

        LoadInspectorInformation(node!);

    }

    private void LoadSceneInEditor(string scenePath)
    {
        var viewport = editorRoot!.GetChild("Main/Center/Viewport/ViewportContainer") as NodeUI;

        viewport!.sizePixels = projectSettings.canvasDefaultSize;

        nodesList!.ClearGraph();
        viewport!.children.Clear();
        
        var scene = PackagedScene.Load(scenePath)!.Instantiate();
        viewport!.AddAsChild(scene);
        

        /* LOAD NODES LIST */
        List<KeyValuePair<string, Node>> ToList = new();
        foreach (var i in scene.children) ToList.Add(new("", i));

        Dictionary<string, Texture> IconsBuffer = new();

        while ( ToList.Count > 0 )
        {
            var keyValue = ToList.Unqueue();
            var path = keyValue.Key;
            var node = keyValue.Value;

            Texture nodeIcon;
            if (IconsBuffer.ContainsKey(node.GetType().Name))
                nodeIcon = IconsBuffer[node.GetType().Name];
            else
            {
                var nTexture = new SvgTexture() { Filter = false };
                nTexture.LoadFromFile("Assets/icons/Nodes/" + node.GetType().Name + ".svg", 20, 20);
                IconsBuffer.Add(node.GetType().Name, nTexture);
                nodeIcon = nTexture;
            }

            var item = nodesList!.AddItem(path, node.name, nodeIcon);
            item!.data.Add("NodeRef", node);
            item!.OnClick.Connect(OnNodeClicked);

            for (int i = node.children.Count-1; i >= 0 ; i--)
                ToList.Insert(0, new(path+"/"+node.name, node.children[i]));
        }

        Texture rootIcon;
        if (IconsBuffer.ContainsKey(scene.GetType().Name))
            rootIcon = IconsBuffer[scene.GetType().Name];
        else
        {
            var nTexture = new SvgTexture() { Filter = false };
            nTexture.LoadFromFile("Assets/icons/Nodes/" + scene.GetType().Name + ".svg", 20, 20);
            IconsBuffer.Add(scene.GetType().Name, nTexture);
            rootIcon = nTexture;
        }

        nodesList!.Root.Name = scene.name;
        nodesList!.Root.Icon = rootIcon;

        nodesList!.Root.data.Add("NodeRef", scene);
        nodesList!.Root.OnClick.Connect(OnNodeClicked);
    }

    private void LoadInspectorInformation(Node node)
    {
        Type nodeType = node.GetType();

        var inspecContainer = editorRoot!.GetChild("Main/RightPannel/Inspector/InspectorContainer")! as NodeUI;
        inspecContainer!.children.Clear();

        Type currentType = nodeType;

        int itemPos = 0;

        while (currentType != typeof(object))
        {
            var item = CreateTitleItem(currentType.Name);
            item.positionPixels.Y = itemPos;
            inspecContainer.AddAsChild(item);
            itemPos += (int) item.Size.Y;

            var members = currentType.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance |
            BindingFlags.Public).Where(e => Attribute.IsDefined(e, typeof(Inspect)));

            foreach (var i in members)
            {  
                string value = "";
                if (i is FieldInfo @fiMember)
                    value = @fiMember.GetValue(node)?.ToString() ?? "<null>";
                else if (i is PropertyInfo @piMember)
                    value = @piMember.GetValue(node)?.ToString() ?? "<null>";

                var field = CreateVariableSetterItem(i.Name.Pascal2Tittle(), node, i);
                field.positionPixels.Y = itemPos;
                field.positionPixels.X = 10;
                field.sizePixels.X = -10;
                inspecContainer.AddAsChild(field);
                itemPos += (int) field.Size.Y;
            }

            currentType = currentType.BaseType!;
            itemPos += 5;
        }
    }

    #region really random stuff

    private static Pannel CreateTitleItem(string title)
    {
        var panel = new Pannel()
        {
            sizePercent = new(1, 0),
            sizePixels = new(0, 25)
        };
        var label = new TextField()
        {
            Text = title,
            anchor = NodeUI.ANCHOR.CENTER_CENTER
        };
        panel.AddAsChild(label);

        return panel;
    }

    private static NodeUI CreateVariableSetterItem(string title, object obj, MemberInfo memberInfo)
    {
        var fieldInfo = memberInfo as FieldInfo;
        var properInfo = memberInfo as PropertyInfo;

        Type fieldType = fieldInfo?.FieldType ?? properInfo!.PropertyType;
        if (fieldType.IsGenericType) fieldType = fieldType.GetGenericTypeDefinition();

        var container = new NodeUI()
        {
            sizePercent = new(1, 0),
            sizePixels = new(0, 25),
        };
        var label = new TextField()
        {
            Text = title,
            sizePercent = new(0.5f, 1),
            verticalAligin = TextField.Aligin.Center,
            anchor = NodeUI.ANCHOR.TOP_LEFT,
            Color = new(255, 255, 255),
        };
        
        container.AddAsChild(label);

        if (fieldType == typeof(string))
        {
            string value = (string) (fieldInfo?.GetValue(obj) ?? properInfo!.GetValue(obj))!;

            var fieldContainer = new Pannel()
            {
                BackgroundColor = new(149, 173, 190),
                sizePercent = new(0.5f, 1),
                anchor = NodeUI.ANCHOR.TOP_RIGHT
            };
            var field = new WriteTextField()
            {
                Text = value,
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                Color = new(0, 0, 0)
            };

            fieldContainer.AddAsChild(field);
            container.AddAsChild(fieldContainer);
        }

        if (fieldType == typeof(bool))
        {
            bool value = (bool) (fieldInfo?.GetValue(obj) ?? properInfo!.GetValue(obj))!;

            var checkbox = new Pannel()
            {
                anchor = NodeUI.ANCHOR.CENTER_RIGHT,
                sizePixels = new(16, 16),
                sizePercent = new(0, 0),
                positionPixels = new(8, 0),
                positionPercent = new(-0.25f, 0),
                BackgroundColor = value ? new(0, 0, 255) : new(0, 0, 0)
            };

            container.AddAsChild(checkbox);
        }
        
        else if (fieldType == typeof(Vector2<>).GetGenericTypeDefinition())
        {
            dynamic value = fieldInfo?.GetValue(obj) ?? properInfo!.GetValue(obj)!;

            container.sizePixels.Y = 50;
            label.sizePercent.Y = 0.5f;

            var fieldContainer1 = new Pannel()
            {
                BackgroundColor = new(149, 173, 190),
                sizePercent = new(0.5f, 0),
                sizePixels = new(0, 25),
                anchor = NodeUI.ANCHOR.TOP_RIGHT
            };
            var fieldContainer2 = new Pannel()
            {
                BackgroundColor = new(149, 173, 190),
                sizePercent = new(0.5f, 0),
                positionPercent = new(0f, 0.5f),
                sizePixels = new(0, 25),
                anchor = NodeUI.ANCHOR.TOP_RIGHT
            };
            
            var field1 = new WriteTextField()
            {
                Text = "" + value.X,
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                Color = new(0, 0, 0)
            };
            var field2 = new WriteTextField()
            {
                Text = "" + value.Y,
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                Color = new(0, 0, 0)
            };

            fieldContainer1.AddAsChild(field1);
            fieldContainer2.AddAsChild(field2);
            container.AddAsChild(fieldContainer1);
            container.AddAsChild(fieldContainer2);
        }

        return container;
    }

    #endregion

}
