using System.Runtime.CompilerServices;
using GameEngine.Util.Values;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using StbRectPackSharp;
using Color = SixLabors.ImageSharp.Color;
using SlFont = SixLabors.Fonts.Font;
using GameEngine.Core;

namespace GameEngine.Util.Resources;

public struct Character
{
    public Character() {}

    public nint Advance { get; set; }

    public uint SizeX { get; set; }
    public uint SizeY { get; set; }

    public Vector2<uint> TexPosition { get; set; } = new();
    public Vector2<uint> TexSize { get; set; } = new();

    public char Char { get; set; }
}

public class CharacterSet : SharedResource
{

    private FontCollection collection = new();
    private SlFont font = null!;
    private TextOptions options = null!;

    private Character baseCharacter;

    public int fontheight;
    public int lineheight;
    
    public FileReference Path {get; private set;}
    public uint Size {get; private set;}

    private Dictionary <char, Character> buffer = [];

    private Image<A8> _texture = new(64, 64);
    private Packer _packer = new(64, 64);

    public byte[] AtlasData {
        get {
            byte[] bmp = new byte[_texture.Width * _texture.Height * Unsafe.SizeOf<A8>()];
            _texture.CopyPixelDataTo(bmp);
            return bmp;
        }
    }
    public Vector2<int> AtlasSize { get { return new(_texture.Width, _texture.Height); } }

    private CharacterSet(FileReference path, uint size) : base()
    {

        if (!path.Exists) throw new FileNotFoundException("Failed to load font file: " + path);
        Path = path;
        Size = size;

        FontFamily family = collection.Add(path.GlobalPath);
        font = family.CreateFont(size, FontStyle.Regular);
        options = new(font);

        baseCharacter = CreateChar('A');
        var advance = TextMeasurer.MeasureAdvance("A", options);
        var bounds = TextMeasurer.MeasureBounds("A", options);

        fontheight = (int)bounds.Height;
        lineheight = (int)advance.Height;

    }

    private unsafe Vector2<uint> AddCharToAtlas(char c, out Vector2<uint> charSize)
    {
    
        var rect = TextMeasurer.MeasureAdvance(c.ToString(), options);

        charSize = new((uint)MathF.Ceiling(rect.Width), (uint)MathF.Ceiling(rect.Height));

        bool canContinue = true;
        uint iteration = 0;
        do
        {
            if (iteration > 0 ) ResizeTexture();

            var res = _packer.PackRect((int)charSize.X, (int)charSize.Y+2, null);
            
            if (res != null)
            {
                
                _texture.Mutate(x => x.DrawText(
                    c.ToString(),
                    font,
                    Color.White,
                    new PointF(res.X, res.Y + 0.5f))
                );

                return new((uint)res.X, (uint)res.Y);

            }
            else canContinue = false;
            

            iteration++;            
        } while (!canContinue);

        return new();

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

            if (c != '\t')
            {
                var charPos = AddCharToAtlas(c, out Vector2<uint> charSize);
                var rect = TextMeasurer.MeasureAdvance(c.ToString(), options);

                ch.Advance = (int) rect.Width;
                ch.SizeX = charSize.X;
                ch.SizeY = charSize.Y;
                ch.TexSize = charSize;
                ch.TexPosition = charPos;
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
        // check if all characters are inside the buffer
        // if not, iterate to create JUST the characters that are not in the buffer
        var notInBuffer = s.Distinct().Where(e => !buffer.ContainsKey(e)).ToArray();
        foreach (var i in notInBuffer) CreateChar(i);

        var temp = new Character[s.Length];
        for (int i = 0; i < s.Length; i++)
            temp[i] = CreateChar(s[i]);
        
        return temp;
    }

    private void ResizeTexture()
    {
        int newTexSize = _texture.Height * 2;

        _packer.Dispose();
        _packer = new Packer(newTexSize, newTexSize);

        Image<A8> newTex = new Image<A8>(newTexSize, newTexSize);

        var res = _packer.PackRect(_texture.Width, _texture.Height, null);
        newTex.Mutate(e => e.DrawImage(_texture, new Point(res.X, res.Y), 1f));

        _texture.Dispose();
        _texture = newTex;
    }

    public override void Dispose()
    {
        _texture.Dispose();
        _packer.Dispose();

        base.Dispose();
    }

    public static CharacterSet CreateOrGet(string font, uint size = 12)
    {
        return CreateOrGet(new FileReference(font), size);
    }
    public static CharacterSet CreateOrGet(FileReference font, uint size = 12)
    {
        var res = ResourceHeap.TryGetReference<CharacterSet>(font, size);

        if (res != null) return res;
        else return new CharacterSet(font, size);
    }

    public override bool AreEqualsTo(params object?[] args)
    {
        if (args.Length == 2)
            return Path == (FileReference)args[0]! && Size == (uint)args[1]!;
        
        else return false;
    }
}
