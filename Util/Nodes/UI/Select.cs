using ExCSS;
using GameEngine.Util.Attributes;
using GameEngine.Util.Resources;

namespace GameEngine.Util.Nodes;

public class Select : NodeUI
{
    private uint _value = 0;
    [Inspect]
    public uint Value
    {
        get { return _value; }
        set
        {
            _value = value;
            UpdateValue();
        }
    }

    [Inspect]
    public Dictionary<uint, string> values = new();


    private readonly Pannel Container = new();
    private readonly TextField Label = new() { mouseFilter = MouseFilter.Ignore };
    private readonly Pannel OptionsContainer = new()
    {
        positionPercent = new(0, 1),
        sizePercent = new(1,1)
    };

    public readonly Signal OnValueChange = new();

    protected override void Init_()
    {
        AddAsChild(Container);
        AddAsChild(Label);
        AddAsChild(OptionsContainer);
    }

    private void UpdateValue()
    {
        Label.Text = values[_value];
    }

}