using System.Reflection;
using GameEngine.Util.Resources;
using static GameEngine.Util.Nodes.TextField;

namespace GameEngine.Util.Interfaces;

public interface IScriptCompiler
{

    public virtual static Assembly? Compile(Script script) => null;
    public virtual static Assembly? CompileMultiple(Script[] scripts) => null;

    public static ColorSpan[] Highlight(string src) => [];

}