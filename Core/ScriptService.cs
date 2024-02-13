using System.Reflection;
using System.Reflection.Emit;
using GameEngine.Util.Resources.Scripting;

namespace GameEngine.Core.Scripting;

public static class ScriptService
{

    static readonly AssemblyName asmName = new();

    private static LocalBuilder? _rtv;
    private static Type? _rtv_type;

    private static LocalBuilder? _selected_val;
    private static Type? _selected_type;

    private static Dictionary<string, FieldData> _fields = [];
    private static Dictionary<string, MethodData> _methods = [];

    private static readonly List<DrasmParameterTypes> _stack = [];

    static ScriptService()
    {
        asmName.Name = "DynamicAss";
    }

    public static void Compile(ScriptData script)
    {

        try {
            AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule("UserContent");

            Type? dynamicClass = null;

            foreach (var classData in script.classes)
            {

                TypeBuilder nType = moduleBuilder.DefineType(classData.name, TypeAttributes.Public);


                /* DECLARE CLASS FIELDS */
                foreach (var fieldData in classData.fields)
                {
                    FieldBuilder fieldBuilder = nType.DefineField(fieldData.name, fieldData.fieldType, FieldAttributes.Public);
                    fieldData.fieldRef = fieldBuilder;
                    _fields.Add(fieldData.name, fieldData);
                }

                /* DECLARE CLASS METHODS */
                foreach (var methodData in classData.methods)
                {
                    MethodBuilder methodBuilder = nType.DefineMethod(methodData.name, MethodAttributes.Public, methodData.returnType, []);
                    methodData.methodRef = methodBuilder;
                    _methods.Add(methodData.name, methodData);
                }

                /* COMPILE CLASS CONSTRUCTORS */
                foreach (var ctrData in classData.constructors)
                {

                    ConstructorBuilder ctrBuilder = nType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);

                    ILGenerator ilGen = ctrBuilder.GetILGenerator();

                    // base constructor code
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                    ilGen.Emit(OpCodes.Call, baseConstructor);

                    // set fields initial data
                    foreach (var f in _fields)
                    {
                        dynamic? value = f.Value.defaultValue;
                        FieldBuilder fieldRef = f.Value.fieldRef!;

                        if (value == null) continue;

                        ilGen.Emit(OpCodes.Ldarg_0);

                        LoadInStack(value, ilGen);
                        ilGen.Emit(OpCodes.Stfld, fieldRef);
                        PopFromStack();
                    }

                    // script code
                    BuildIl(ilGen, ctrData.script);
                    
                    ilGen.Emit(OpCodes.Ret);
                }

                /* COMPILE CLASS METHODS */
                foreach (var mtdData in _methods)
                {
                    MethodBuilder methodBuilder = mtdData.Value.methodRef!;

                    ILGenerator ilGen = methodBuilder.GetILGenerator();

                    BuildIl(ilGen, mtdData.Value.script);
                }


                dynamicClass = nType.CreateType();

            }

            try
            {
                dynamic instance = Activator.CreateInstance(dynamicClass!)!;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.InnerException!.Message);
            }

