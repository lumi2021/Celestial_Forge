using System.Text.RegularExpressions;
using GameEngine.Core.Scripting;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources.Scripting;

namespace GameEngine.Util.Resources;

public class DrasmCompiler : Resource, IScriptCompiler
{
    public void Compile(string src)
    {
        string[] lines = Preprocess(src);
        var tokens = Tokenize(lines);
        var data = ConvertToData(tokens);
        ScriptService.Compile(data);
    }

    private string[] Preprocess(string src)
    {
        string[] lines = RemoveComments(src).Split('\n');

        lines = (from l in lines where l.Trim() != "" select l.Trim()).ToArray();

        return lines;
    }
    private string RemoveComments(string src)
    {
        string regex = @"//.*?\n";
        return Regex.Replace(src, regex, "");
    }

    private Token[][] Tokenize(string[] src)
    {
        char[] specialChars = new char[] { '=', '(', ')', ':' };
        
        List<List<Token>> tokens = new();

        for (int i = 0; i < src.Length; i++)
        {
            var line = src[i];
            tokens.Add([]);

            string token = "";
            bool insideString = false;
            foreach (char c in line)
            {
                if (c == '"')
                    insideString = !insideString;
                
                if (!insideString)
                {
                    if (specialChars.Contains(c))
                    {
                        if (token != "")
                        tokens[i].Add( ConvertToToken(token) );
                        tokens[i].Add( ConvertToToken("" + c) );
                        token = "";
                        continue;
                    }
                    if (c == ' ')
                    {
                        if (token != "")
                        tokens[i].Add( ConvertToToken(token) );
                        token = "";
                        continue;
                    }
                }

                token += c;
            }
            if (token != "")
            tokens[i].Add( ConvertToToken(token) );
        }

        Token[][] result = [];
        foreach (var i in tokens) result = result.Append(i.ToArray()).ToArray();
        return result;
    }

    private Token ConvertToToken(dynamic value)
    {
        dynamic finalValue = value;

        var type = value switch
        {
            "class"     => TokenType.ClassDef,
            "field"     => TokenType.FieldDef,
            "func"      => TokenType.FuncDef,

            "public" or
            "private" or
            "abstract" or
            "override"  => TokenType.Attribute,

            "end"       => TokenType.RegionEnd,

            "="         => TokenType.char_equal,
            ":"         => TokenType.char_colom,
            ">"         => TokenType.char_greaterThan,
            "("         => TokenType.char_Rbracked,
            ")"         => TokenType.char_Lbracked,        

            _           => TokenType.Identfier
        };
        if (value.StartsWith("\"") && value.EndsWith("\""))
        {
            finalValue = ((string) value)[1..^1];
            type = TokenType.StringValue;
        }
        
        if (double.TryParse(((string)value).Replace(".", ","), out double numericValue))
        {
            if (numericValue == Math.Floor(numericValue))
            {
                type = TokenType.IntNumberValue;
                finalValue = (int) numericValue;
            }
            else
            {
                type = TokenType.FloatNumberValue;
                finalValue = (float) numericValue;
            }
        }

        var a = new Token(type, finalValue);

        return a;
    }

