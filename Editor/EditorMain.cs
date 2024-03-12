using System.Reflection;
using GameEngine.Core;
using GameEngine.Util;
using GameEngine.Util.Attributes;
using GameEngine.Util.Core;
using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using Silk.NET.Windowing;
using GameEngine.Debugging;
using Window = GameEngine.Util.Nodes.Window;
using GameEngineEditor.EditorNodes;

namespace GameEngine.Editor;

public class EditorMain
{

    private ProjectSettings projectSettings;
    private readonly Window mainWindow;

    /* IMPORTANT NODES */
    private Node? editorRoot;
    private TreeGraph? filesList;
    private TreeGraph? nodesList;
    private Pannel? sceneViewport;
    private Pannel? textEditor;
    private NodeUI? console;

    private Viewport sceneEnviropment = null!;

    /* ETC */
    private int maintab = 0;
    //private int bottomtab = 0;

    private FileReference? fileBeingEdited = null;

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
        var scene = PackagedScene.Load("Data/Screens/mainEditor/editor.json")!.Instantiate();
        mainWindow.AddAsChild(scene);
        editorRoot = scene;

        /* CONFIGURATE MAIN SCREEN */
        #region

        sceneViewport = editorRoot!.GetChild("Main/Center/Main/Viewport") as Pannel;
        textEditor = editorRoot!.GetChild("Main/Center/Main/TextEditor") as Pannel;

        Button sceneBtn = (editorRoot!.GetChild("TopBar/MainOptions/SceneEditor") as Button)!;
        Button scriptBtn = (editorRoot!.GetChild("TopBar/MainOptions/ScriptEditor") as Button)!;

        sceneBtn.OnPressed.Connect((object? from, dynamic[]? args) => ChangeMainView(0));
        scriptBtn.OnPressed.Connect((object? from, dynamic[]? args) => ChangeMainView(1));

        //var textEditorField = textEditor!.GetChild("FileContent") as WriteTextField;
        var textSaveBtn = (textEditor!.GetChild("Toolbar/SaveBtn") as Button)!;
        textSaveBtn.OnPressed.Connect( (object? from, dynamic[]? args) => SaveOpenTextFile() );

        var textCompileBtn = (textEditor!.GetChild("Toolbar/CompileBtn") as Button)!;
        textCompileBtn.OnPressed.Connect( (object? from, dynamic[]? args) => CompileOpenTextFile() );

        var textEditorField = textEditor.GetChild("FileContentContainer/FileContent") as WriteTextField;
        textEditorField!.OnTextEdited.Connect((object? node, dynamic[]? args) => {
            textEditorField.colorsList = CSharpCompiler.Highlight(args![0]);
        });

        #endregion

        /* CONFIGURATE VIEWPORT */
        #region

        sceneEnviropment = new()
        {
            useContainerSize = true,
            ContainerSize = (Vector2<uint>)projectSettings.canvasDefaultSize,
            backgroundColor = new(50, 50, 100)
        };

        mainWindow.children.Insert(0, sceneEnviropment);
        sceneEnviropment.parent = mainWindow;

        ViewportContainer viewportContainer = new() { linkedViewport = sceneEnviropment };
        sceneViewport!.AddAsChild(viewportContainer);

        #endregion

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

        /* INSTANTIATE AND CONFIGURATE BOTTOM BAR */
        #region
        
        // tab buttons
        var bottomBar = editorRoot!.GetChild("Main/Center/BottomBar") as Pannel;

        var outputBtn = bottomBar!.GetChild("Tabs/OutputBtn") as Button;
        var errorsBtn = bottomBar!.GetChild("Tabs/ErrorsBtn") as Button;
        var monitorsBtn = bottomBar!.GetChild("Tabs/MonitorsBtn") as Button;

        var consoleTab = bottomBar!.GetChild("BottomBarWindow/ConsoleTab") as NodeUI;
        var errorsTab = bottomBar!.GetChild("BottomBarWindow/ErrorsTab") as NodeUI;
        var monitorsTab = bottomBar!.GetChild("BottomBarWindow/MonitorsTab") as NodeUI;


