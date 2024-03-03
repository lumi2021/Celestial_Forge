namespace GameEngine.Util.Interfaces;

public interface IScriptCompiler
{
    public Type? Compile(string src, string sourcepath);
}