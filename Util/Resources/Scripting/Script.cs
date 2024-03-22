namespace GameEngine.Util.Resources;

public class Script(FileReference src, string lang) : Resource
{

    public string Code => path.ReadAllFile();
    public readonly FileReference path = src;
    public readonly string language = lang;

    public CodeJump[] afterPreprocessJumps = [];


    public readonly struct CodeJump(int pos, int len)
    {
        public readonly int position = pos;
        public readonly int length = len;
    }

}