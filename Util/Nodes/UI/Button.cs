using GameEngine.Util.Attributes;
using GameEngine.Util.Resources;

namespace GameEngine.Util.Nodes;

[Icon("./Assets/icons/Nodes/Button.svg")]
public class Button : NodeUI
{
    
    private Pannel Container = new();
    private TextField Label = new() { mouseFilter = MouseFilter.Ignore };

    public readonly Signal OnPressed = new();

    protected override void Init_()
    {
        AddAsChild(Container);
        AddAsChild(Label);

        Container.onClick.Connect(OnClick);
    }

    private void OnClick(object? from, dynamic[]? args)
    {
        OnPressed.Emit(this);
    }

}