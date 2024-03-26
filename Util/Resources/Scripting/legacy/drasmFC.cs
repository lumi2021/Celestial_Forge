using System.Reflection;
using System.Reflection.Emit;
using GameEngine.Util.Resources.Scripting;

namespace GameEngine.Core.Scripting;

//TOFIX rework on all this

public static class DrasmFinalCompiler
{

    static readonly AssemblyName asmName = new();

    #region execution data
    private static LocalBuilder? _rtv;
    private static Type? _rtv_type;

    private static dynamic? _selected_val;
    private static DrasmParameterTypes _selected_type;
    private static dynamic? _comparing_val;
    private static DrasmParameterTypes _comparing_type;
    #endregion

    private static TypeBuilder? _currentType = null;

    private static Dictionary<string, FieldData> _fields = [];
    private static Dictionary<string, MethodData> _methods = [];

    private static Dictionary<string, LocalBuilder> _scopeData = [];
    private static Dictionary<string, Label> _scopeLabels = [];
    private static List<Label> _scopeLoops = [];

    private static readonly List<DrasmParameterTypes> _DrasmStack = [];
    private static readonly List<Type?> _DotnetStack = [];

    private static Assembly? lastAsm;

    static DrasmFinalCompiler()
    {
        asmName.Name = "DynamicAss";
    }

    public static void Compile(ScriptData script)
    {

        try {
            AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule("UserContent");

            lastAsm = moduleBuilder.Assembly;

            Type? dynamicClass = null;

            foreach (var classData in script.classes)
            {

                TypeBuilder nType = moduleBuilder.DefineType(classData.name, TypeAttributes.Public);
                _currentType = nType;

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

                        if (!f.Value.fieldType.IsArray)
                        {
                            LoadInStack(value, ilGen);
                            ilGen.Emit(OpCodes.Stfld, fieldRef);
                            PopFromStack();
                        }
                        else
                        {
                            var elType = f.Value.fieldType.GetElementType()!;
                            if (value is int v)
                            {
                                ilGen.Emit(OpCodes.Ldc_I4, v);
                                ilGen.Emit(OpCodes.Newarr, elType);
                                ilGen.Emit(OpCodes.Stfld, fieldRef);
                            }
                            else if (value.GetType().IsArray)
                            {   
                                ilGen.Emit(OpCodes.Ldc_I4, value.Length);
                                ilGen.Emit(OpCodes.Newarr, elType);

                                for (var i = 0; i < value.Length; i++)
                                {
                                    ilGen.Emit(OpCodes.Dup);
                                    ilGen.Emit(OpCodes.Ldc_I4, i);
                                    LoadInStack(value[i], ilGen);
                                    ilGen.Emit(OpCodes.Stelem, elType!);
                                    PopFromStack();
                                }

                                ilGen.Emit(OpCodes.Stfld, fieldRef);
                            }
                        }
                    }

                    // script code
                    BuildIl(ilGen, ctrData.script);
                }

                /* COMPILE CLASS METHODS */
                foreach (var mtdData in _methods)
                {
                    MethodBuilder methodBuilder = mtdData.Value.methodRef!;

                    ILGenerator ilGen = methodBuilder.GetILGenerator();

                    BuildIl(ilGen, mtdData.Value.script);
                }

                _currentType = null;
                dynamicClass = nType.CreateType();

            }

