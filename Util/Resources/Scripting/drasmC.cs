using System.Text.RegularExpressions;
using GameEngine.Core.Scripting;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources.Scripting;

namespace GameEngine.Util.Resources;

//TOFIX rework on all this
public class DrasmCompiler : Resource, IScriptCompiler
{
    public Type? Compile(string src, string sourceFile="")
    {
        try {

            string[] lines = Preprocess(src);
            var tokens = Tokenize(lines);
            var data = ConvertToData(tokens);
            //ScriptService.Compile(data);

        }
        catch (Exception e)
        {
            Console.WriteLine("Error while compiling! (step 1/2)");
            Console.WriteLine(e);
        }

        return null;
    }

    private string[] Preprocess(string src)
    {
        //string[] lines = RemoveComments(src).Split('\n');
        //lines = (from l in lines where l.Trim() != "" select l.Trim()).ToArray();
        //return lines;
        return src.Split('\n');
    }
    private string RemoveComments(string src)
    {
        string regex = @"//.*?\n";
        return Regex.Replace(src, regex, "");
    }

    private Token[][] Tokenize(string[] src)
    {
        char[] specialChars = [ '=', '(', ')', ':', '>', '[', ']' ];
        
        List<List<Token>> tokens = [];

        for (int i = 0; i < src.Length; i++)
        {
            var tmpTokens = new List<Token>();

            var line = src[i];

            int tokenStart = 0;
            string token = "";
            bool insideString = false;

            for (int j = 0; j < line.Length; j++)
            {
                var c = line[j];

                if (c == '"')
                    insideString = !insideString;

                if (line.Length > j+1 && c == '/' && line[j+1] == '/') break;
                
                if (!insideString)
                {
                    if (specialChars.Contains(c))
                    {
                        if (token != "")
                        tmpTokens.Add( ConvertToToken(token, i, tokenStart, j) );
                        tmpTokens.Add( ConvertToToken("" + c, i, tokenStart, j) );
                        token = "";
                        tokenStart = j;
                        continue;
                    }
                    if (c == ' ')
                    {
                        if (token != "")
                        tmpTokens.Add( ConvertToToken(token, i, tokenStart, j) );
                        token = "";
                        tokenStart = j;
                        continue;
                    }
                }

                token += c;
            }
            
            if (token != "")
            tmpTokens.Add( ConvertToToken(token, i, tokenStart, line.Length) );
        
            List<Token> finalTokens = [];
            for (int j = 0; j < tmpTokens.Count; j++)
            {

                /* get array size allocation definitions */
                if (tmpTokens.Count > j+4 &&
                tmpTokens[j].type == TokenType.char_Lbrace &&
                tmpTokens[j+1].type == TokenType.Identfier &&
                tmpTokens[j+1].value == "len" &&
                tmpTokens[j+2].type == TokenType.char_colom &&
                tmpTokens[j+3].type == TokenType.IntNumberValue &&
                tmpTokens[j+4].type == TokenType.char_Rbrace)
                {
                    var ntoken = new Token
                    {
                        type = TokenType.ArrayLength,
                        value = (int)tmpTokens[j + 3].value,
                        line = tmpTokens[j].line,
                        colStart = tmpTokens[j].colStart,
                        colEnd = tmpTokens[j + 4].colEnd
                    };

                    finalTokens.Add(ntoken);
                    j+= 4;
                }

                /* get an array type declaration */
                else if (tmpTokens.Count > j+2 &&
                tmpTokens[j+1].type == TokenType.char_Lbrace &&
                tmpTokens[j+2].type == TokenType.char_Rbrace)
                {
                    var tkn = tmpTokens[j];
                    tkn.value += "[]";

                    tmpTokens[j] = tkn;
                    finalTokens.Add(tmpTokens[j]);
                    j+=2;
                }

                /* get the indexer and appends it to the last identifier */
                else if (tmpTokens.Count > j + 2 &&
                finalTokens.Count > 0 &&
                finalTokens[^1].type == TokenType.Identfier &&
                tmpTokens[j].type == TokenType.char_Lbrace &&
                tmpTokens[j+2].type == TokenType.char_Rbrace)
                {
                    var tkn = finalTokens[^1];
                    tkn.value += ".[" + tmpTokens[j+1].value + "]";
                    finalTokens[^1] = tkn;
                    j += 2;
                    /*
                    else
                    {
                        var ntoken = new Token();
                        ntoken.type = TokenType.ArrayIndex;
                        ntoken.value = tmpTokens[j+1].value;
                        ntoken.line = tmpTokens[j].line;
                        ntoken.colStart = tmpTokens[j].colStart;
                        ntoken.colEnd = tmpTokens[j+2].colEnd;

                        finalTokens.Add(ntoken);
                        j += 2;
                    }
                    */
                }

                /* get compilation-time declared array lists */
                else if (tmpTokens[j].type == TokenType.char_Lbrace &&
                tmpTokens.Count > j+2 &&
                tmpTokens[j+1].type != TokenType.Identfier)
                {
                    List<dynamic> arrayData = [];

                    for (int k = j+1; k < tmpTokens.Count; k++)
                    {
                        if (tmpTokens[k].type == TokenType.char_Rbrace)
                        {
                            var nTkn = new Token();
                            nTkn.type = TokenType.ArrayList;
                            nTkn.value = arrayData.ToArray();
                            finalTokens.Add(nTkn);

                            j = k;
                        }
                        else arrayData.Add(tmpTokens[k].value);
                    }
                }

                else finalTokens.Add(tmpTokens[j]);

            }
            tokens.Add(finalTokens);

        }

        Token[][] result = [];
        foreach (var i in tokens) result = [.. result, [.. i]];
        result = result.Where(e => e.Length > 0).ToArray();
        return result;
    }

