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
                NodeSelected(null);
            });
            acceptBtn!.OnPressed.Connect((_,_) => {
                open = false;
                popupNode.Free();
                NodeSelected( typeof(Node) );
            });

            return popupNode;
        }
        return null;
    }

    private void NodeSelected(Type? node)
    {
        returnedNodeEvent?.Invoke(node);
    }

}