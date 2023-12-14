using GameEngine.Text;

namespace GameEngine.Util.Resources;

public class Font : Resource
{

    private FreeType_TtfGlyphLoader glyphLoader = new("../../../Assets/Fonts/calibri-regular.ttf", 24);
    private uint _size = 0;
    private string _path = "";
    public uint Size
    {
        get {return _size;}
        set
        {
            _size = value;
            LoadFont(_path, value);
        }
    }

    // font metrics data
    public int descender;
    public int fontheight;
    public int lineheight;
    public int ascender;

    public Font() {}
    public Font(string path)
    {
        LoadFont(path, 24);
    }
    public Font(string path, uint size)
    {
        LoadFont(path, size);
    }

    public void LoadFont(string path, uint size)
    {
        _path = path;
        glyphLoader = new("../../../" + path, size);
        descender = glyphLoader.descender;
        fontheight = glyphLoader.fontheight;
        lineheight = glyphLoader.lineheight;
        ascender = glyphLoader.ascender;
    }

    public Character[] CreateStringTexture(string s)
    {
        return glyphLoader.CreateStringTexture(s);
    }

}