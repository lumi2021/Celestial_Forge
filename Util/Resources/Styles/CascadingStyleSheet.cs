using GameEngine.Core;

namespace GameEngine.Util.Resources;

public partial class CascadingStyleSheet : Resource
{
    private readonly List<Style> styles = new();

    public struct Style
    {
        public string[] ids;
        public string[] tags;
        public string[] classes;
        public string[] modifiers;

        public StyleItem[] items;
    }
    public struct StyleItem
    {
        public string target;
        public dynamic value;
    }

}

