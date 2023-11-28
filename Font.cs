using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FreeTypeSharp.Native;
using Silk.NET.SDL;

namespace GameEngine;

public class Character
    {
        public byte[] Texture { get; set; } = Array.Empty<byte>();

        public nint Advance { get; set; }

        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public uint TexSizeX { get; set; }
        public uint TexSizeY { get; set; }

        public uint SizeX { get; set; }
        public uint SizeY { get; set; }

        public char Char { get; set; }
    }

public class Font
{

    private FT_FaceRec face;
    private IntPtr faceptr;

    private Character baseCharacter;
    private int yoffset;

    public int descender;
    public int fontheight;
    public int lineheight;
    public int ascender;

    private uint rSize;
    
    public uint Size {get; private set;}
    public float ResizeScale {get; private set;} = 1f;

    private Dictionary <char, Character> buffer = new Dictionary <char, Character>();

    public Font(string font, uint size)
    {
        if (!File.Exists(font)) throw new FileNotFoundException("Failed to load font file:" + font);
        
        if (size < 24)
        {
            Size = size;
            rSize = 48;
            ResizeScale = size/24f;
        }
        else
        {
            Size = size;
            rSize = size;
        }

        int r1 = (int) FT.FT_Init_FreeType(out IntPtr libptr);
        if (r1!=0) throw new Exception("Failed to load FreeType library.");

        int r2 = (int) FT.FT_New_Face(libptr, font, 0, out faceptr);
        if (r2 != 0) throw new Exception("Failed to create font face.");

        face = Marshal.PtrToStructure<FT_FaceRec>(faceptr);
        FT.FT_Set_Char_Size(faceptr, (int)rSize << 6, (int)rSize << 6, 96, 96);
        FT.FT_Set_Pixel_Sizes(faceptr, rSize, rSize);

        ascender = (int) ((face.ascender >> 6) * ResizeScale);
        descender = (int) ((face.descender >> 6) * ResizeScale);
        fontheight = (int) (((face.height >> 6)*ResizeScale + descender + ascender) / 4);
        yoffset = (int)((rSize - ascender) * ResizeScale);
        lineheight = fontheight - descender;
        baseCharacter = CreateChar('i');

    }

    private unsafe FT_GlyphSlotRec GetCharBitmap(uint c)
    {
        uint index = FT.FT_Get_Char_Index(faceptr, c);

        int r1 = (int) FT.FT_Load_Glyph(faceptr, index, FT.FT_LOAD_TARGET_NORMAL);

        FT_GlyphSlotRec glyph_rec = Marshal.PtrToStructure<FT_GlyphSlotRec>((nint) face.glyph);

        int r2 = (int) FT.FT_Render_Glyph((IntPtr)Unsafe.AsPointer(ref glyph_rec), FT_Render_Mode.FT_RENDER_MODE_NORMAL);

        return glyph_rec;
    }

    private Character CreateChar(char c)
    {
        Character ch = new Character();

        try
        {
            if (buffer.ContainsKey(c))
                return buffer[c];
            
            if (c == '\r' || c == '\n')
            {
                ch.Char = c;
                buffer.Add(c, ch);
                return(ch);
            }

            if (c != ' ' && c != '\t')
            {
                var tt = GetCharBitmap(Convert.ToUInt32(c));
                var charoffsety = ascender - tt.bitmap_top;
                var charoffsetx = tt.bitmap_left;

                byte[] bmp = new byte[tt.bitmap.rows * tt.bitmap.width];
                Marshal.Copy(tt.bitmap.buffer, bmp, 0, bmp.Length);

                ch.Texture = bmp;
                ch.Advance = (nint)(tt.advance.x/64 * ResizeScale);
                ch.OffsetY = (int)(charoffsety * ResizeScale) + yoffset;
                ch.OffsetX = (int)(charoffsetx * ResizeScale);
                ch.SizeX = (uint)(tt.bitmap.width * ResizeScale);
                ch.SizeY = (uint)(tt.bitmap.rows * ResizeScale);
                ch.TexSizeX = tt.bitmap.width;
                ch.TexSizeY = tt.bitmap.rows;
                ch.Char    = c;
                buffer.Add(c, ch);
            }
            else
            {
                ch.Char = c;
                buffer.Add(c, ch);
            }
        }
        catch (Exception)
        {
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
}
