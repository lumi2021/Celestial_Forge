using Silk.NET.Input;

namespace GameEngine.Util.Nodes;

public class WriteTextField : Label
{

    private uint caretLine = 0;
    private uint caretRow = 0;

    private Pannel caret = new();

    protected override void Init_()
    {
        base.Init_();
        font.FontUpdated += OnFontUpdate;

        AddAsChild(caret);
        caret.sizePercent = new();
        caret.sizePixels.X = 2;
        caret.sizePixels.Y = font.lineheight;
        caret.backgroundColor = new(255, 255, 255);
    }

    protected override void Process(double deltaT)
    {
        Text += Input.LastInputedChars;
        caretRow += (uint) Input.LastInputedChars.Length;

        if (Input.IsActionJustPressed(Key.Enter))
        {
            Text += '\n';
            caretLine++;
            caretRow = 0;
        }
        if (Text.Length > 0 && Input.IsActionJustPressed(Key.Backspace))
        {
            caretRow -= 1;
            Text = Text.Substring(0, Text.Length-1);
        }

        /*
        if (Input.IsActionPressed(Key.Up))
            font.Size += 1;
        if (Input.IsActionPressed(Key.Down))
            font.Size -= 1;
        */

        int caretPosX = 0;
        for (int i = 0; i < caretRow; i++)
            caretPosX += (int)charsList[caretLine][i].Advance;
        
        caret.positionPixels.X = caretPosX;
        caret.positionPixels.Y = (int)(caretLine * font.lineheight);
    }

    private void OnFontUpdate()
    {
        caret.sizePixels.Y = font.lineheight;
    }

}
