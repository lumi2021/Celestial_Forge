using Silk.NET.GLFW;
using static GameEngine.Util.Nodes.Window.InputHandler;

namespace GameEngine.Util.Nodes;

public class WriteTextField : TextField
{

    private uint caretLine = 0;
    private uint caretRow = 0;
    //private uint caretRowMax = 0;

    private Pannel caret = new();

    protected override void Init_()
    {
        base.Init_();

        AddAsChild(caret);
        caret.sizePercent = new();
        caret.sizePixels.X = 2;
        caret.sizePixels.Y = Font.lineheight;
        caret.BackgroundColor = new(255, 255, 255);
    }

    protected override void Process(double deltaT)
    {
        if (Input.LastInputedChars.Length > 0)
            AppendBeforeCursor(Input.LastInputedChars);

        int caretPosX = 0;
        for (int i = 0; i < caretRow; i++)
            caretPosX += (int)charsList[caretLine][i].Advance;
        
        caret.positionPixels.X = caretPosX;
        caret.positionPixels.Y = (int)(caretLine * Font.lineheight) + 2;
    }

    protected override void OnFontUpdate()
    {
        base.OnFontUpdate();
        caret.sizePixels.Y = Font.lineheight;
    }

    protected override void OnInputEvent(InputEvent e)
    {
        if (e is KeyboardInputEvent @event && @event.action != InputAction.Release)
        {

            if (@event.key == Keys.Enter)
            {
                AppendBeforeCursor("\n");
                caretLine++;
                caretRow = 0;
            }

            else if (@event.key == Keys.Backspace)
            {
                RemoveBeforeCursor(1);
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
                else if (caretLine < _textLines.Length)
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

    protected void AppendBeforeCursor(string s)
    {
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

}
