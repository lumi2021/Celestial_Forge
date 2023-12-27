using Silk.NET.GLFW;
using static GameEngine.Util.Nodes.Window.InputHandler;

namespace GameEngine.Util.Nodes;

public class WriteTextField : Label
{

    private uint caretLine = 0;
    private uint caretRow = 0;
    //private uint caretRowMax = 0;

    private Pannel caret = new();

    protected override void Init_()
    {
        base.Init_();
        font.FontUpdated += OnFontUpdate;

        AddAsChild(caret);
        caret.sizePercent = new();
        caret.sizePixels.X = 2;
        caret.sizePixels.Y = font.lineheight;
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
        caret.positionPixels.Y = (int)(caretLine * font.lineheight) + 2;
    }

    private void OnFontUpdate()
    {
        caret.sizePixels.Y = font.lineheight;
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
                if (caretLine <= 0 && caretRow <= 0) return;

                if (caretRow > 0)
                {
                    RemoveBeforeCursor(1);
                }
                else {
                    caretLine--;
                    caretRow = (uint) _textLines[caretLine].Length;
                    Console.WriteLine(caretRow);
                }
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
        var line =  _textLines[caretLine];
        _textLines[caretLine] =
        line[..(int)(caretRow - length)] + line[(int)caretRow..];
        Text = string.Join('\n', _textLines);

        caretRow -= length;
    }
}
