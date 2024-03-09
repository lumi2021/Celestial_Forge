namespace GameEngine.Util.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class IconAttribute(string path) : Attribute
{
    public readonly string path = path;
}