namespace GameEngine.Util.Resources;

public class Script(FileReference src, string lang) : Resource
{

    public string Code => path.ReadAllFile();
    public readonly FileReference path = src;
    public readonly string language = lang;

}