            try
            {
                dynamic instance = Activator.CreateInstance(dynamicClass!)!;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.InnerException!.Message);
                Console.WriteLine("Printing assembly...\n");
                LogAssembly(lastAsm);
            }

        }
        catch(Exception e)
        {
            Console.WriteLine("Error while compiling! (step 2/2)");
            Console.WriteLine(e);
        }
        finally {
            GeneralCleanUp();
        }

    }

    private static void BuildIl(ILGenerator ilGen, CodeData code)
    {

        Label[] opRefs = new Label[code.script.Length+1];
        var returned = false;

        foreach (var i in code.labels)
            _scopeLabels.Add(i, ilGen.DefineLabel());
        for (int i = 0; i <= code.script.Length; i++)
        {
            opRefs[i] = ilGen.DefineLabel();
        }
        
        for (int lineIndex = 0; lineIndex < code.script.Length; lineIndex++)
        {
            var i = code.script[lineIndex];

            //Console.WriteLine("A:\t{0}", _DrasmStack.Count);

            ilGen.MarkLabel(opRefs[lineIndex]);
            switch (i.operation)
            {
                // data
                case DrasmOperations.op_select:
                    SelectValue(i.args[0].value, i.args[0].type, ilGen);
                    break;
                case DrasmOperations.op_compare:
                    CompareValue(i.args[0].value, i.args[0].type, ilGen);
                    break;
                case DrasmOperations.op_define:
                    if (i.ArgCount == 3 && i.args[1].type == DrasmParameterTypes.pt_identifier && i.args[1].value == "as")
                    {
                        if (Type.GetType(i.args[2].value) != null)
                        {
                            LocalBuilder newVar = ilGen.DeclareLocal(GetReferencedType(i.args[2].value));
                            _scopeData.Add(i.args[0].value, newVar);
                        }
                        else throw new Exception($"Inexistent type \"{i.args[2].value}\" reference");
                    }
                    else throw new Exception("Op_define can only accept 3 arguments, being the seccond the operator \"as\"");
                    
                    break;
                case DrasmOperations.op_set:
                    if (i.ArgCount == 2)
                    {
                        if (_scopeData.ContainsKey(i.args[0].value))
                        {
                            LoadInStack(i.args[1].value, i.args[1].type, ilGen);

                            var lType = _scopeData[i.args[0].value].LocalType;
                            
                            if (lType == typeof(int)) ilGen.Emit(OpCodes.Conv_I4);

                            ilGen.Emit(OpCodes.Stloc, _scopeData[i.args[0].value]);
                            PopFromStack();
                        }
                        else if (_fields.ContainsKey(i.args[0].value))
                        {
                            ilGen.Emit(OpCodes.Ldarg_0);
                            LoadInStack(i.args[1].value, i.args[1].type, ilGen);
                            ilGen.Emit(OpCodes.Stfld, _fields[i.args[0].value].fieldRef!);
                            PopFromStack();
                        }
                        else throw new Exception($"Inexistent field \"{i.args[0].value}\"");
                    }
                    else throw new Exception("Op_set can only accept 2 arguments");
                    break;
                case DrasmOperations.op_delete:
                    if (_scopeData.ContainsKey(i.args[0].value))
                    {
                        _scopeData.Remove(i.args[0].value);
                    }
                    else throw new Exception($"Inexistent local variable \"{i.args[0].value}\"");

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
                    _DrasmStack.Add(DrasmParameterTypes.pt_number_single);
                    _DotnetStack.Add(typeof(float));
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
                    _DrasmStack.Add(DrasmParameterTypes.pt_number_single);
                    _DotnetStack.Add(typeof(float));
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
                    _DrasmStack.Add(DrasmParameterTypes.pt_number_single);
                    _DotnetStack.Add(typeof(float));
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
                    _DrasmStack.Add(DrasmParameterTypes.pt_number_single);
                    _DotnetStack.Add(typeof(float));
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
                    _DrasmStack.Add(DrasmParameterTypes.pt_number_single);
                    _DotnetStack.Add(typeof(float));
                    SetValueInRTV(ilGen, typeof(float));

                    break;

                // process
                case DrasmOperations.op_call:
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.EmitCall(OpCodes.Call, _methods[i.args[0].value].methodRef, Type.EmptyTypes);
                    break;
                case DrasmOperations.op_return:
                    ilGen.Emit(OpCodes.Ret);
                    returned = true;
                    break;
                case DrasmOperations.op_goto:
                    ilGen.Emit(OpCodes.Br_S, _scopeLabels[i.args[0].value]);
                    break;
                case DrasmOperations.op_nop:
                    ilGen.Emit(OpCodes.Nop);
                    break;
                case DrasmOperations.op_loop:
                    _scopeLoops.Add(opRefs[lineIndex]);
                    break;
                case DrasmOperations.op_doop:
                    ilGen.Emit(OpCodes.Br_S, _scopeLoops[^1]);
                    break;
                case DrasmOperations.op_break:
                    _scopeLoops.Pop();
                    break;

                // conditional
                case DrasmOperations.op_istrue:
                    LoadComparing(ilGen);
                    ilGen.Emit(OpCodes.Brfalse_S, opRefs[lineIndex+2]);
                    PopFromStack();
                    break;
                case DrasmOperations.op_isfalse:
                    LoadComparing(ilGen);
                    ilGen.Emit(OpCodes.Brtrue_S, opRefs[lineIndex+2]);
                    PopFromStack();
                    break;
                case DrasmOperations.op_iseqls:
                    LoadComparing(ilGen);
                    LoadInStack(i.args[0].value, i.args[0].type, ilGen);

                    ilGen.Emit(OpCodes.Ceq);
                    ilGen.Emit(OpCodes.Brfalse_S, opRefs[lineIndex+2]);
                    PopFromStack(2);
                    break;
                case DrasmOperations.op_isneqls:
                    LoadComparing(ilGen);
                    LoadInStack(i.args[0].value, i.args[0].type, ilGen);

                    ilGen.Emit(OpCodes.Ceq);
                    ilGen.Emit(OpCodes.Brtrue_S, opRefs[lineIndex+2]);
                    PopFromStack(2);
                    break;
                case DrasmOperations.op_islstn:
                    LoadComparing(ilGen);
                    LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                    ilGen.Emit(OpCodes.Bge_S, opRefs[lineIndex+2]);
                    PopFromStack(2);
                    break;
                case DrasmOperations.op_isgrtn:
                    LoadComparing(ilGen);
                    LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                    ilGen.Emit(OpCodes.Ble_S, opRefs[lineIndex+2]);
                    PopFromStack(2);
                    break;

                // output
                case DrasmOperations.op_write:
                {
                    // args -> any : target2Log
                    if (i.ArgCount > 0)
                    {
                        LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                        var mi = typeof(Console).GetMethod("Write", [ _DotnetStack[^1]! ]);
                        ilGen.EmitCall(OpCodes.Call, mi!, [ _DotnetStack[^1]! ]);
                        PopFromStack();
                    }

                    break;
                }
                case DrasmOperations.op_print:
                    // args -> any : target2Log
                    if (i.ArgCount > 0)
                    {
                        LoadInStack(i.args[0].value, i.args[0].type, ilGen);
                        var mi = typeof(Console).GetMethod("WriteLine", [ _DotnetStack[^1]! ]);
                        ilGen.EmitCall(OpCodes.Call, mi!, [ _DotnetStack[^1]! ]);
                        PopFromStack();
                    }

                    break;
            
                // misc
                case DrasmOperations.def_label:
                    if (_scopeLabels.ContainsKey(i.args[0].value))
                        ilGen.MarkLabel(_scopeLabels[i.args[0].value]);

                    else throw new Exception(string.Format("Already declared label \"{0}\"", i.args[0].value));

                    break;

                // error
                default:
                    throw new Exception(string.Format("Undefined operator {0}", i.operation));
            }

            //Console.WriteLine("B:\t{0}", _DrasmStack.Count);

        }
        if (!returned) ilGen.Emit(OpCodes.Ret);

        CleanUp();

    }
    
    private static void LoadInStack(dynamic? value, DrasmParameterTypes type, ILGenerator il)
    {

        Type? netType = ParamType2DotNetType(type) ?? value?.GetType();

        switch (type)
        {
            case DrasmParameterTypes.pt_null:
                il.Emit(OpCodes.Ldnull); break;

            case DrasmParameterTypes.pt_identifier:
                netType = LoadReference(((string)value!).Split('.'), false, null, il);
                type = DotNetType2ParamType(netType);
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

        _DrasmStack.Add(type);
        _DotnetStack.Add(netType);
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

        _DrasmStack.Add( DotNetType2ParamType(type) );
        _DotnetStack.Add(type);

    }
    private static void LoadSelected(ILGenerator il)
    {
        LoadInStack(_selected_val, _selected_type, il);
    }
    private static void LoadComparing(ILGenerator il)
    {
        LoadInStack(_comparing_val, _comparing_type, il);
    }

    private static Type? LoadReference(string[] path, bool insideSelf, Type? from, ILGenerator il)
    {

        if (path[0] == "this")
        {
            if (from == null && !insideSelf)
            {
                il.Emit(OpCodes.Ldarg_0);
                insideSelf = true;
            }
            else throw new Exception("You can't use the keyword \"this\" inside another reference!");
        }

        else if (from == null && insideSelf)
        {
            var field = _fields[path[0]].fieldRef!;

            if (field != null)
            {
                il.Emit(OpCodes.Ldfld, field);
                from = field.FieldType;
            }
            
            else throw new Exception(string.Format("Field \"{0}\" don't exist in base {1}!", path[0], from));
        }

        else if (from != null)
        {
            if (path[0].StartsWith('[') && path[0].EndsWith(']'))
            {
                if (from.IsArray)
                {
                    if (int.TryParse(path[0][1 .. ^1], out int index))
                    {
                        from = from.GetElementType()!;
                        il.Emit(OpCodes.Ldc_I4, index);
                        il.Emit(OpCodes.Ldelem, from);
                    }
                    else 
                    {
                        LoadReference(path[0][1 .. ^1].Split('.'), false, null, il);
                        from = from.GetElementType()!;
                        il.Emit(OpCodes.Ldelem, from);
                    }
                }
            }
            else if (from.GetMember(path[0]) != null)
            {
                FieldInfo? fi = from.GetField(path[0]);

                if (fi != null)
                {
                    il.Emit(OpCodes.Ldfld, fi);
                    from = fi.FieldType;
                }
                else
                {
                    PropertyInfo? pi = from.GetProperty(path[0]);
                    if (pi != null)
                    {
                        il.Emit(OpCodes.Call, pi.GetMethod!);
                        from = pi.PropertyType;
                    }
                }
            }
            else throw new Exception(string.Format("Field \"{0}\" don't exist in base {1}!", path[0], from));
        }

        else if (from == null && !insideSelf)
        {
            if (_scopeData.ContainsKey(path[0]))
            {
                LocalBuilder lb = _scopeData[path[0]]!;
                il.Emit(OpCodes.Ldloc, lb);
                from = lb.LocalType;
            }
            else throw new Exception(string.Format("Field \"{0}\" does not exist in this scope!", path[0]));
        }

        if (path.Length > 1)
            return LoadReference(path[1 ..], insideSelf, from, il);
        else return from;

    }
    private static Type? GetReferencedType(string path)
    {
        return Type.GetType(path);
    }

    private static void SelectValue(dynamic? val, DrasmParameterTypes t, ILGenerator il)
    {
        _selected_val = val;
        _selected_type = t;
    }
    private static void CompareValue(dynamic? val, DrasmParameterTypes t, ILGenerator il)
    {
        _comparing_val = val;
        _comparing_type = t;
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
        {
            _DrasmStack.RemoveAt(_DrasmStack.Count-1);
            _DotnetStack.RemoveAt(_DotnetStack.Count-1);
        }
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

            _ => null //throw new NotImplementedException()
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

        else if (t.IsArray) rtn = DrasmParameterTypes.pt_array;

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

    private static void GeneralCleanUp()
    {
        CleanUp();

        _fields.Clear();
        _methods.Clear();
    }
    private static void CleanUp()
    {
        _rtv = null;
        _rtv_type = null;
        _selected_val = null;
        _selected_type = DrasmParameterTypes._undefined;
        _comparing_val = null;
        _comparing_type = DrasmParameterTypes._undefined;
        _DrasmStack.Clear();
        _scopeData.Clear();
        _scopeLabels.Clear();

        _scopeLoops.Clear();
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