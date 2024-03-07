namespace GameEngine;

public static class StringCustomExtensionMethods
{

    public static string Pascal2Tittle(this string str)
    {
        string result = char.ToUpper(str[0]).ToString();
        for (int i = 1; i < str.Length; i++)
        {
            if (char.IsUpper(str[i]))
                result += " " + str[i];
            
            else result += str[i];
        }

        return result;
    }

    public static string Snake2Tittle(this string str)
    {
        string result = char.ToUpper(str[0]).ToString();
        for (int i = 1; i < str.Length; i++)
        {
            if (str[i] == '_')
                result += " " + char.ToUpper(str[++i]);
            
            else result += str[i];
        }

        return result;
    }

}