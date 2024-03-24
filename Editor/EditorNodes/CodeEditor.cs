using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using static GameEngine.Util.Nodes.TextField;
using Silk.NET.GLFW;

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

    private Panel _intellisense = new()
    {
        Visible = false,
        sizePercent = new(),
        sizePixels = new(250, 0),
        BackgroundColor = new(0,0,0),
        StrokeColor = new(255,255,255),
        StrokeSize = 1,
        ClipChildren = true
    };
    private Panel _intellisenseSelection = new()
    {
        BackgroundColor = new(0,0,255, 0.25f),
        sizePercent = new(1, 0)
    };
    private TextField _intellisenseContent = new();

    private Font _font = new("./Assets/Fonts/consola.ttf", 12);

    private uint _lastLineCount = 0;

    private string[] _intellissenseContentItens = [];
    private string[] _intellissenseFilteredItens = [];

    private string _toFilter = "";
    private int _intellisenseSelectedIndex = 0;
    private int _intellisenseFilteredContentLength = 0;

    private bool IntellisenseActived
    {
        get => _intellisense.Visible;
        set
        {
            if (value)
                ActiveIntellisense();
            else UnactiveIntellisense();
        }
    }

    protected override void Init_()
    {
        margin = 10;

        _lineCount.Font = _font;
        _textField.Font = _font;
        _intellisenseContent.Font = _font;

        _intellisense.sizePixels.Y = _font.lineheight * 9;
        _intellisenseSelection.sizePixels.Y = _font.lineheight;

        _intellisense.AddAsChild(_intellisenseSelection);
        _intellisense.AddAsChild(_intellisenseContent);

        AddAsGhostChild(_lineCount);
        AddAsGhostChild(_textField);
        AddAsGhostChild(_intellisense);

        _textField.OnTextEdited.Connect((_, args) => OnTextUpdate(args![0]));
        _textField.preprocessInput += OnTextFieldInputEvent;

        _textField.OnFocus.Connect((_,_) => OnTextFieldFocusChanged(true));
        _textField.OnUnfocus.Connect((_,_) => OnTextFieldFocusChanged(false));
        _textField.OnCaretMoved.Connect((_,args)
            => OnCaretMoved(new((int)args![1], (int)args![0]), new((int)args![3], (int)args![2])));

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
    {

        if (!focused)
            _intellisense.Visible = false;

    }
    private void OnCaretMoved(Vector2<int> newPos, Vector2<int> oldPos)
    {

        var offset = new Vector2<int>(2, _textField.Font.lineheight + 2);
        _intellisense.positionPixels = (Vector2<int>)_textField.CaretGlobalPosition - Position + offset;
        _intellisense.RequestUpdateAllChildrens();

        if ((newPos - oldPos).Y != 0)
        {
            _toFilter = "";
            IntellisenseActived = false;
        }

    }

    private const string specialCharacters1 = "()[]{}\'\".,;\\?:$+-*/= <>";
    private const string specialCharacters2 = "([{\"'";
    private bool OnTextFieldInputEvent(InputEvent e)
    {

        if (e.Is<KeyboardKeyInputEvent>(out var @event))
        {

            if (@event.action == InputAction.Press)
            {
                if (@event.key == Keys.Space && !IntellisenseActived)
                {
                    if (Input.IsActionPressed(Keys.ControlLeft) || Input.IsActionPressed(Keys.ControlRight))
                    {
                        _toFilter = "";
                        var res = CSharpCompiler.GetAutocompleteInPosition(Text, _textField.GetUnidimensionalCaretPosition());
                        _intellissenseContentItens = res;
                        IntellisenseActived = true;
                        FilterIntellisense();

                        return false;
                    }
                }
                else if (@event.key == Keys.Enter && IntellisenseActived)
                {
                    _textField.AppendBeforeCursor(_intellissenseFilteredItens[_intellisenseSelectedIndex][_toFilter.Length ..]);
                    IntellisenseActived = false;
                    return false;
                }
            }

            if (@event.action != InputAction.Release)
            {
            
                if (@event.key == Keys.Backspace)
                {
                    if (_toFilter.Length > 0)
                    {
                        _toFilter = _toFilter[.. (_toFilter.Length-1)];
                        FilterIntellisense();
                    }
                    else IntellisenseActived = false;
                }
                else if (@event.key == Keys.Tab)
                {
                    int tabLength = (int)(_textField.CaretCol - _textField.CaretCol/4 * 4);
                    if (tabLength == 0) tabLength = 4;

                    _textField.AppendBeforeCursor("".PadLeft(tabLength));
                    return false;
                }

                else if (@event.key == Keys.Up && IntellisenseActived)
                {
                    if (_intellisenseSelectedIndex > 0)
                        _intellisenseSelectedIndex--;
                    else
                        _intellisenseSelectedIndex = _intellisenseFilteredContentLength-1;

                    UpdateIntellisense();
                    return false;
                }
                else if (@event.key == Keys.Down && IntellisenseActived)
                {
                    if (_intellisenseSelectedIndex < _intellisenseFilteredContentLength-1)
                        _intellisenseSelectedIndex++;
                    else
                        _intellisenseSelectedIndex = 0;

                    UpdateIntellisense();
                    return false;
                }

            }

            else if (@event.key == Keys.Period && @event.action == InputAction.Release)
            {
                _toFilter = "";

                var res = CSharpCompiler.GetAutocompleteInPosition(Text, _textField.GetUnidimensionalCaretPosition());

                _intellissenseContentItens = res;
                IntellisenseActived = true;
                FilterIntellisense();

            }

        }

        else if (e.Is<KeyboardCharInputEvent>(out var @char))
        {
            if (@char.character == ' ' && (Input.IsActionPressed(Keys.ControlLeft)
                || Input.IsActionPressed(Keys.ControlRight)))
                return false;

            if (specialCharacters2.Contains(@char.character))
            {
                switch (@char.character)
                {
                    case '(':
                        _textField.AppendBeforeCursor("(");
                        _textField.AppendAfterCursor(")");
                        break;
                    case '[':
                        _textField.AppendBeforeCursor("[");
                        _textField.AppendAfterCursor("]");
                        break;
                    case '{':
                        _textField.AppendBeforeCursor("{");
                        _textField.AppendAfterCursor("}");
                        break;
                    case '"':
                        _textField.AppendBeforeCursor("\"");
                        _textField.AppendAfterCursor("\"");
                        break;
                    case '\'':
                        _textField.AppendBeforeCursor("'");
                        _textField.AppendAfterCursor("'");
                        break;

                }

                return false;
            }

            if (!specialCharacters1.Contains(@char.character))
                _toFilter += @char.character;
            else _toFilter = "";

            FilterIntellisense();
        }

        return true;
    }


    private void ActiveIntellisense()
    {
        _intellisense.Show();
    }
    private void UnactiveIntellisense()
    {
        _intellisense.Hide();
    }

    private void UpdateIntellisense()
    {
        int sliceStart = 0;
        int sliceLength = 9;

        bool addUpLabel = false;
        bool addBtnLabel = false;

        if (_intellisenseSelectedIndex < 4)
        {
            sliceStart = 0;
            _intellisenseSelection.positionPixels.Y =  _font.lineheight * _intellisenseSelectedIndex;

            if (_intellisenseFilteredContentLength <= 9)
                sliceLength = 9;
            else
            {
                sliceLength = 8;
                addBtnLabel = true;
            }
        }

        else if (_intellisenseSelectedIndex >= 4 && _intellisenseSelectedIndex < _intellisenseFilteredContentLength - 4)
        {
            sliceStart = _intellisenseSelectedIndex - 3;
            sliceLength = 7;
            addUpLabel = true;
            addBtnLabel = true;

            _intellisenseSelection.positionPixels.Y =  _font.lineheight * 4;
        }

        else if (_intellisenseSelectedIndex >= _intellisenseFilteredContentLength - 4)
        {
            _intellisenseSelection.positionPixels.Y =  _font.lineheight * (_intellisenseSelectedIndex - _intellisenseFilteredContentLength + 9);
            if (_intellisenseFilteredContentLength <= 9)
            {
                sliceLength = 9;
                sliceStart = _intellisenseFilteredContentLength - 9;
            }
            else
            {
                sliceLength = 8;
                sliceStart = _intellisenseFilteredContentLength - 8;
                addUpLabel = true;
            }
        }

        var result = _intellissenseFilteredItens.Skip(sliceStart).Take(sliceLength).ToList();

        if (addUpLabel)
            result.Insert(0, $"... ({sliceStart} more)");
        if (addBtnLabel)
            result.Add($"... ({_intellisenseFilteredContentLength - sliceStart - sliceLength} more)");

        var resultString = string.Join('\n', result);

        _intellisenseContent.Text = resultString;
    }

    private void FilterIntellisense()
    {
        var result = _intellissenseContentItens.Where(e => e.StartsWith(_toFilter)).ToArray();
        _intellisenseFilteredContentLength = result.Length;
        _intellissenseFilteredItens = result;
        _intellisenseSelectedIndex = 0;

        UpdateIntellisense();
    }

}
