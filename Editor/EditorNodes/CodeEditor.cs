using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
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

    private Font _font = new("./Assets/Fonts/consola.ttf", 12);

    private uint _lastLineCount = 0;

    protected override void Init_()
    {
        margin = 10;

        _lineCount.Font = _font;
        _textField.Font = _font;

        AddAsGhostChild(_lineCount);
        AddAsGhostChild(_textField);

        _textField.OnTextEdited.Connect((_, args) => OnTextUpdate(args![0]));

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
    }

}