            Console.WriteLine("\n-------------------------");
            Console.WriteLine("Printing assembly...\n");
            LogAssembly(dynamicClass!.Assembly);
        
        }
        catch(Exception e)
        {
            Console.WriteLine("Error while compiling! (step 2/2)");
            Console.WriteLine(e);
        }
        finally {
            CleanUp();
        }

    }

    private static void BuildIl(ILGenerator ilGen, DrasmOperation[] script)
    {
        
        foreach (var i in script)
        {

            Console.WriteLine("A:\t{0}", _stack.Count);

            switch (i.operation)
            {
                // data
                case DrasmOperations.op_select:

                    Type type = i.args[0].value!.GetType();
                    LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                    SelectValue(ilGen, type);

                    break;

                // math
                case DrasmOperations.op_add:
                    // args -> op2 : number
                    //         op1 : number, op2: number

                    if (i.args.Length > 1)
                    {
                        if (
                            DiscoverType(i.args[0].value, i.args[0].type) == DrasmParameterTypes.pt_number_single ||
                            DiscoverType(i.args[1].value, i.args[1].type) == DrasmParameterTypes.pt_number_single
                        )
                        {
                            LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                            if (DiscoverType(i.args[0].value, i.args[0].type) == DrasmParameterTypes.pt_number_int)
                                ilGen.Emit(OpCodes.Conv_R4);

                            LoadInStack(i.args[1].value, i.args[1].type, ilGen);
                            if (DiscoverType(i.args[1].value, i.args[1].type) == DrasmParameterTypes.pt_number_int)
                                ilGen.Emit(OpCodes.Conv_R4);
                        }
                        else
                        {
                            LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                            LoadInStack(i.args[1].value, i.args[1].type, ilGen);
                        }
                    }
                    else {
                        LoadSelected(ilGen);
                        LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                    }

                    ilGen.Emit(OpCodes.Add);
                    PopFromStack(2);
                    _stack.Add(DrasmParameterTypes.pt_number_single);
                    SetValueInRTV(ilGen, typeof(float));

                    break;
                case DrasmOperations.op_sub:
                    // args -> op2 : number
                    //         op1 : number, op2: number

                    if (i.args.Length > 1)
                    {
                        if (
                            DiscoverType(i.args[0].value, i.args[0].type) == DrasmParameterTypes.pt_number_single ||
                            DiscoverType(i.args[1].value, i.args[1].type) == DrasmParameterTypes.pt_number_single
                        )
                        {
                            LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                            if (DiscoverType(i.args[0].value, i.args[0].type) == DrasmParameterTypes.pt_number_int)
                                ilGen.Emit(OpCodes.Conv_R4);

                            LoadInStack(i.args[1].value, i.args[1].type, ilGen);
                            if (DiscoverType(i.args[1].value, i.args[1].type) == DrasmParameterTypes.pt_number_int)
                                ilGen.Emit(OpCodes.Conv_R4);
                        }
                        else
                        {
                            LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                            LoadInStack(i.args[1].value, i.args[1].type, ilGen);
                        }
                    }
                    else {
                        LoadSelected(ilGen);
                        LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                    }

                    ilGen.Emit(OpCodes.Sub);
                    PopFromStack(2);
                    _stack.Add(DrasmParameterTypes.pt_number_single);
                    SetValueInRTV(ilGen, typeof(float));

                    break;
                case DrasmOperations.op_mul:
                    // args -> op2 : number
                    //         op1 : number, op2: number

                    if (i.args.Length > 1)
                    {
                        if (
                            DiscoverType(i.args[0].value, i.args[0].type) == DrasmParameterTypes.pt_number_single ||
                            DiscoverType(i.args[1].value, i.args[1].type) == DrasmParameterTypes.pt_number_single
                        )
                        {
                            LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                            if (DiscoverType(i.args[0].value, i.args[0].type) == DrasmParameterTypes.pt_number_int)
                                ilGen.Emit(OpCodes.Conv_R4);

                            LoadInStack(i.args[1].value, i.args[1].type, ilGen);
                            if (DiscoverType(i.args[1].value, i.args[1].type) == DrasmParameterTypes.pt_number_int)
                                ilGen.Emit(OpCodes.Conv_R4);
                        }
                        else
                        {
                            LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                            LoadInStack(i.args[1].value, i.args[1].type, ilGen);
                        }
                    }
                    else {
                        LoadSelected(ilGen);
                        LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                    }

                    ilGen.Emit(OpCodes.Mul);
                    PopFromStack(2);
                    _stack.Add(DrasmParameterTypes.pt_number_single);
                    SetValueInRTV(ilGen, typeof(float));

                    break;
                case DrasmOperations.op_div:
                    // args -> op2 : number
                    //         op1 : number, op2: number

                    if (i.args.Length > 1)
                    {
                        if (
                            DiscoverType(i.args[0].value, i.args[0].type) == DrasmParameterTypes.pt_number_single ||
                            DiscoverType(i.args[1].value, i.args[1].type) == DrasmParameterTypes.pt_number_single
                        )
                        {
                            LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                            if (DiscoverType(i.args[0].value, i.args[0].type) == DrasmParameterTypes.pt_number_int)
                                ilGen.Emit(OpCodes.Conv_R4);

                            LoadInStack(i.args[1].value, i.args[1].type, ilGen);
                            if (DiscoverType(i.args[1].value, i.args[1].type) == DrasmParameterTypes.pt_number_int)
                                ilGen.Emit(OpCodes.Conv_R4);
                        }
                        else
                        {
                            LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                            LoadInStack(i.args[1].value, i.args[1].type, ilGen);
                        }
                    }
                    else {
                        LoadSelected(ilGen);
                        LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                    }

                    ilGen.Emit(OpCodes.Div);
                    PopFromStack(2);
                    _stack.Add(DrasmParameterTypes.pt_number_single);
                    SetValueInRTV(ilGen, typeof(float));

                    break;
                case DrasmOperations.op_rem:
                    // args -> op2 : number
                    //         op1 : number, op2: number

                    if (i.args.Length > 1)
                    {
                        if (
                            DiscoverType(i.args[0].value, i.args[0].type) == DrasmParameterTypes.pt_number_single ||
                            DiscoverType(i.args[1].value, i.args[1].type) == DrasmParameterTypes.pt_number_single
                        )
                        {
                            LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                            if (DiscoverType(i.args[0].value, i.args[0].type) == DrasmParameterTypes.pt_number_int)
                                ilGen.Emit(OpCodes.Conv_R4);

                            LoadInStack(i.args[1].value, i.args[1].type, ilGen);
                            if (DiscoverType(i.args[1].value, i.args[1].type) == DrasmParameterTypes.pt_number_int)
                                ilGen.Emit(OpCodes.Conv_R4);
                        }
                        else
                        {
                            LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                            LoadInStack(i.args[1].value, i.args[1].type, ilGen);
                        }
                    }
                    else {
                        LoadSelected(ilGen);
                        LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                    }

                    ilGen.Emit(OpCodes.Rem);
                    PopFromStack(2);
                    _stack.Add(DrasmParameterTypes.pt_number_single);
                    SetValueInRTV(ilGen, typeof(float));

                    break;

                // process
                case DrasmOperations.op_call:
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.EmitCall(OpCodes.Call, _methods[i.args[0].value].methodRef, Type.EmptyTypes);
                    break;
                case DrasmOperations.op_return:
                    ilGen.Emit(OpCodes.Ret);
                    break;

                // output
                case DrasmOperations.op_write:
                {
                    // args -> any : target2Log
                    if (i.ArgCount > 0)
                    {
                        LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                        var mi = typeof(Console).GetMethod("Write", [ ParamType2DotNetType(_stack.Last<DrasmParameterTypes>())! ]);
                        ilGen.EmitCall(OpCodes.Call, mi!, [ ParamType2DotNetType(_stack.Last<DrasmParameterTypes>())! ]);
                        PopFromStack();
                    }

                    break;
                }
                case DrasmOperations.op_print:
                    // args -> any : target2Log
                    if (i.ArgCount > 0)
                    {
                        LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                        var mi = typeof(Console).GetMethod("WriteLine", [ ParamType2DotNetType(_stack.Last<DrasmParameterTypes>())! ]);
                        ilGen.EmitCall(OpCodes.Call, mi!, [ ParamType2DotNetType(_stack.Last<DrasmParameterTypes>())! ]);
                        PopFromStack();
                    }

                    break;
            }

            Console.WriteLine("B:\t{0}", _stack.Count);

        }

        _rtv = null;
        _rtv_type = null;
        _selected_val = null;
        _selected_type = null;
        _stack.Clear();

    }
    
    private static void LoadInStack(dynamic? value, DrasmParameterTypes type, ILGenerator il)
    {

        switch (type)
        {
            case DrasmParameterTypes.pt_null:
                il.Emit(OpCodes.Ldnull); break;

            case DrasmParameterTypes.pt_identifier:
                var fi = _fields[(string)value!].fieldRef;
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fi!);
                type = DotNetType2ParamType(fi!.FieldType);
                break;

            case DrasmParameterTypes.pt_boolean:
                il.Emit(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0); break;

            case DrasmParameterTypes.pt_string:
                il.Emit(OpCodes.Ldstr, value); break;
            
            case DrasmParameterTypes.pt_number_byte:
                il.Emit(OpCodes.Ldc_I4_S, value);
                il.Emit(OpCodes.Conv_U1);
                break;

            case DrasmParameterTypes.pt_number_int:
                il.Emit(OpCodes.Ldc_I4, value); break;

            case DrasmParameterTypes.pt_number_single:
                il.Emit(OpCodes.Ldc_R4, value); break;

            case DrasmParameterTypes.pt_number_double:
                il.Emit(OpCodes.Ldc_R8, value); break;

            case DrasmParameterTypes.pt_data_returnValue:
                il.Emit(OpCodes.Ldloc, _rtv!);
                il.Emit(OpCodes.Unbox_Any, _rtv_type!);
                break;

            default:
                throw new NotImplementedException();
        }

        _stack.Add(type);
    }
    private static void LoadInStack(dynamic? value, ILGenerator il)
    {
        
        Type? type = value?.GetType();

        if (type == null) il.Emit(OpCodes.Ldnull);

        else if (type == typeof(int))    il.Emit(OpCodes.Ldc_I4, value);
        else if (type == typeof(float))  il.Emit(OpCodes.Ldc_R4, value);
        else if (type == typeof(double)) il.Emit(OpCodes.Ldc_R8, value);
        else if (type == typeof(bool))   il.Emit(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        else if (type == typeof(char))   il.Emit(OpCodes.Ldc_I4, value);
        else if (type == typeof(byte))
        {
            il.Emit(OpCodes.Ldc_I4_S, value);
            il.Emit(OpCodes.Conv_U1);
        }
        else if (type == typeof(string)) il.Emit(OpCodes.Ldstr, value);

        _stack.Add( DotNetType2ParamType(type) );

    }
    private static void LoadSelected(ILGenerator il)
    {
        il.Emit(OpCodes.Ldloc, _selected_val!);
        il.Emit(OpCodes.Unbox_Any, _selected_type!);
        _stack.Add( DotNetType2ParamType(_selected_type) );
    }

    private static void SelectValue(ILGenerator il, Type t)
    {
        _selected_val ??= il.DeclareLocal(typeof(object));
        _selected_type = t;
        il.Emit(OpCodes.Box, t);
        il.Emit(OpCodes.Stloc, _selected_val);
        PopFromStack();
    }
    private static void SetValueInRTV(ILGenerator il, Type t)
    {
        _rtv ??= il.DeclareLocal(typeof(object));
        _rtv_type = t;
        il.Emit(OpCodes.Box, t);
        il.Emit(OpCodes.Stloc, _rtv);
        PopFromStack();
    }

    private static void PopFromStack(int count = 1)
    {
        for (int i = 0; i < count; i++)
            _stack.RemoveAt(_stack.Count-1);
    }

    private static Type? ParamType2DotNetType(DrasmParameterTypes t)
    {
        return t switch
        {
            DrasmParameterTypes.pt_null => null,

            DrasmParameterTypes.pt_string => typeof(string),
            DrasmParameterTypes.pt_boolean => typeof(bool),

            DrasmParameterTypes.pt_number_byte => typeof(byte),
            DrasmParameterTypes.pt_number_int => typeof(int),
            DrasmParameterTypes.pt_number_single => typeof(float),
            DrasmParameterTypes.pt_number_double => typeof(double),

            DrasmParameterTypes.pt_data_returnValue => _rtv_type,

            _ => throw new NotImplementedException()
        };
    }
    private static DrasmParameterTypes DotNetType2ParamType(Type? t)
    {

        DrasmParameterTypes rtn = DrasmParameterTypes._undefined;

        if (t == null) rtn = DrasmParameterTypes.pt_null;

        else if (t == typeof(string)) rtn = DrasmParameterTypes.pt_string;
        else if (t == typeof(bool)) rtn = DrasmParameterTypes.pt_boolean;

        else if (t == typeof(byte)) rtn = DrasmParameterTypes.pt_number_byte;
        else if (t == typeof(int)) rtn = DrasmParameterTypes.pt_number_int;
        else if (t == typeof(float)) rtn = DrasmParameterTypes.pt_number_single;
        else if (t == typeof(double)) rtn = DrasmParameterTypes.pt_number_double;

        return rtn;

    }
    private static DrasmParameterTypes DiscoverType(dynamic value, DrasmParameterTypes t)
    {
        if (t != DrasmParameterTypes.pt_identifier && t != DrasmParameterTypes.pt_data_returnValue)
            return t;
        
        if (t == DrasmParameterTypes.pt_data_returnValue)
            return DotNetType2ParamType(_rtv_type);
        
        else return t;
    }

    private static void CleanUp()
    {
        _rtv = null;
        _rtv_type = null;
        _selected_val = null;
        _selected_type = null;
        _stack.Clear();

        _fields.Clear();
        _methods.Clear();
    }

    private static void LogAssembly(Assembly assembly)
    {
        Console.WriteLine("Metadata and IL of {0}:", assembly);
        Type[] classes = assembly.GetTypes();

        foreach(Type c in classes)
        {
            Console.WriteLine("Type {0}:", c.FullName);

            Console.WriteLine("\n");
            Console.WriteLine("# Fields ({0}):", c.GetFields().Length);
            foreach (FieldInfo field in c.GetFields())
            {
                Console.WriteLine("\t{1} {0};", field.Name, field.FieldType);
            }

            Console.WriteLine("\n________________________________");
            Console.WriteLine("# Constructors ({0}):", c.GetConstructors().Length);
            foreach (ConstructorInfo ctor in c.GetConstructors())
            {
                Console.WriteLine("{0} parameters:", ctor.GetParameters().Length);
                Console.WriteLine("{\n");

                foreach (var instruc in Mono.Reflection.Disassembler.GetInstructions(ctor))
                    Console.WriteLine("  {0}",instruc.ToString());

                Console.WriteLine("\n}");
            }
            
            Console.WriteLine("\n________________________________");
            Console.WriteLine("# Methods ({0}):", c.GetMethods().Length);
            foreach (MethodInfo method in c.GetMethods())
            {
                Console.WriteLine("{0} -> {1}:", method.Name, method.ReturnType);
                Console.WriteLine("{\n");
                MethodBody? methodBody = method.GetMethodBody();

                if (methodBody != null)
                    foreach (var instruc in Mono.Reflection.Disassembler.GetInstructions(method))
                        Console.WriteLine("  {0}",instruc.ToString());

                Console.WriteLine("\n}");
            }
        }

    }

}

public enum DrasmOperations : byte
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
    op_rem,
}

public enum DrasmParameterTypes : byte
{
    pt_identifier,
    pt_null,
    pt_string,
    pt_boolean,

    // numbers (why there's so much)
    pt_number,
    pt_number_byte,
    pt_number_int,
    pt_number_single,
    pt_number_double,

    // build-in data
    pt_data_returnValue,

    _undefined

}
