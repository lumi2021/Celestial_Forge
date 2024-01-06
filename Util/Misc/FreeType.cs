using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FreeTypeSharp.Native;
using GameEngine.Util.Values;
using StbRectPackSharp;

namespace GameEngine.Text;

public struct Character
{
    public Character() {}

    public byte[] Texture { get; set; } = Array.Empty<byte>();

    public nint Advance { get; set; }

    public int OffsetX { get; set; }
    public int OffsetY { get; set; }

    public uint SizeX { get; set; }
    public uint SizeY { get; set; }

    public Vector2<uint> TexPosition { get; set; } = new();
    public Vector2<uint> TexSize { get; set; } = new();

    public char Char { get; set; }
}

public class FreeType_TtfGlyphLoader
{

    private FT_FaceRec face;
    private IntPtr faceptr;

    private Character baseCharacter;
    private int yoffset;

    public int descender;
    public int fontheight;
    public int lineheight;
    public int ascender;
    
    public uint Size {get; private set;}

    private Dictionary <char, Character> buffer = new Dictionary <char, Character>();

    private int _textureSize = 256;
    private byte[] _bufferTexture = new byte[256*256];
    private Packer _packer = new(256, 256);

    public byte[] AtlasData { get { return _bufferTexture; } }
    public Vector2<int> AtlasSize { get { return new(_textureSize, _textureSize); } }

    public FreeType_TtfGlyphLoader(string font, uint size)
    {
        if (!File.Exists(font)) throw new FileNotFoundException("Failed to load font file:" + font);
        
        Size = size;

        int r1 = (int) FT.FT_Init_FreeType(out IntPtr libptr);
        if (r1!=0) throw new Exception("Failed to load FreeType library.");

        int r2 = (int) FT.FT_New_Face(libptr, font, 0, out faceptr);
        if (r2 != 0) throw new Exception("Failed to create font face.");

        face = Marshal.PtrToStructure<FT_FaceRec>(faceptr);
        FT.FT_Set_Char_Size(faceptr, (int)Size << 6, (int)Size << 6, 96, 96);

        ascender = (int) (face.ascender >> 6);
        descender = (int) (face.descender >> 6);
        fontheight = (int) (((face.height >> 6) - descender + ascender) / 4);
        yoffset = (int) (size - ascender);
        lineheight = fontheight + yoffset - (int)(descender*1.8f);
        baseCharacter = CreateChar('a');

    }

    private unsafe FT_GlyphSlotRec GetCharBitmap(uint c)
    {
        uint index = FT.FT_Get_Char_Index(faceptr, c);

        int r1 = (int) FT.FT_Load_Glyph(faceptr, index, FT.FT_LOAD_TARGET_NORMAL);

        FT_GlyphSlotRec glyph_rec = Marshal.PtrToStructure<FT_GlyphSlotRec>((nint) face.glyph);

        int r2 = (int) FT.FT_Render_Glyph((IntPtr)Unsafe.AsPointer(ref glyph_rec), FT_Render_Mode.FT_RENDER_MODE_NORMAL);

        return glyph_rec;
    }

    public Character CreateChar(char c)
    {
        Character ch = new();

        try
        {
            if (buffer.ContainsKey(c))
                return buffer[c];
            
            if (c == '\r' || c == '\n')
            {
                ch.Char = c;
                buffer.Add(c, ch);
            }

            if (c != ' ' && c != '\t')
            {
                var tt = GetCharBitmap(Convert.ToUInt32(c));
                var charoffsety = ascender - tt.bitmap_top;
                var charoffsetx = tt.bitmap_left;

                byte[] bmp = new byte[tt.bitmap.rows * tt.bitmap.width];
                Marshal.Copy(tt.bitmap.buffer, bmp, 0, bmp.Length);

                ch.Texture = bmp;
                ch.Advance = tt.advance.x / 64;
                ch.OffsetY = charoffsety + yoffset;
                ch.OffsetX = charoffsetx;
                ch.SizeX = tt.bitmap.width;
                ch.SizeY = tt.bitmap.rows;
                ch.TexSize = new(tt.bitmap.width, tt.bitmap.rows);
                ch.TexPosition = AddCharacterToTexture((int) tt.bitmap.width, (int) tt.bitmap.rows, bmp);
                ch.Char    = c;

                buffer.Add(c, ch);
            }
            else
            {
                ch.Advance = c != '\t' ? baseCharacter.Advance : baseCharacter.Advance * 4;
                ch.Char = c;
                buffer.Add(c, ch);
            }
        }
        catch (Exception)
        {
            if (!buffer.ContainsKey(c))
                buffer.Add(c, ch);
        }

        return ch;
    }

    public Character[] CreateStringTexture(string s)
    {
        var temp = new Character[s.Length];

        for (int i = 0; i < s.Length; i++)
            temp[i] = CreateChar(s[i]);
        
        return temp;
    }

    private Vector2<uint> AddCharacterToTexture(int width, int height, byte[] charBitmap)
    {
        bool canContinue;
        uint iteration = 0;
        do
        {
            if (iteration > 0 ) ResizeTexture();

            var res = _packer.PackRect(width, height, null);
            
            if (res != null)
            {
                // Add character to the atlas
                for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)

                    _bufferTexture[((res.Y+y)*_textureSize) + res.X+x] = charBitmap[(y*width)+x];
                

                return new Vector2<uint>((uint) res.X, (uint) res.Y);
            }
            else canContinue = false;
            

            iteration++;            
        } while (!canContinue);

        return new();
    }

    private void ResizeTexture()
    {
        _textureSize *= 2;
        _packer = new Packer(_textureSize, _textureSize);
        _bufferTexture = new byte[_textureSize * _textureSize];

        foreach (var i in buffer)
        {
            var v = i.Value;
            v.TexPosition =
            AddCharacterToTexture((int) v.TexSize.X, (int) v.TexSize.Y, v.Texture); 
            buffer[i.Key] = v;
        }
    }

}
