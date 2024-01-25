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

    public class ScriptData
    {
        public ClassData[] classes = Array.Empty<ClassData>();


        public ClassData? GetClass(string name)
        {
            return classes.First(e => e.name == name);
        }

    }
    public struct ClassData
    {
        public string name;
        public bool isPrivate;
        public bool isAbstract;

        public FieldData[] fields;
        public MethodData[] methods;
    }
    public struct FieldData
    {
        public string name;
        public bool isPrivate;
        public Type fieldType;
        public dynamic defaultValue;
    }
    public struct MethodData
    {
        public string name;
        public bool isPrivate;
        public Type returnType;
    }

}