using System.Reflection;
using GameEngine;
using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
using GameEngineEditor.Editor.Popups;
using static GameEngine.Util.Nodes.TreeGraph;

namespace GameEngineEditor.Editor;

public class NodeMananger
{

    private readonly string NodeManangerComponentPath = "./Data/Screens/tabs/nodeMananger.json";

    private TreeGraph _nodesList = null!;

    public delegate void NodeClickedHandler(object node);
    public NodeClickedHandler? onNodeClickedEvent;

    public Node? sceneRoot = null;
    public TreeGraphItem? selectedNodeTGI = null;
    public Node? selectedNode = null;

    private NewNodePopup newNodePopup = new();

    public Node? CreateNodeMananger()
    {
        var nodeMananger = PackagedScene.Load(NodeManangerComponentPath)!.Instantiate();

        _nodesList = (nodeMananger!.GetChild("Container/NodesList") as TreeGraph)!;

        var newNodeBtn = nodeMananger.GetChild("Controls/NewNodeBtn") as Button;

        newNodeBtn!.OnPressed.Connect((_, _) => CreateNewNodeRequest());

        newNodePopup.returnedNodeEvent = CreateNewNodeResponse;

        return nodeMananger;
    }

    public void ReloadSceneNodes()
    {
        if (sceneRoot != null)
            LoadSceneNodes(sceneRoot);
    }
    public void LoadSceneNodes(Node sceneRoot)
    {

        this.sceneRoot = sceneRoot;

        _nodesList.ClearGraph();

        List<KeyValuePair<string, Node>> ToList = [new("", sceneRoot)];

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
                nTexture.LoadFromFile(nodeIconAtrib.path, 16, 16);
                IconsBuffer.Add(node.GetType().Name, nTexture);
                nodeIcon = nTexture;
            }

            var item = _nodesList.AddItem(path, node.name, nodeIcon);
            item!.SetData("NodeRef", node);
            item!.OnClick.Connect(OnNodeClicked);

            for (int i = node.children.Count-1; i >= 0 ; i--)
                ToList.Insert(0, new(path+"/"+node.name, node.children[i]));
        }

    }

    private void OnNodeClicked(object? from, dynamic[]? args)
    {
        var tgi = (from as TreeGraphItem)!;

        var node = tgi.GetData("NodeRef", null) as Node;

        selectedNodeTGI = tgi;
        selectedNode = node;

        onNodeClickedEvent?.Invoke(node!);
    }

    private void CreateNewNodeRequest()
    {
        if (selectedNode != null && selectedNodeTGI != null)
        {
            var popup = newNodePopup.RequestNewNode();

            if (popup != null)
                Engine.NodeRoot.AddAsChild(popup);

        }
    }

    private void CreateNewNodeResponse(Type? node)
    {
        if (selectedNode != null && selectedNodeTGI != null && node != null)
        {

            var nNode = (Node)Activator.CreateInstance(node)!;
            nNode.name = nNode.GetType().Name;

            selectedNode.AddAsChild(nNode);

            var a = _nodesList.AddItem(selectedNodeTGI.Path, nNode.name, null);

            var nTexture = new SvgTexture() { Filter = false };
            IconAttribute nodeIconAtrib = (IconAttribute)nNode.GetType()
            .GetCustomAttribute(typeof(IconAttribute))!;
            nTexture.LoadFromFile(nodeIconAtrib.path, 20, 20);
            a!.Icon = nTexture;

            a!.SetData("NodeRef", nNode);

            a!.OnClick.Connect(OnNodeClicked);

            _nodesList.UpdateList();

        }
    }

}