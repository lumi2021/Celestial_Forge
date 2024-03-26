using System.Reflection;
using GameEngine.Util.Attributes;
using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;

namespace GameEngineEditor.Editor.Popups;

public class NewNodePopup
{

    public bool open = false;

    private readonly string popupComponentPath = "./Data/Screens/Popups/NewNode.json";
    private PackagedScene popup = null!;

    public delegate void ReturnedNodeEventHandler(Type? node);
    public ReturnedNodeEventHandler? returnedNodeEvent;

    private Type? selected = null;

    public NewNodePopup()
    {
        popup = PackagedScene.Load(popupComponentPath)!;
    }

    public Window? RequestNewNode()
    {
        if (!open)
        {
            open = true;

            var popupNode = (popup.Instantiate() as Window)!;

            var cancelBtn = popupNode.GetChild("Body/Options/Cancel") as Button;
            var acceptBtn = popupNode.GetChild("Body/Options/Accept") as Button;

            cancelBtn!.OnPressed.Connect((_,_) => {
                open = false;
                popupNode.Free();
                NodeSelected( null );
            });
            acceptBtn!.OnPressed.Connect((_,_) => {
                open = false;
                popupNode.Free();
                NodeSelected( selected );
            });

            var nodeList = (popupNode.GetChild("Body/MainContainer/TreeContanier/NodesList") as TreeGraph)!;
            nodeList.HideRoot = true;

            var typesList = Editor.RegistredTypes.Where(e => e.type.IsAssignableTo(typeof(Node)));
            Dictionary<Type, TreeGraph.TreeGraphItem> type2Item = [];
            Dictionary<Type, Texture> iconsBuf = [];

            foreach (var t in typesList)
            {
                var path = t.extends != null ? 
                    (type2Item.TryGetValue(t.extends, out var i) ? i.Path : "root")
                    : "root";

                var item = nodeList.AddItem(path, t.type.Name)!;
                item.Collapsed = true;

                if (iconsBuf.TryGetValue(t.type, out var tex)) item.Icon = tex;
                else
                {
                    var nTexture = new SvgTexture() { Filter = false };
                    IconAttribute nodeIconAtrib = (IconAttribute)t.type.GetCustomAttribute(typeof(IconAttribute))!;
                    nTexture.LoadFromFile(nodeIconAtrib.path, 16, 16);
                    iconsBuf.Add(t.type, nTexture);
                    item.Icon = nTexture;
                }

                item.SetData("refType", t.type);

                item.OnClick.Connect(ItemSelected);

                type2Item.Add(t.type, item);
            }

            return popupNode;
        }
        return null;
    }

    private void NodeSelected(Type? t)
    {
        returnedNodeEvent?.Invoke(t);
    }

    private void ItemSelected(object? item, dynamic[]? args)
    {
        selected = (item as TreeGraph.TreeGraphItem)!.GetData("refType");
    }

}