    private ScriptData ConvertToData(Token[][] script)
    {
        ScriptData newAsm = new();

        ClassData? insideClass = null;
        MethodData? insideMethod = null;
        ConstructorData? insideConstructor = null;

        foreach (var line in script)
        switch (line[0].value)
        {
            case "class":
                if (insideClass == null)
                {
                    ClassData nClass = new();
                    nClass.name = line[1].value;
                    
                    var atributes = (from e in line where e.type == TokenType.Attribute
                    select e.value).ToArray();

                    nClass.isPrivate  = !atributes.Contains("public");
                    nClass.isAbstract = atributes.Contains("abstract");

                    newAsm.classes = [.. newAsm.classes, nClass];
                    insideClass = nClass;
                }
                else throw new ApplicationException(
                    "Compilation error! A class can't be declarated inside another!");
                
                break;
            case "func":
                if (insideMethod == null && insideConstructor == null && insideClass != null)
                {
                    MethodData nMethod = new();
                    nMethod.name = line[1].value;

                    var atributes = (from e in line where e.type == TokenType.Attribute
                    select e.value).ToArray();

                    nMethod.isPrivate  = !atributes.Contains("public");
                    insideClass.methods = [.. insideClass.methods, nMethod];

                    insideMethod = nMethod;
                }
                else throw new ApplicationException("Compilation Error! invalid definition of a method!");

                break;
            case "constructor":
                if (insideMethod == null && insideConstructor == null && insideClass != null)
                {
                    ConstructorData nConstruc = new();

                    var atributes = (from e in line where e.type == TokenType.Attribute
                    select e.value).ToArray();

                    nConstruc.isPrivate  = !atributes.Contains("public");
                    insideClass.constructors = [.. insideClass.constructors, nConstruc];

                    insideConstructor = nConstruc;
                }
                else throw new ApplicationException("Compilation Error! invalid definition of a constructor!");

                break;
        
            case "field":
                if (insideClass != null && insideMethod == null && insideConstructor == null)
                {

                    FieldData nField = new();
                    nField.name = line[1].value;

                    if (line[2].type == TokenType.char_colom)
                        nField.fieldType = Type.GetType( (string) line[3].value )!;
                    
                    List<string> attributes = [];
                    foreach (var t in line[4 ..])
                    {
                        if (t.value == "=") break;
                        attributes.Add((string) t.value);
                    }
                    nField.isPrivate = !attributes.Contains("private");

                    int valueIdx = Array.FindIndex(line, e => e.type == TokenType.char_equal) + 1;

                    nField.defaultValue = line[valueIdx].value;

                    insideClass.fields = [.. insideClass.fields, nField];

                }
                break;

            case "end":
                if (insideMethod != null) insideMethod = null;
                else if (insideConstructor != null) insideConstructor = null;
                else if (insideClass != null) insideClass = null;
                
                break;
        
            default:
                var DrasmOps = Enum.GetNames(typeof(DrasmOperations));
                if (DrasmOps.Contains("op_" + ((string)line[0].value).ToLower()))
                {
                    List<Token> args = line.Skip(1).ToList();

                    var op = new DrasmOperation()
                    {
                    operation = (DrasmOperations)
                    Enum.Parse(typeof(DrasmOperations), "op_" + ((string)line[0].value).ToLower()),
                    args = []
                    };

                    foreach (var a in args)
                    {
                        OpArg argument = new();
                        #region
                        argument.type = a.type switch
                        {
                            TokenType.Identfier        => DrasmParameterTypes.pt_identifier,
                            TokenType.StringValue      => DrasmParameterTypes.pt_string,
                            TokenType.IntNumberValue   => DrasmParameterTypes.pt_number_int,
                            TokenType.FloatNumberValue => DrasmParameterTypes.pt_number_single,
                            _                          => throw new NotImplementedException()
                        };
                        #endregion

                        argument.value = a.value;
                        op.args = [.. op.args, argument];
                    }

                    if (insideConstructor != null) insideConstructor.script = [.. insideConstructor.script, op];
                    if (insideMethod != null) insideMethod.script = [.. insideMethod.script, op];
                }
                
                break;
        }

        return newAsm;
    }

    private struct Token
    {
        public TokenType type;
        public dynamic value;

        public Token(TokenType type, dynamic value)
        {
            this.type = type;
            this.value = value;
        }

        public override readonly string ToString()
        {
            return string.Format("{0} : {1}", type, value ?? "");
        }
    }

    private enum TokenType
    {
        ClassDef,
        FieldDef,
        FuncDef,

        Identfier,
        Attribute,

        Instruction,

        StringValue,

        IntNumberValue,
        FloatNumberValue,

        char_equal,
        char_greaterThan,
        char_colom,
        char_Rbracked,
        char_Lbracked,

        RegionEnd
    }
}
