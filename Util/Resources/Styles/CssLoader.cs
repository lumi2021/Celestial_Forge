using GameEngine.Core;
using GameEngine.Util.Values;

namespace GameEngine.Util.Resources;

public partial class CascadingStyleSheet : Resource
{

    public static void Load(string path)
    {

        var fileContent = FileService.GetFile(path);
        var formatedFile = fileContent
            .Replace("\n", "").Replace("\r", "")
            .Replace("    ", "").Replace("\t", "")
            .Replace(", ", ",").Replace(": ", ":")
            .Trim();

        Lexer(formatedFile);

    }

    #region interpreter

    private static void Lexer(string source)
    {
        List<CssToken> tokens = new();

        string token = "";
        bool insideScope = false;
        int parentesesLvl = 0;
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];

            if (!insideScope)
            {
                if (c == '{')
                {
                    insideScope = true;
                    tokens.Add(new(TokenType.char_key));
                    token = "";
                    continue;
                }
                if (c == ' ')
                {
                    if (token.StartsWith("#"))
                        tokens.Add(new(TokenType.id_ref, token[1..]));
                    else
                        tokens.Add(new(TokenType.tag_ref, token));

                    token = "";
                    continue;
                }
            }
            else
            {
                if (c == ':')
                {
                    tokens.Add(new(TokenType.style_target, token));
                    token = "";
                    continue;
                }
                else if (c == ';')
                {
                    tokens.Add(new(TokenType.style_value, token));
                    token = "";
                    continue;
                }
                else if (c == '}')
                {
                    insideScope = false;
                    tokens.Add(new(TokenType.char_key));
                    token = "";
                    continue;
                }
                else if (c == ',' && parentesesLvl>0)
                {
                    token += ' ';
                    continue;
                }
                else if (c == '(')
                    parentesesLvl++;
                else if (c == ')')
                    parentesesLvl--;

                //else if (c == ' ') continue;
            }

            token += c;
        }

        // get just style value tokens
        var idxs = from i in Enumerable.Range(0, tokens.Count) where tokens[i].token == TokenType.style_value select i;

        // Unpack the correct values
        foreach (var i in idxs)
        {
            var tkn = tokens[i];

            var strVals = (object[]) (tkn.value as String)!.Split(new char[] {','});
            object[] finalValues = new object[strVals.Length];
            
            for (int j = 0; j < strVals.Length; j++)
            {
                var objString = (string)strVals[j]!;
                // parse the value to a object
                if (objString.StartsWith("rgba("))
                {
                    var v = objString[5..^1].Split(' ');
                    finalValues[j] = (object) new Color(int.Parse(v[0]), int.Parse(v[1]), int.Parse(v[2]), float.Parse(v[3]));
                }
                else if (objString.StartsWith("rgb("))
                {
                    var v = objString[4..^1].Split(' ');
                    finalValues[j] = (object) new Color(int.Parse(v[0]), int.Parse(v[1]), int.Parse(v[2]));
                }

            }

            tkn.value = finalValues;
            tokens[i] = tkn;
        }

        foreach (var i in tokens)
            Console.WriteLine(i);
    }


    private enum TokenType
    {
        class_ref,
        id_ref,
        tag_ref,
        style_target,
        style_value,
        char_key,
        char_semicolon
    }
    private struct CssToken
    {
        public TokenType token;
        public object? value;

        public CssToken(TokenType tkn, object? val = null)
        {
            token = tkn;
            value = val;
        }
        public override readonly string ToString() => string.Format("{0}: \t{1};", token, value ?? "<n>");
    }
    #endregion

}