        outputBtn!.OnPressed.Connect((object? from, dynamic[]? args) => {
            //bottomtab = 0;
            consoleTab!.Visible = true;
            errorsTab!.Visible = false;
            monitorsTab!.Visible = false;
        });
        errorsBtn!.OnPressed.Connect((object? from, dynamic[]? args) => {
            //bottomtab = 1;
            consoleTab!.Visible = false;
            errorsTab!.Visible = true;
            monitorsTab!.Visible = false;
        });
        monitorsBtn!.OnPressed.Connect((object? from, dynamic[]? args) => {
            //bottomtab = 2;
            consoleTab!.Visible = false;
            errorsTab!.Visible = false;
            monitorsTab!.Visible = true;
        });
        

        // console
        console = bottomBar!.GetChild("BottomBarWindow/ConsoleTab/Console/ConsoleLog") as NodeUI;
        Debug.OnLogEvent += OnLog;

        #endregion

        /* CONFIGURATE BUTTONS */
        var runButton = scene.GetChild("TopBar/RunButton") as Button;
        runButton?.OnPressed.Connect(RunButtonPressed);

    }

    private void ChangeMainView(int to)
    {
        if (maintab == to) return;

        switch (to)
        {
            case 0:
                sceneViewport!.Show();
                textEditor!.Hide();
                break;

            case 1:
                sceneViewport!.Hide();
                textEditor!.Show();
                break;
        }
    
        maintab = to;
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

                default:
                    OpenTextFile(path); break;
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
        sceneEnviropment.FreeChildren();

        var cam = new SceneEditor2DCamera();
        sceneEnviropment.AddAsChild(cam);
        cam.Current = true;

        nodesList!.ClearGraph();
        
        var scene = PackagedScene.Load(scenePath)!.Instantiate();
        sceneEnviropment!.AddAsChild(scene);

        // LOAD NODES LIST //
        List<KeyValuePair<string, Node>> ToList = [];
        foreach (var i in scene.children) ToList.Add(new("", i));

        Dictionary<string, Texture> IconsBuffer = [];

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
                IconAttribute nodeIconAtrib = (IconAttribute)node.GetType().GetCustomAttribute(typeof(IconAttribute))!;
                nTexture.LoadFromFile(nodeIconAtrib.path, 20, 20);
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
            IconAttribute nodeIconAtrib = (IconAttribute)scene.GetType().GetCustomAttribute(typeof(IconAttribute))!;
            nTexture.LoadFromFile(nodeIconAtrib.path, 20, 20);
            IconsBuffer.Add(scene.GetType().Name, nTexture);
            rootIcon = nTexture;
        }

        nodesList!.Root.Name = scene.name;
        nodesList!.Root.Icon = rootIcon;

        nodesList!.Root.data.Add("NodeRef", scene);
        nodesList!.Root.OnClick.Connect(OnNodeClicked);
    
        ChangeMainView(0);
    }

    private void OpenTextFile(string filePath)
    {
        var textField = (textEditor!.GetChild("FileContentContainer/FileContent") as WriteTextField)!;

        var file = new FileReference(filePath);

        var content = file.ReadAllFile();

        textField.Text = content;

        fileBeingEdited = file;

        ChangeMainView(1);
    }
    private void SaveOpenTextFile()
    {
        var textField = (textEditor!.GetChild("FileContentContainer/FileContent") as WriteTextField)!;
        fileBeingEdited?.Write(textField.Text);
    }
    private void CompileOpenTextFile()
    {
        var textField = (textEditor!.GetChild("FileContentContainer/FileContent") as WriteTextField)!;
        var code = textField.Text;

        var csc = new CSharpCompiler();
        Type? scriptType = csc.Compile(code, fileBeingEdited!.Value.GlobalPath);

        if (scriptType != null)
        {
            object scriptInstance = Activator.CreateInstance(scriptType)!;
            
            MethodInfo executeMethod = scriptType.GetMethod("Execute")!;
            MethodInfo freeMethod = scriptType.GetMethod("Free")!;

            executeMethod.Invoke(scriptInstance, null);
            freeMethod.Invoke(scriptInstance, null);
        }
    }

    private void LoadInspectorInformation(Node node)
    {
        Type nodeType = node.GetType();

        var inspecContainer = editorRoot!.GetChild("Main/RightPannel/Inspector/InspectorContainer")! as NodeUI;
        inspecContainer!.FreeChildren();

        Type currentType = nodeType;

        int itemPos = 0;

        while (currentType != typeof(object))
        {
            var item = CreateTitleItem(currentType.Name);
            item.positionPixels.Y = itemPos;
            inspecContainer.AddAsChild(item);
            itemPos += (int) item.Size.Y;

            var members = currentType.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance |
            BindingFlags.Public).Where(e => Attribute.IsDefined(e, typeof(InspectAttribute)));

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

    private void OnLog(LogInfo log)
    {
        console!.AddAsChild(CreateLogItem(log));
        var p = 0;
        foreach (var i in console.children)
        if (i is NodeUI node)
        {
            node.positionPixels.Y = p;
            p += node.sizePixels.Y;
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
        Type[] fieldGenericArgs = [];
        if (fieldType.IsGenericType)
        {
            fieldGenericArgs = fieldType.GetGenericArguments();
            fieldType = fieldType.GetGenericTypeDefinition();
        }

        InspectAttribute inspectAtt = memberInfo.GetCustomAttribute<InspectAttribute>()!;

        var container = new NodeUI()
        {
            sizePercent = new(1, 0),
            sizePixels = new(0, 25),
            name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter"
        };
        var label = new TextField()
        {
            Text = title,
            sizePercent = new(0.5f, 0),
            sizePixels = new(0, 25),
            verticalAligin = TextField.Aligin.Center,
            anchor = NodeUI.ANCHOR.TOP_LEFT,
            Color = new(255, 255, 255),
            name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_label"
        };
        
        container.AddAsChild(label);

        if (fieldType == typeof(string))
        {
            string value = (string) (fieldInfo?.GetValue(obj) ?? properInfo!.GetValue(obj))!;

            var fieldContainer = new Pannel()
            {
                BackgroundColor = new(149, 173, 190),
                sizePercent = new(0.5f, 1),
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_container"
            };
            var field = new WriteTextField()
            {
                Text = value,
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                Color = new(0, 0, 0),
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_TextField"
            };

            field.OnTextEdited.Connect((object? from, dynamic[]? args) => {
                fieldInfo?.SetValue(obj, args![0]);
                properInfo?.SetValue(obj, args![0]);
            });

            if (inspectAtt.usage == InspectAttribute.Usage.multiline_text)
            {
                container.sizePixels.Y = 25 + 100;
                fieldContainer.sizePercent = new(1, 0);
                fieldContainer.sizePixels.Y = 100;
                fieldContainer.positionPixels.Y = 25;
                fieldContainer.positionPercent = new();
            }

            fieldContainer.AddAsChild(field);
            container.AddAsChild(fieldContainer);
        }

        else if (fieldType == typeof(bool))
        {
            bool value = (bool) (fieldInfo?.GetValue(obj) ?? properInfo!.GetValue(obj))!;

            var texture_check = new SvgTexture(); texture_check.LoadFromFile("Assets/icons/Misc/checkbox-checked.svg", 20, 20);
            var texture_uncheck = new SvgTexture(); texture_uncheck.LoadFromFile("Assets/icons/Misc/checkbox-unchecked.svg", 20, 20);

            var fieldContainer = new NodeUI()
            {
                sizePercent = new(0.5f, 1),
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_container"
            };
            var checkbox = new Checkbox()
            {
                sizePixels = new(20, 20),
                sizePercent = new(0, 0),
                positionPixels = new(2, 0),
                anchor = NodeUI.ANCHOR.CENTER_LEFT,
                value = value,
                mouseFilter = NodeUI.MouseFilter.Ignore,
                actived_texture = texture_check,
                unactived_texture = texture_uncheck,
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_checkbox"
            };
            var text = new TextField()
            {
                Text = value ? "enabled" : "disabled",
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                verticalAligin = TextField.Aligin.Center,
                sizePixels = new(-28, 0),
                Color = new(255, 255, 255),
                mouseFilter = NodeUI.MouseFilter.Ignore,
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_value_label"
            };

            fieldContainer.onClick.Connect((object? from, dynamic[]? args) =>
            {
                bool value = !(bool) (fieldInfo?.GetValue(obj) ?? properInfo!.GetValue(obj))!;
                checkbox.value = value;
                text.Text = value ? "enabled" : "disabled";

                fieldInfo?.SetValue(obj, value);
                properInfo?.SetValue(obj, value);
            });

            container.AddAsChild(fieldContainer);
            fieldContainer.AddAsChild(checkbox);
            fieldContainer.AddAsChild(text);
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
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_x_container"
            };
            var fieldContainer2 = new Pannel()
            {
                BackgroundColor = new(149, 173, 190),
                sizePercent = new(0.5f, 0),
                positionPercent = new(0f, 0.5f),
                sizePixels = new(0, 25),
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_y_container"
            };
            
            var field1 = new WriteTextField()
            {
                Text = "" + value.X,
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                Color = new(0, 0, 0),
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_x_field"
            };
            var field2 = new WriteTextField()
            {
                Text = "" + value.Y,
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                Color = new(0, 0, 0),
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_y_field"
            };

            field1.OnTextEdited.Connect((object? from, dynamic[]? args) => {
                if (!double.TryParse(args![0], out double _)) args![0] = "0";

                dynamic value = fieldInfo?.GetValue(obj) ?? properInfo!.GetValue(obj)!;
                value.X = Convert.ChangeType(double.Parse(args![0]), fieldGenericArgs[0]);
                fieldInfo?.SetValue(obj, value);
                properInfo?.SetValue(obj, value);
            });
            field2.OnTextEdited.Connect((object? from, dynamic[]? args) => {
                if (!double.TryParse(args![0], out double _)) args![0] = "0";

                dynamic value = fieldInfo?.GetValue(obj) ?? properInfo!.GetValue(obj)!;
                value.Y = Convert.ChangeType(double.Parse(args![0]), fieldGenericArgs[0]);
                fieldInfo?.SetValue(obj, value);
                properInfo?.SetValue(obj, value);
            });

            fieldContainer1.AddAsChild(field1);
            fieldContainer2.AddAsChild(field2);
            container.AddAsChild(fieldContainer1);
            container.AddAsChild(fieldContainer2);
        }

        else if (fieldType.IsEnum)
        {
            int value = (int) (fieldInfo?.GetValue(obj) ?? properInfo!.GetValue(obj))!;

            var values = Enum.GetValues(fieldType);

            var field = new Select()
            {
                sizePercent = new(0.5f, 1),
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_select_box"
            };

            for (int i = 0; i < values.Length; i++)
                field.AddValue( (int) values.GetValue(i)!, values.GetValue(i)!.ToString()! );
            
            field.Value = value;

            field.OnValueChange.Connect((from, args) => {
                fieldInfo?.SetValue(obj, args![0]);
                properInfo?.SetValue(obj, args![0]);
            });

            container.AddAsChild(field);
        }

        return container;
    }

    private static Pannel CreateLogItem(LogInfo log)
    {

        var nLog = new Pannel
        {
            sizePercent = new(1, 0),
            sizePixels = new(0, 40)
        };
        var message = new TextField
        {
            Color = new(255, 255, 255),
            Font = new("./Assets/Fonts/calibri.ttf", 15),
            Text = log.message,
            sizePixels = new(-10, -22),
            positionPixels = new(5, 5)
        };
        var details = new TextField
        {
            anchor = NodeUI.ANCHOR.BOTTOM_LEFT,
            Color = new(255, 255, 255, 0.5f),
            Font = new("./Assets/Fonts/consola.ttf", 10),
            sizePercent = new(1, 0),
            sizePixels = new(0, 12)
        };
         
        var sourceFile = log.sourceFile != "" ? log.sourceFile : "undefined";
        var timestamp = log.timestamp.ToString(@"hh\:mm\:ss");

        details.Text = $"{sourceFile}:{log.callerName} (l. {log.lineNumber}) at {timestamp}";

        nLog.AddAsChild(message);
        nLog.AddAsChild(details);

        return nLog;

    }

    #endregion

}
