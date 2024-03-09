namespace GameEngine.Util.Values;

public static class ExtensionMethods
{
    
    public static void Add(this List<float> to, Color col)
    {
        to.Add(col.NormalR);
        to.Add(col.NormalG);
        to.Add(col.NormalB);
        to.Add(col.A);
    }

}