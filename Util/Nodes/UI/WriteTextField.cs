using Silk.NET.Input;

namespace GameEngine.Util.Nodes;

public class WriteTextField : Label
{

    private uint line = 0;
    private uint row = 0;

    protected override void Init_()
    {
        base.Init_();
        
    }

    protected override void Process(double deltaT)
    {
        text += Input.LastInputedChars;

        if (Input.IsActionJustPressed(Key.Enter))
            text += '\n';
        if (text.Length > 0 && Input.IsActionJustPressed(Key.Backspace))
            text = text.Substring(0, text.Length-1);
    }

}
