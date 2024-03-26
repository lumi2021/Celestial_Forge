﻿using System.Reflection;
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
using GameEngine;

namespace GameEngineEditor.Editor;

public static class Editor
{

    private static ProjectSettings _projectSettings = null!;
    public static ProjectSettings ProjectSettings => _projectSettings;

    private static  List<EditorPlugin> _plugins = [];
    private static Window _mainWindow = null!;

    /* DATA */
    private static List<RegistredType> _registredTypes = [new(typeof(object), null)];
    public static RegistredType[] RegistredTypes => [.. _registredTypes];

    /* COMPONENTS */
    private static EditorUI _uiComponent = null!;

    public static void StartEditor(ProjectSettings settings, Window mainWin)
    {
        _projectSettings = settings;
        _mainWindow = mainWin;

        StartComponents();

        _plugins.Add(new BaseData());
        StartPlugins();
    }

    private static void StartComponents()
    {
        _uiComponent = new(_mainWindow);
        _uiComponent.Create();
    }
    
    private static void StartPlugins()
    {
        foreach (var i in _plugins)
            if (i.Start()) i.active = true;
    }

    public static void RequestRegisterType(Type type, Type? extends)
    {
        if (!_registredTypes.Any(e => e.type == type) && _registredTypes.Any(e => e.type == extends))
            _registredTypes.Add(new(type, extends));
        
        else throw new Exception($"Type {type.Name} is already registred!");
    }

    public struct RegistredType(Type type, Type? extends)
    {
        public readonly Type type = type;
        public readonly Type? extends = extends;
    }

}

class EditorUI (Window mWin)
{
    /* IMPORTANT NODES */
    private readonly Window mainWindow = mWin;

    private Node? editorRoot;
    private TreeGraph? filesList;
    private Panel? sceneViewport;
    private Panel? textEditor;

    private NodeUI? console;
    private NodeUI? errors;

    private NodeMananger? nodeMananger = null;

    private Viewport sceneEnviropment = null!;

    /* ETC */
    private int maintab = 0;

    private FileReference? fileBeingEdited = null;

    public void Create()
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

        sceneViewport = editorRoot!.GetChild("Main/Center/Main/Viewport") as Panel;
        textEditor = editorRoot!.GetChild("Main/Center/Main/TextEditor") as Panel;

        Button sceneBtn = (editorRoot!.GetChild("TopBar/MainOptions/SceneEditor") as Button)!;
        Button scriptBtn = (editorRoot!.GetChild("TopBar/MainOptions/ScriptEditor") as Button)!;

        sceneBtn.OnPressed.Connect((object? from, dynamic[]? args) => ChangeMainView(0));
        scriptBtn.OnPressed.Connect((object? from, dynamic[]? args) => ChangeMainView(1));

        //var textEditorField = textEditor!.GetChild("FileContent") as WriteTextField;
        var textSaveBtn = (textEditor!.GetChild("Toolbar/SaveBtn") as Button)!;
        textSaveBtn.OnPressed.Connect( (object? from, dynamic[]? args) => SaveOpenTextFile() );

        #endregion

        /* CONFIGURATE VIEWPORT */
        #region

        sceneEnviropment = new()
        {
            useContainerSize = true,
            ContainerSize = (Vector2<uint>)Editor.ProjectSettings.canvasDefaultSize,
            backgroundColor = new(50, 50, 100)
        };

        mainWindow.children.Insert(0, sceneEnviropment);
        sceneEnviropment.parent = mainWindow;

        ViewportContainer viewportContainer = new() { linkedViewport = sceneEnviropment };
        sceneViewport!.AddAsChild(viewportContainer);

        #endregion

        /* CONFIGURATE SCRIPT EDITOR */
        #region
        var textEditorField = new CodeEditor() {name = "FileContent"};
        textEditor.GetChild("FileContentContainer")!.AddAsChild(textEditorField);