    private Token ConvertToToken(dynamic value, int line, int s, int e)
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
            "("         => TokenType.char_Rbracket,
            ")"         => TokenType.char_Lbracket,  
            "["         => TokenType.char_Lbrace,
            "]"         => TokenType.char_Rbrace,

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
        if (value == "true" || value == "false")
        {
            type = TokenType.BooleanValue;
            finalValue = value == "true";
        }

        var a = new Token(type, finalValue)
        {
            line = line,
            colStart = s,
            colEnd = e
        };

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
                    {
                        nField.fieldType = GetReferencedType(line[3].value)!;

                        if (nField.fieldType == null)
                        throw new Exception(string.Format("Type {0} don't exist!",
                        (string)line[3].value.value.Replace(' ', '.')));
                    }
                    else throw new Exception("Type expected when declaring a field!");
                    
                    List<string> attributes = [];
                    foreach (var t in line[4 ..])
                    {
                        if (t.type == TokenType.char_equal) break;
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

                        if (a.type == TokenType.Identfier && a.value.StartsWith("%"))
                        {
                            if (a.value == "%rtv")
                                argument.type = DrasmParameterTypes.pt_data_returnValue;
                            
                        }
                        else
                        {
                            argument.type = a.type switch
                            {
                                TokenType.Identfier => DrasmParameterTypes.pt_identifier,
                                TokenType.StringValue => DrasmParameterTypes.pt_string,
                                TokenType.BooleanValue => DrasmParameterTypes.pt_boolean,
                                TokenType.IntNumberValue => DrasmParameterTypes.pt_number_int,
                                TokenType.FloatNumberValue => DrasmParameterTypes.pt_number_single,
                                TokenType.ArrayIndex => DrasmParameterTypes.pt_arrayIndex,
                                TokenType.ArrayList => DrasmParameterTypes.pt_arrayList,
                                _ => throw new NotImplementedException(
                                    string.Format("{0} ({1}:{2})", a.type.ToString(), a.line, a.colStart))
                            };

                            argument.value = a.value;
                        }

                        op.args = [.. op.args, argument];
                    }

                    if (insideConstructor != null) insideConstructor.script.script = [.. insideConstructor.script.script, op];
                    if (insideMethod != null) insideMethod.script.script = [.. insideMethod.script.script, op];
                }

                else if (line[0].type == TokenType.char_greaterThan)
                {
                    if (line.Length == 3)
                    {
                        var op = new DrasmOperation()
                        {
                            operation = DrasmOperations.def_label,
                            args = [
                                new()
                                {
                                    type = DrasmParameterTypes.pt_identifier,
                                    value = line[1].value
                                }
                            ]
                        };
                        if (insideConstructor != null)
                        {
                            insideConstructor.script.script = [.. insideConstructor.script.script, op];
                            insideConstructor.script.labels = [.. insideConstructor.script.labels, line[1].value];
                        }
                        if (insideMethod != null)
                        {
                            insideMethod.script.script = [.. insideMethod.script.script, op];
                            insideMethod.script.labels = [.. insideMethod.script.labels, line[1].value];
                        }
                    }
                    else throw new Exception("Invalid label declaration!");
                }

                break;
        }

        return newAsm;
    }

    private static Type? GetReferencedType(string[] path)
    {
        var completePath = string.Join('.', path);
        return Type.GetType(completePath);
    }
    private static Type? GetReferencedType(string path)
    {
        var completePath = path.Replace(' ', '.');
        return Type.GetType(completePath);
    }

    private struct Token
    {
        public TokenType type;
        public dynamic value;

        public int line;
        public int colStart;
        public int colEnd;

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

        Label,
        Instruction,

        StringValue,
        BooleanValue,

        IntNumberValue,
        FloatNumberValue,

        ArrayLength,
        ArrayList,
        ArrayIndex,

        char_equal,
        char_greaterThan,
        char_colom,
        char_Rbracket, // }
        char_Lbracket, // {
        char_Rbrace,   // ]
        char_Lbrace,   // [

        RegionEnd
    }
}
