using GameEngine.Core.Scripting;
using System.Reflection.Emit;

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

    public FieldBuilder? fieldRef;
}

public class MethodData
{
    
    public string name = "";
    public bool isPrivate;
    public Type returnType = typeof(void);

    public CodeData script = new();

    public MethodBuilder? methodRef;

}

public class ConstructorData
{
    public bool isPrivate;
    public CodeData script = new();

    public ConstructorBuilder? constructorRef;
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

public class CodeData
{
    public string[] labels = [];
    public DrasmOperation[] script = [];
}
