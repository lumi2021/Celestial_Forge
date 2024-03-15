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
    public Dictionary<int, string> values = [];

    private readonly Panel Container = new();
    private readonly TextField Label = new() {
        mouseFilter = MouseFilter.Ignore,
        verticalAligin = TextField.Aligin.Center
    };
    private readonly Panel OptionsContainer = new()
    {
        positionPercent = new(0, 1),
        sizePercent = new(1,0),
        Visible = false,
        ZIndex = 999
    };
    private readonly Dictionary<int, Button> options = [];

    public readonly Signal OnValueChange = new();

    protected override void Init_()
    {
        AddAsGhostChild(Container);
        Container.AddAsChild(Label);
        AddAsGhostChild(OptionsContainer);
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
        if (values.TryAdd(number, label))
        {

            var nOp = new Button()
            {
                sizePercent = new(1, 0),
                sizePixels = new(0, 25),
                name = $"select_option_{number}_{label}"
            };
            var nLb = new TextField()
            {
                Text = label,
                verticalAligin = TextField.Aligin.Center,
                mouseFilter = MouseFilter.Pass
            };
            nOp.AddAsChild(nLb);

            nOp.OnPressed.Connect((object? from, dynamic[]? args) => {
                CloseOptionsList();
                Value = number;

            });

            OptionsContainer.AddAsChild(nOp);
            options.Add(number, nOp);

            SortButtons();
            UpdateValue();
        }
    }

    private void UpdateValue()
    {
        Label.Text = values.Count <= 0 ? "" : values[ Value ];
        OnValueChange.Emit(this, [Value]);
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
