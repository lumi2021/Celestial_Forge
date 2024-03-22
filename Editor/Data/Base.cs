using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;

namespace GameEngineEditor.Editor;

public class BaseData : EditorPlugin
{

    public override bool Start()
    {
        
        #region Nodes

        AddCustomType(typeof(Node), typeof(object));
            // UI
            AddCustomNode(typeof(NodeUI), typeof(Node));
                AddCustomNode(typeof(Panel), typeof(NodeUI));
                AddCustomNode(typeof(Button), typeof(NodeUI));
                AddCustomNode(typeof(Checkbox), typeof(NodeUI));
                AddCustomNode(typeof(DragHandler), typeof(NodeUI));
                AddCustomNode(typeof(ScrollBar), typeof(NodeUI));
                AddCustomNode(typeof(Select), typeof(NodeUI));
                AddCustomNode(typeof(TextField), typeof(NodeUI));
                    AddCustomNode(typeof(WriteTextField), typeof(TextField));
                AddCustomNode(typeof(TextureRect), typeof(NodeUI));
                AddCustomNode(typeof(TreeGraph), typeof(NodeUI));
                AddCustomNode(typeof(ViewportContainer), typeof(NodeUI));
            // 2D
            AddCustomNode(typeof(Node2D), typeof(Node));
                AddCustomNode(typeof(Camera2D), typeof(Node2D));

            // Misc
            AddCustomNode(typeof(Viewport), typeof(Node));
                AddCustomNode(typeof(Window), typeof(Viewport));

        #endregion

        return true;

    }

}