using GameEngine.Util.Attributes;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using Silk.NET.GLFW;

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

    public bool CaretActive => Focused;

    public Vector2<float> CaretGlobalPosition
    {
        get
        {
            int caretPosX = 0;
            for (int i = 0; i < _caretCol; i++)
                caretPosX += (int)charsList[_caretLine][i].Advance;

            var caretPosY = (int)_caretLine * Font.lineheight;

            return Position + new Vector2<float>(caretPosX, caretPosY);
        }
    }

    private uint _caretLine = 0;
    private uint _caretCol = 0;

    public uint CaretLine
    {
        get => _caretLine;
        set
        {
            _caretLine = value;
            CaretMoved();
        }
    }
    public uint CaretCol
    {
        get => _caretCol;
        set
        {
            _caretCol = value;
            CaretMoved();
        }
    }

    private uint _caretLastCol = 0;

    private readonly Panel caret = new();

    public readonly Signal OnTextEdited = new();
    public readonly Signal OnCaretMoved = new();

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

    protected override void OnFontUpdate()
    {
        base.OnFontUpdate();
        caret.sizePixels.Y = Font.fontheight + 2;
    }
    protected override void TextEdited()
    {
        base.TextEdited();
        if (_textLines.Length <= _caretLine)
        {
            CaretLine = (uint) _textLines.Length - 1;
            CaretCol = (uint) _textLines[_caretLine].Length;
        }
        else if (_textLines[_caretLine].Length < _caretCol)
            CaretCol = (uint) _textLines[_caretLine].Length;

        OnTextEdited.Emit(this, Text);
    }

    protected virtual void CaretMoved()
    {
        int caretPosX = 0;
        for (int i = 0; i < _caretCol; i++)
            caretPosX += (int)charsList[_caretLine][i].Advance;
        
        caret.positionPixels.X = caretPosX;
        caret.positionPixels.Y = (int)_caretLine * Font.lineheight;
        caret.BackgroundColor = Color;

        OnCaretMoved.Emit(this, _caretLine, _caretCol);
    }

    protected override void OnUIInputEvent(InputEvent e)
    {
        if (e.Is<MouseInputEvent>())
        {
            if (mouseFilter == MouseFilter.Ignore) return;

            if (e.Is<MouseBtnInputEvent>(out var @event) && @event.action == InputAction.Press)
            if (new Rect(Position, Size).Intersects(@event.position))
            {
                OnClick.Emit(this);

                if (mouseFilter != MouseFilter.Ignore)
    
                Viewport?.SupressInputEvent();
                Focus();

                Vector2<int> relativeMousePos = @event.position - Position;

                // put the carret on the right position
                _caretLine = (uint) (relativeMousePos.Y / Font.lineheight);

                if (_caretLine > _textLines.Length-1)
                {
                    _caretLine = (uint) _textLines.Length-1;
                    _caretCol = (uint) _textLines[^1].Length;
                }
                else
                {
                    int advance = 0;
                    int index = 0;

                    foreach (var c in charsList[_caretLine])
                    {
                        if (advance + c.Advance > relativeMousePos.X) break;
                        advance += (int) c.Advance;
                        index++;
                    }
                    
                    /*
                    if (charsList[_caretLine].Length < index-1)
                    {
                        double d1 = Math.Abs(relativeMousePos.X - advance);
                        double d2 = Math.Abs(relativeMousePos.X
                        - (advance + charsList[_caretLine][index+1].Advance));
                        if (d1 > d2) index++;
                    }
                    */
                    
                    _caretCol = (uint) index;
                }
                CaretMoved();
            
                _caretLastCol = _caretCol;

            }
            else if (Focused) Unfocus();

            //if (e.Is<MouseMoveInputEvent>(out var mMoveEvent))
            //{
            //    if (new Rect(Position, Size).Intersects(mMoveEvent.position + Viewport!.Camera2D.position))
            //    {
            //        Viewport?.SupressInputEvent();
            //        Input.SetCursorShape(CursorShape.IBeam);
            //    }
            //}
        }
    }
    protected override void OnFocusedUIInputEvent(InputEvent e)
    {
        if (e.Is<KeyboardKeyInputEvent>(out var @event) && @event.action != InputAction.Release)
        {
            if (!MultiLine && @event.key == Keys.Enter)
            {
                AppendBeforeCursor("\n");
                _caretLine++;
                _caretCol = 0;
                CaretMoved();
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
                if (_caretCol > 0)
                    CaretCol--;
                else if (_caretLine > 0)
                {
                    CaretLine--;
                    CaretCol = (uint) _textLines[_caretLine].Length;
                }

                _caretLastCol = _caretCol;
            }
            else if (@event.key == Keys.Right)
            {
                if (_caretCol < _textLines[_caretLine].Length)
                    CaretCol++;
                
                else if (_caretLine < _textLines.Length-1)
                {
                    CaretLine++;
                    CaretCol = 0;
                }

                _caretLastCol = _caretCol;
            }
            else if (@event.key == Keys.Up)
            {
                if (_caretLine > 0)
                {
                    CaretLine--;
                    CaretCol = Math.Min(_caretLastCol, (uint) _textLines[_caretLine].Length);
                }
            }
            else if (@event.key == Keys.Down)
            {
                if (_caretLine < _textLines.Length-1)
                {
                    CaretLine++;
                    CaretCol = Math.Min(_caretLastCol, (uint) _textLines[_caretLine].Length);
                }
            }
        }
        else if (e.Is<KeyboardCharInputEvent>(out var @charEvent))
        {
            AppendBeforeCursor("" + @charEvent.character);
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

        var line =  _textLines[_caretLine];
        _textLines[_caretLine] =
        line[..(int)_caretCol] + s + line[(int)_caretCol..];
        Text = string.Join('\n', _textLines);

        _caretCol += (uint) str.Replace("\n", "").Length;
        CaretMoved();
    }
    protected void RemoveBeforeCursor(uint length)
    {
        uint charsToRemove = length;

        while(charsToRemove > 0)
        {
            var line = _textLines[_caretLine];
            // Do it if charsToRemove is lower than the caret position
            if (line[..(int)_caretCol].Length >= charsToRemove)
            {
                _textLines[_caretLine] =
                line[..(int)(_caretCol - charsToRemove)] + line[(int)_caretCol..];
                _caretCol -= charsToRemove;
                charsToRemove = 0;
            }
            // Do it if charsToRemove is higher than the caret position and there's no lines before
            else if (_caretLine <= 0)
            {
                int toRemove = line[..(int)_caretCol].Length;
                _textLines[_caretLine] = line[toRemove..];

                _caretCol = 0;
                charsToRemove = 0;
            }
            // Do it if charsToRemove is higher than the caret position and there's a line before
            else {
                if (_caretCol <= 0) // remove this line and continue in the next
                {
                    var nextCarretRow =  _textLines[_caretLine-1].Length;

                    _textLines[_caretLine-1] += line[(int)_caretCol..];
                    var linesList = _textLines.ToList();
                    linesList.RemoveAt((int) _caretLine);
                    _textLines = linesList.ToArray();

                    _caretLine--;

                    _caretCol = (uint) nextCarretRow;
                    charsToRemove--;
                }
                else
                {
                    int toRemove = line[..(int)_caretCol].Length;

                    _textLines[_caretLine] = line[toRemove..];

                    charsToRemove -= (uint) toRemove;
                    _caretCol -= (uint) toRemove;
                }

            }
            CaretMoved();
        }

        Text = string.Join('\n', _textLines);
    }
    protected void RemoveAfterCursor(uint length)
    {
        uint charsToRemove = length;

        while(charsToRemove > 0)
        {
            var line = _textLines[_caretLine];
            // Do it if charsToRemove is lower than the caret position
            if (line[(int)_caretCol..].Length > 0)
            {
                _textLines[_caretLine] =
                line[..(int)_caretCol] + line[(int)(_caretCol + charsToRemove)..];
                charsToRemove = 0;
            }
            // Do it if charsToRemove is higher than the caret position and there's no lines before
            else if (_caretLine >= _textLines.Length)
            {
                int toRemove = line[(int)_caretCol..].Length;
                _textLines[_caretLine] = line[toRemove..];

                charsToRemove = 0;
            }
            // Do it if charsToRemove is higher than the caret position and there's a line before
            else {
                if (_caretCol >= _textLines[_caretLine].Length) // remove this line and continue in the next
                {
                    
                    _textLines[_caretLine] += _textLines[_caretLine+1];
                    var linesList = _textLines.ToList();
                    linesList.RemoveAt((int) _caretLine+1);
                    _textLines = [.. linesList];

                    charsToRemove--;
                }
                else
                {
                    int toRemove = line[(int)_caretCol..].Length;

                    _textLines[_caretLine] = line[..toRemove];

                    charsToRemove -= (uint) toRemove;
                }

            }
        }

        Text = string.Join('\n', _textLines);
    }

}
