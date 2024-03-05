using GameEngine.Util.Attributes;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using Silk.NET.GLFW;
using static GameEngine.Util.Nodes.Window.InputHandler;

namespace GameEngine.Util.Nodes;

public class WriteTextField : TextField
{

    private bool _multiLine = false;
    [Inspect]
    public bool MultiLine
    {
        get { return _multiLine; }
        set
        {
            _multiLine = value;
            if (!value)
                Text = Text.Replace("\r", "").Replace('\n', ' ');

        }
    }

    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            TextEdited();
        }
    }

    private uint caretLine = 0;
    private uint caretRow = 0;
    //private uint caretRowMax = 0;

    private readonly Pannel caret = new();

    public readonly Signal OnTextEdited = new();

    protected override void Init_()
    {
        base.Init_();

        AddAsChild(caret);
        caret.sizePercent = new();
        caret.sizePixels.X = 1;
        caret.sizePixels.Y = Font.fontheight;
        caret.BackgroundColor = new(255, 255, 255);
        caret.Visible = Focused;
    }

    protected override void Process(double deltaT)
    {
        int caretPosX = 0;
        for (int i = 0; i < caretRow; i++)
            caretPosX += (int)charsList[caretLine][i].Advance;
        
        caret.positionPixels.X = caretPosX;
        caret.positionPixels.Y = (int)caretLine * Font.lineheight;
        caret.BackgroundColor = Color;
    }

    protected override void OnFontUpdate()
    {
        base.OnFontUpdate();
        caret.sizePixels.Y = Font.fontheight + 2;
    }
    protected override void TextEdited()
    {
        base.TextEdited();
        if (_textLines.Length <= caretLine)
        {
            caretLine = (uint) _textLines.Length - 1;
            caretRow = (uint) _textLines[caretLine].Length;
        }
        else if (_textLines[caretLine].Length < caretRow)
            caretRow = (uint) _textLines[caretLine].Length;

        OnTextEdited.Emit(this, Text);
    }

    protected override void OnUIInputEvent(InputEvent e)
    {
        if (e is MouseInputEvent)
        {
            if (mouseFilter == MouseFilter.Ignore) return;

            if (e is MouseBtnInputEvent @event && @event.action == Silk.NET.GLFW.InputAction.Press)
            if (new Rect(Position, Size).Intersects(@event.position))
            {
                onClick.Emit(this);
                if (mouseFilter == MouseFilter.Block)
                {
                    Viewport?.SupressInputEvent();
                    Focus();

                    Vector2<int> relativeMousePos = @event.position - Position;
                    // put the carret on the right position
                    caretLine = (uint) (relativeMousePos.Y / Font.lineheight);

                    if (caretLine > _textLines.Length-1)
                    {
                        caretLine = (uint) _textLines.Length-1;
                        caretRow = (uint) _textLines[^1].Length;
                    }
                    else
                    {
                        int advance = 0;
                        int index = 0;
                        foreach (var c in charsList[caretLine])
                        {
                            if (advance + c.Advance > relativeMousePos.X) break;
                            advance += (int) c.Advance;
                            index++;
                        }
                        
                        if (charsList[caretLine].Length < index-1)
                        {
                            double d1 = Math.Abs(relativeMousePos.X - advance);
                            double d2 = Math.Abs(relativeMousePos.X
                            - (advance + charsList[caretLine][index+1].Advance));
                            if (d1 > d2) index++;
                        }
                        
                        caretRow = (uint) index;
                    }
                }
            }
        }
    }
    protected override void OnFocusedUIInputEvent(InputEvent e)
    {
        base.OnFocusedUIInputEvent(e);

        if (e is KeyboardInputEvent @event && @event.action != InputAction.Release)
        {
            if (Input.LastInputedChars.Length > 0)
                AppendBeforeCursor(Input.LastInputedChars);

            if (!MultiLine && @event.key == Keys.Enter)
            {
                AppendBeforeCursor("\n");
                caretLine++;
                caretRow = 0;
            }

            else if (@event.key == Keys.Backspace)
            {
                RemoveBeforeCursor(1);
            }

            else if (@event.key == Keys.Delete)
            {
                RemoveAfterCursor(1);
            }
        
            else if (@event.key == Keys.Left)
            {
                if (caretRow > 0)
                    caretRow--;
                else if (caretLine > 0)
                {
                    caretLine--;
                    caretRow = (uint) _textLines[caretLine].Length;
                }
            }
            else if (@event.key == Keys.Right)
            {
                if (caretRow < _textLines[caretLine].Length)
                    caretRow++;
                
                else if (caretLine < _textLines.Length-1)
                {
                    caretLine++;
                    caretRow = 0;
                }
            }
            else if (@event.key == Keys.Up)
            {
                if (caretLine > 0)
                {
                    caretLine--;
                    caretRow = (uint) _textLines[caretLine].Length;
                }
            }
            else if (@event.key == Keys.Down)
            {
                if (caretLine < _textLines.Length-1)
                {
                    caretLine++;
                    caretRow = (uint) _textLines[caretLine].Length;
                }
            }
        }
    }

    protected override void OnFocusChanged(bool focused)
    {
        caret.Visible = focused;
    }

    protected void AppendBeforeCursor(string str)
    {
        var s = str;
        if (MultiLine) s = s.Replace("\r", "").Replace('\n', ' ');

        var line =  _textLines[caretLine];
        _textLines[caretLine] =
        line[..(int)caretRow] + s + line[(int)caretRow..];
        Text = string.Join('\n', _textLines);

        caretRow += (uint) Input.LastInputedChars.Length;
    }
    protected void RemoveBeforeCursor(uint length)
    {
        uint charsToRemove = length;

        while(charsToRemove > 0)
        {
            var line = _textLines[caretLine];
            // Do it if charsToRemove is lower than the caret position
            if (line[..(int)caretRow].Length >= charsToRemove)
            {
                _textLines[caretLine] =
                line[..(int)(caretRow - charsToRemove)] + line[(int)caretRow..];
                caretRow -= charsToRemove;
                charsToRemove = 0;
            }
            // Do it if charsToRemove is higher than the caret position and there's no lines before
            else if (caretLine <= 0)
            {
                int toRemove = line[..(int)caretRow].Length;
                _textLines[caretLine] = line[toRemove..];

                caretRow = 0;
                charsToRemove = 0;
            }
            // Do it if charsToRemove is higher than the caret position and there's a line before
            else {
                if (caretRow <= 0) // remove this line and continue in the next
                {
                    var nextCarretRow =  _textLines[caretLine-1].Length;

                    _textLines[caretLine-1] += line[(int)caretRow..];
                    var linesList = _textLines.ToList();
                    linesList.RemoveAt((int) caretLine);
                    _textLines = linesList.ToArray();

                    caretLine--;

                    caretRow = (uint) nextCarretRow;
                    charsToRemove--;
                }
                else
                {
                    int toRemove = line[..(int)caretRow].Length;

                    _textLines[caretLine] = line[toRemove..];

                    charsToRemove -= (uint) toRemove;
                    caretRow -= (uint) toRemove;
                }

            }
        }

        Text = string.Join('\n', _textLines);
    }
    protected void RemoveAfterCursor(uint length)
    {
        uint charsToRemove = length;

        while(charsToRemove > 0)
        {
            var line = _textLines[caretLine];
            // Do it if charsToRemove is lower than the caret position
            if (line[(int)caretRow..].Length > 0)
            {
                _textLines[caretLine] =
                line[..(int)caretRow] + line[(int)(caretRow + charsToRemove)..];
                charsToRemove = 0;
            }
            // Do it if charsToRemove is higher than the caret position and there's no lines before
            else if (caretLine >= _textLines.Length)
            {
                int toRemove = line[(int)caretRow..].Length;
                _textLines[caretLine] = line[toRemove..];

                charsToRemove = 0;
            }
            // Do it if charsToRemove is higher than the caret position and there's a line before
            else {
                if (caretRow >= _textLines[caretLine].Length) // remove this line and continue in the next
                {
                    
                    _textLines[caretLine] += _textLines[caretLine+1];
                    var linesList = _textLines.ToList();
                    linesList.RemoveAt((int) caretLine+1);
                    _textLines = [.. linesList];

                    charsToRemove--;
                }
                else
                {
                    int toRemove = line[(int)caretRow..].Length;

                    _textLines[caretLine] = line[..toRemove];

                    charsToRemove -= (uint) toRemove;
                }

            }
        }

        Text = string.Join('\n', _textLines);
    }

}
