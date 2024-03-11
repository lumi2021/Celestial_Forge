using GameEngine.Util.Attributes;
using GameEngine.Util.Resources;

namespace GameEngine.Util.Nodes;

public class Select : NodeUI
{
    private int _value = 0;
    [Inspect]
    public int Value
    {
        get { return values.Count <= 0 ? 0 : values.Keys.ToArray()[_value]; }
        set
        {
            _value = (int) Math.Min(Math.Max(0, value), values.Count);
            UpdateValue();
        }
    }

    [Inspect]
    public Dictionary<int, string> values = new();

    private readonly Pannel Container = new();
    private readonly TextField Label = new() { mouseFilter = MouseFilter.Ignore };
    private readonly Pannel OptionsContainer = new()
    {
        positionPercent = new(0, 1),
        sizePercent = new(1,0),
        Visible = false,
        ZIndex = 999
    };
    private readonly Dictionary<int, Button> options = new();

    public readonly Signal OnValueChange = new();

    protected override void Init_()
    {
        AddAsChild(Container);
        Container.AddAsChild(Label);
        AddAsChild(OptionsContainer);
        UpdateValue();

        Container.onFocus.Connect((object? _, dynamic[]? _) => ToggleOptionsList());
        Container.onUnfocus.Connect((object? _, dynamic[]? _) => CloseOptionsList());
    }

    private void ToggleOptionsList()
    {
        OptionsContainer.Visible = !OptionsContainer.Visible;
    }
    //private void OpenOptionsList()
    //{
    //    OptionsContainer.Visible = true;
    //}
    private void CloseOptionsList()
    {
        OptionsContainer.Visible = false;
    }

    public void AddValue(int number, string label)
    {
        if (!values.ContainsKey(number))
        {
            values.Add(number, label);

            var nOp = new Button()
            {
                sizePercent = new(1, 0),
                sizePixels = new(0, 25)
            };
            var nLb = new TextField()
            {
                Text = label
            };
            nOp.AddAsChild(nLb);

            OptionsContainer.AddAsChild(nOp);
            options.Add(number, nOp);

            SortButtons();
            UpdateValue();
        }
    }

    private void UpdateValue()
    {
        Label.Text = values.Count <= 0 ? "" : values[ Value ];
    }
    private void SortButtons()
    {
        var btnList = options.ToList();
        btnList.Sort((a, b) => a.Key - b.Key);

        int position = 0;
        foreach (var btn in btnList)
        {
            btn.Value.positionPixels.Y = position;
            position += btn.Value.sizePixels.Y;
        }
        OptionsContainer.sizePixels.Y = position;
    }
}
