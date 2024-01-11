namespace GameEngine;

public static class ColectionGenericsCustomExtensionMethods
{

    public static T Unqueue<T>(this List<T> list)
    {

        var firstElement = list[0];
        list.RemoveAt(0);
        return firstElement;

    }
    public static T Pop<T>(this List<T> list)
    {

        var lastElement = list[^1];
        list.RemoveAt(list.Count - 1);
        return lastElement;

    }

}