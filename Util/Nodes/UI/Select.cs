using GameEngine.Util.Attributes;
using GameEngine.Util.Resources;

namespace GameEngine.Util.Nodes;

public class Select : NodeUI
{
    private int _value = 0;
    [Inspect]
    public int Value
    {
        get { return _value; }
        set
        {
            _value = value;
            UpdateValue();
        }
    }

    //[Inspect]
    private Dictionary<int, string> values = new();
    private Dictionary<int, Button> buttons = new();

    private readonly Pannel Container = new() { mouseFilter = MouseFilter.Ignore };
    private readonly TextField Label = new() { mouseFilter = MouseFilter.Ignore };
    private readonly Pannel OptionsContainer = new()
    {
        positionPercent = new(0, 1),
        sizePercent = new(1, 0),
        sizePixels = new(0, 25),
        Visible = false
    };

    public readonly Signal OnValueChange = new();

    protected override void Init_()
    {
        AddAsChild(Container);
        AddAsChild(Label);
        AddAsChild(OptionsContainer);
    }

    public void AddValue(int value, string label)
    {
        if (!values.ContainsKey(value))
        {
            values.Add(value, label);
            UpdateValue();

            // Creating a button here
            var nBtn = new Button()
            {
                sizePercent = new(1, 0),
                sizePixels = new(0, 25)
            };
            var btnLabel = new TextField()
            { Text = label };

            nBtn.AddAsChild(btnLabel);
            OptionsContainer.AddAsChild(nBtn);
            buttons.Add(value, nBtn);

            UpdateList();
        }
    }
    public void SetValue(int value)
    {
        _value = value;
        UpdateValue();
    }

    private void UpdateList()
    {
        int height = 0;
        foreach (var i in buttons)
        {
            i.Value.positionPixels.Y = height;
            height += i.Value.sizePixels.Y;
        }
        OptionsContainer.sizePixels.Y = height;
    }

    private void UpdateValue()
    {
        Label.Text = values[_value];
    }

    protected override void OnFocusChanged(bool focused)
    {
        OptionsContainer.Visible = focused && !OptionsContainer.Visible;
    }

}