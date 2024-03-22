using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using static GameEngine.Util.Nodes.TextField;

namespace GameEngineEditor.EditorNodes;

internal class CodeEditor : NodeUI
{

    #region WriteTextField overriding properties

    public string Text
    {
        get => _textField.Text;
        set => _textField.Text = value;
    }
    public uint FontSize
    {
        get => _font.Size;
        set => _font.Size = value;
    }
    public Signal OnTextEdited => _textField.OnTextEdited;
    public ColorSpan[] ColorsList
    {
        get => _textField.colorsList;
        set => _textField.colorsList = value;
    }

    #endregion

    private WriteTextField _textField = new()
    {
        Color = new(225, 225, 225),
        anchor = ANCHOR.TOP_RIGHT
    };
    private TextField _lineCount = new()
    {
        sizePercent = new(0, 1),
        Color = new(225, 225, 225, 0.5f),
        anchor = ANCHOR.TOP_LEFT,
        ForceTextSize = true
    };

    private Panel _intelisense = new()
    {
        Visible = false,
        sizePercent = new(),
        sizePixels = new(250, 200),
        BackgroundColor = new(0,0,0),
        StrokeColor = new(255,255,255),
        StrokeSize = 1
    };

    private Font _font = new("./Assets/Fonts/consola.ttf", 12);

    private uint _lastLineCount = 0;

    protected override void Init_()
    {
        margin = 10;

        _lineCount.Font = _font;
        _textField.Font = _font;

        AddAsGhostChild(_lineCount);
        AddAsGhostChild(_textField);
        AddAsGhostChild(_intelisense);

        _textField.OnTextEdited.Connect((_, args) => OnTextUpdate(args![0]));

        _textField.OnFocus.Connect((_,_) => OnTextFieldFocusChanged(true));
        _textField.OnUnfocus.Connect((_,_) => OnTextFieldFocusChanged(false));
        _textField.OnCaretMoved.Connect((_,_) => OnCaretMoved());

        OnTextUpdate(Text);
    }

    private void OnTextUpdate(string text)
    {
        var linesNumber = (uint) text.Split('\n').Length;

        if (linesNumber != _lastLineCount)
        {
            _lastLineCount = linesNumber;

            _lineCount.Text = "";
            for (uint i = 0; i < linesNumber; i++)
            {
                _lineCount.Text += $"{i+1}\n";
            }

        }
    
        _textField.sizePixels.X = (int) -_lineCount.Size.X - 10;
        _textField.RequestUpdateAllChildrens();
    }

    private void OnTextFieldFocusChanged(bool focused)
        => _intelisense.Visible = false;//focused;
    private void OnCaretMoved()
    {

        var offset = new Vector2<int>(30, 0);
        _intelisense.positionPixels = (Vector2<int>)_textField.CaretGlobalPosition - Position + offset;
        _intelisense.RequestUpdateAllChildrens();

    }

}