        textEditorField.OnTextEdited.Connect((object? node, dynamic[]? args) => {
            textEditorField.ColorsList = CSharpCompiler.Highlight(args![0]);
        });
        #endregion

        /* INSTANTIATE AND CONFIGURATE FILE MANANGER */
        #region
        var filesSection = scene.GetChild("Main/LeftPanel/FileMananger");

        filesList = new TreeGraph();
        filesSection!.AddAsChild(filesList);
        
        var txtFile = new SvgTexture() { Filter = false }; txtFile.LoadFromFile("Assets/Icons/Files/textFile.svg", 16, 16);
        var cFolder = new SvgTexture() { Filter = false }; cFolder.LoadFromFile("Assets/Icons/Files/closedFolder.svg", 16, 16);
        var eFolder = new SvgTexture() { Filter = false }; eFolder.LoadFromFile("Assets/Icons/Files/emptyFolder.svg", 16, 16);
        var unkFile = new SvgTexture() { Filter = false }; unkFile.LoadFromFile("Assets/Icons/Files/unknowFile.svg", 16, 16);
        var anvilWk = new SvgTexture() { Filter = false }; anvilWk.LoadFromFile("Assets/Icons/Files/AnvilKey.svg", 16, 16);
        var csproj  = new SvgTexture() { Filter = false }; csproj.LoadFromFile("Assets/Icons/Files/csharpLogo.svg", 16, 16);
        var sceFile = new SvgTexture() { Filter = false }; sceFile.LoadFromFile("Assets/Icons/Files/scene.svg", 16, 16);

        filesList.Root.Icon = cFolder;
        filesList.Root.Name = "res://";

        List<FileSystemInfo> itens = [.. FileService.GetDirectory("res://")];
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

            else if (i.Extension == ".csproj")
                iconImage = csproj;

            var path = FileService.GetProjRelativePath(i.FullName);
            path = path[6..][..^i.Name.Length];

