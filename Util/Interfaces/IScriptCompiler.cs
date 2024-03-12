using static GameEngine.Util.Nodes.TextField;

namespace GameEngine.Util.Interfaces;

public interface IScriptCompiler
{

    public Type? Compile(string src, string sourcepath);

    public static ColorSpan[] Highlight(string src) => [];

}