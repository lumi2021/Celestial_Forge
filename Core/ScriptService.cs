using System.Reflection;
using System.Reflection.Emit;
using GameEngine.Util.Resources.Scripting;

namespace GameEngine.Core.Scripting;

public static class ScriptService
{

    static AssemblyName asmName = new();

    static ScriptService()
    {
        asmName.Name = "DynamicAss";
    }

    public static void Compile(ScriptData script)
    {

        AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndCollect);
        ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule("UserContent");

        Type? dynamicClass = null;

        foreach (var classData in script.classes)
        {
            TypeBuilder nType = moduleBuilder.DefineType(classData.name, TypeAttributes.Public);

            /* CREATE CLASS FIELDS */
            foreach (var fieldData in classData.fields)
            {
                FieldBuilder fieldBuilder = nType.DefineField(fieldData.name, fieldData.fieldType, FieldAttributes.Public);
                fieldBuilder.
            }

            /* CREATE CLASS CONSTRUCTORS */
            foreach (var ctrData in classData.constructors)
            {

                ConstructorBuilder ctrBuilder = nType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);

                ILGenerator ilGen = ctrBuilder.GetILGenerator();

                // base constructor code
                ilGen.Emit(OpCodes.Ldarg_0);
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilGen.Emit(OpCodes.Call, baseConstructor);
                
                BuildIl(ilGen, ctrData.script);

                ilGen.Emit(OpCodes.Ret);

            }

            dynamicClass = nType.CreateType();

        }

        dynamic instance = Activator.CreateInstance(dynamicClass!)!;

        Console.WriteLine("{0}", instance.message);

    }

    private static void BuildIl(ILGenerator ilGen, DrasmOperation[] script)
    {
        
        foreach (var i in script)
        {
            
            switch (i.operation)
            {
                case DrasmOperations.op_print:
                    // args -> any : targetToprint
                    if (i.ArgCount > 0)
                    {
                        if (i.args[0].type == DrasmParameterTypes.pt_string)
                            ilGen.EmitWriteLine( (string) i.args[0].value! );
                    }

                    break;
            }


        }

    }

}

public enum DrasmOperations
{
    // data/memory instructions
    op_define,
    op_set,
    op_select,
    op_delete,

    // process instructions
    op_return,
    op_goto,
    op_jmp,
    op_call,
    op_loop,
    op_doop,
    op_nop,

    // conditional operators
    op_compare,
    op_istrue,
    op_isfalse,
    op_isnum,
    op_isnan,
    op_istext,
    op_iseqls,
    op_isgrtn,
    op_islstn,

    // output instructions
    op_print,
    op_write,

    // binary operators
    op_add,
    op_sub,
    op_mul,
    op_div,
}

public enum DrasmParameterTypes
{
    pt_identifier,
    pt_string,
    pt_number
}