            var item = filesList.AddItem(path, i.Name, iconImage);
            item!.Collapsed = type == "folder";
            item!.SetData("type", type);
            item!.OnClick.Connect(OnFileClicked);
        }
        
        filesList.Root.Collapsed = false;
        #endregion

        /* INSTANTIATE AND CONFIGURATE NODE MANANGER */
        #region

        nodeMananger = new NodeMananger();

        var nodeMngNode = nodeMananger.CreateNodeMananger() as NodeUI;
        scene.GetChild("Main/RightPanel")!.AddAsChild(nodeMngNode!);

        (scene.GetChild("Main/RightPanel/CenterHandler") as DragHandler)!.nodeA = nodeMngNode!; 

        nodeMananger.onNodeClickedEvent = LoadInInspector;

        #endregion

        /* INSTANTIATE AND CONFIGURATE BOTTOM BAR */
        #region
        
        // tab buttons
        var bottomBar = editorRoot!.GetChild("Main/Center/BottomBar") as Panel;

        var aaa = bottomBar!.GetChild("Tabs");

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
        errors = bottomBar!.GetChild("BottomBarWindow/ErrorsTab/Errors/ErrorsLog") as NodeUI;
        Debug.OnLogEvent += OnLog;
        Debug.OnErrorEvent += OnError;

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
            Size = (Vector2<uint>) Editor.ProjectSettings.canvasDefaultSize
        };

        mainWindow.AddAsChild(gameWindow);

        var gameScene = PackagedScene.Load(Editor.ProjectSettings.entryScene)!.Instantiate();
        gameWindow.AddAsChild(gameScene);
    }


    private void OnFileClicked(object? from, dynamic[]? args)
    {
        var item = from as TreeGraph.TreeGraphItem;

        var path = "res://" + item!.Path[7..];

        if (item!.GetData("type", "") != "folder")
        {
            switch (item!.GetData("type", ""))
            {
                case ".sce":
                    LoadSceneInEditor(path); break;

                default:
                    OpenTextFile(path); break;
            }
        }

    }
    private void LoadInInspector(object node)
    {
        LoadInspectorNodeInformation((node as Node)!);
    }

    private void LoadSceneInEditor(string scenePath)
    {
        sceneEnviropment.FreeChildren();

        var cam = new SceneEditor2DCamera();
        sceneEnviropment.AddAsChild(cam);
        cam.Current = true;
        
        var scene = PackagedScene.Load(scenePath)!.Instantiate();

        List<Node> toIterate = [scene];
        // Iterate for all children and disable their process //
        while (toIterate.Count > 0)
        {

            var current = toIterate.Unqueue();

            current.processEnabled = false;
            current.readyEnabled = false;
            current.inputEnabled = false;

            toIterate.AddRange(current.GetAllChildren);

        }

        sceneEnviropment!.AddAsChild(scene);

        nodeMananger?.LoadSceneNodes(scene);
    
        ChangeMainView(0);
    }

    private void OpenTextFile(string filePath)
    {
        var textField = (textEditor!.GetChild("FileContentContainer/FileContent") as CodeEditor)!;
        var file = new FileReference(filePath);

        var content = file.ReadAllFile();

        textField.Text = content;

        fileBeingEdited = file;

        ChangeMainView(1);
    }
    private void SaveOpenTextFile()
    {
        var textField = (textEditor!.GetChild("FileContentContainer/FileContent") as CodeEditor)!;
        fileBeingEdited?.Write(textField.Text);
    }

    private void LoadInspectorNodeInformation(Node node)
    {
        Type nodeType = node.GetType();

        var inspecContainer = editorRoot!.GetChild("Main/RightPanel/Inspector/InspectorContainer")! as NodeUI;
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
    private void OnError(LogInfo err)
    {
        errors!.AddAsChild(CreateLogItem(err));
        var p = 0;
        foreach (var i in errors.children)
        if (i is NodeUI node)
        {
            node.positionPixels.Y = p;
            p += node.sizePixels.Y;
        }
    }


    #region really random stuff

    private static Panel CreateTitleItem(string title)
    {
        var panel = new Panel()
        {
            sizePercent = new(1, 0),
            sizePixels = new(0, 25),
            BackgroundColor = new(0,0,0, 0.2f)
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

            var fieldContainer = new Panel()
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
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_TextField",
                MultiLine = false
            };

            field.OnTextEdited.Connect((object? from, dynamic[]? args) => {
                fieldInfo?.SetValue(obj, args![0]);
                properInfo?.SetValue(obj, args![0]);
            });

            if (inspectAtt.usage == InspectAttribute.Usage.multiline_text)
            {
                field.MultiLine = true;
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

            checkbox.OnValueChange.Connect((object? from, dynamic[]? args) =>
            {
                bool value = args![0];
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

            var fieldContainer1 = new Panel()
            {
                BackgroundColor = new(149, 173, 190),
                sizePercent = new(0.5f, 0),
                sizePixels = new(0, 25),
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_x_container"
            };
            var fieldContainer2 = new Panel()
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
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_x_field",
                verticalAligin = TextField.Aligin.Center,
                MultiLine = false
            };
            var field2 = new WriteTextField()
            {
                Text = "" + value.Y,
                anchor = NodeUI.ANCHOR.TOP_RIGHT,
                Color = new(0, 0, 0),
                name = fieldInfo?.Name ?? properInfo!.Name + "_inspector_setter_y_field",
                verticalAligin = TextField.Aligin.Center,
                MultiLine = false
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

    private static Panel CreateLogItem(LogInfo log)
    {

        var nLog = new Panel
        {
            sizePercent = new(1, 0),
            sizePixels = new(0, 40),
            BackgroundColor = new(0, 0, 0, 0)
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
        var srcFile = new FileReference(sourceFile);

        details.Text = $"{srcFile.RelativePath}:{log.callerName} (l. {log.lineNumber}) at {timestamp}";

        nLog.AddAsChild(message);
        nLog.AddAsChild(details);

        return nLog;

    }

    #endregion

}
