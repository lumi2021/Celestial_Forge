using GameEngine.Core.Scripting;

namespace GameEngine.Util.Resources.Scripting;


public class ScriptData
{
    public ClassData[] classes = [];
}

public class ClassData
{
    public string name = "";
    public bool isPrivate;
    public bool isAbstract;

    public FieldData[] fields = [];
    public ConstructorData[] constructors = [];
    public MethodData[] methods = [];

    public override string ToString()
    {
        
        var str = string.Format(
            "{1}class {0}:\n" +
            "\tConstructor count: {2}\n",
            name, GetAttributesString(),
            constructors.Length
        );

        str += string.Format("\tMethods ({0}):\n", methods.Length);
        foreach (var i in methods)
            str += "\t\t" + i.ToString() + "\n";

        return str;


    }
    public string GetAttributesString()
    {
        var str = "";
        if (isPrivate) str += "[private] ";
        else str += "[public] ";
        if (isAbstract) str += "[abstract] ";
        return str;
    }
}

public class FieldData
{
    public string name = "";
    public bool isPrivate;
    public Type fieldType = typeof(void);
    public dynamic? defaultValue = null;
}

public class MethodData
{
    public string name = "";
    public bool isPrivate;
    public Type returnType = typeof(void);

    public DrasmOperation[] script = [];

    public override string ToString()
    {
        return string.Format("{0}{1} -> {2} ({3} ops)", GetAttributesString(), name, returnType.Name, script.Length);
    }
    public string GetAttributesString()
    {
        var str = "";
        if (isPrivate) str += "[private] ";
        else str += "[public] ";
        return str;
    }
}

public class ConstructorData
{
    public bool isPrivate;
    public DrasmOperation[] script = [];
}

public struct DrasmOperation
{
    public DrasmOperations operation;
    public OpArg[] args;
    public readonly int ArgCount { get => args.Length; }
}

public struct OpArg
{
    public DrasmParameterTypes type;
    public dynamic? value;
}