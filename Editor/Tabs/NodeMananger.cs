using System.Reflection;
using GameEngine;
using GameEngine.Util.Attributes;
using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;

namespace GameEngineEditor.Editor;

public class NodeMananger
{

    private readonly string NodeManangerComponentPath = "./Data/Screens/tabs/nodeMananger.json";

    private TreeGraph _nodesList = null!;


    public Node? CreateNodeMananger()
    {
        var nodeMananger = PackagedScene.Load(NodeManangerComponentPath)!.Instantiate();

        _nodesList = (nodeMananger!.GetChild("Container/NodesList") as TreeGraph)!;

        return nodeMananger;
    }

    public void LoadSceneNodes(Node sceneRoot)
    {

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
                nTexture.LoadFromFile(nodeIconAtrib.path, 20, 20);
                IconsBuffer.Add(node.GetType().Name, nTexture);
                nodeIcon = nTexture;
            }

            var item = _nodesList.AddItem(path, node.name, nodeIcon);
            item!.data.Add("NodeRef", node);
            //item!.OnClick.Connect(OnNodeClicked);

            for (int i = node.children.Count-1; i >= 0 ; i--)
                ToList.Insert(0, new(path+"/"+node.name, node.children[i]));
        }

        Texture rootIcon;
        if (IconsBuffer.ContainsKey(sceneRoot.GetType().Name))
            rootIcon = IconsBuffer[sceneRoot.GetType().Name];
        else
        {
            var nTexture = new SvgTexture() { Filter = false };
            IconAttribute nodeIconAtrib = (IconAttribute)sceneRoot.GetType()
            .GetCustomAttribute(typeof(IconAttribute))!;
            nTexture.LoadFromFile(nodeIconAtrib.path, 20, 20);
            IconsBuffer.Add(sceneRoot.GetType().Name, nTexture);
            rootIcon = nTexture;
        }

        _nodesList.Root.Name = sceneRoot.name;
        _nodesList.Root.Icon = rootIcon;

        _nodesList.Root.data.Add("NodeRef", sceneRoot);
        //_nodesList.Root.OnClick.Connect(OnNodeClicked);

    }

}