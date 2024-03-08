namespace GameEngine.Util.Resources;

public class Script : Resource
{

    public FileReference? source;
    public string? language;

    public bool compiled = false;

    public void Compile()
    {
        Compile(source?.ReadAllFile() ?? "");
    }
    public void Compile(string src)
    {

    }

}