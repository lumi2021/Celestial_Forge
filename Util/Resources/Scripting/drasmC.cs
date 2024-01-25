using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameEngine.Util.Interfaces;

namespace GameEngine.Util.Resources;

public class DrasmCompiler : Resource, IScriptCompiler
{
    public void Compile(string src)
    {
        
        string[] lines = Preprocess(src);

        var tokens = Tokenize(lines);
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
            tokens.Add(new());

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
                        Console.WriteLine(ConvertToToken(token));
                        Console.WriteLine(ConvertToToken("" + c));
                        token = "";
                        continue;
                    }
                    if (c == ' ')
                    {
                        if (token != "")
                        Console.WriteLine(ConvertToToken(token));
                        token = "";
                        continue;
                    }
                }

                token += c;
            }
            if (token != "")
            Console.WriteLine(ConvertToToken(token));
            Console.WriteLine();
        }

        Token[][] result = Array.Empty<Token[]>();
        foreach (var i in tokens) result = result.Append(i.ToArray()).ToArray();
        return result;
    }

    private Token ConvertToToken(dynamic value)
    {
        var type = value switch
        {
            "class"     => TokenType.ClassDef,
            "field"     => TokenType.FieldDef,
            "func"      => TokenType.FuncDef,

            "public" or
            "private" or
            "override"  => TokenType.Attribute,

            "end"       => TokenType.RegionEnd,

            "="         => TokenType.char_equal,
            ":"         => TokenType.char_colom,
            ">"         => TokenType.char_greaterThan,
            "("         => TokenType.char_Rbracked,
            ")"         => TokenType.char_Lbracked,        

            _           => TokenType.Identfier,
        };
        if (value.StartsWith("\"") && value.EndsWith("\""))
        {
            value = ((string) value)[1..^1];
            type = TokenType.StringValue;
        }
        if (double.TryParse((string) value, out var numericValue))
        {
            type = TokenType.NumberValue;
            value = numericValue;
        }

        return new Token(type, value);
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
        NumberValue,

        char_equal,
        char_greaterThan,
        char_colom,
        char_Rbracked,
        char_Lbracked,

        RegionEnd
    }
}