using System.Drawing;
using System.Drawing.Imaging;
using GameEngine.Util.Values;
using System.Runtime.InteropServices;
using Svg;

namespace GameEngine.Util.Resources;

public unsafe class SvgTexture : Texture
{

    private FileReference _path = new();
    public FileReference Path
    {
        get => _path;
        set
        {
            _path = value;
            LoadFromFile(_path, _size);
        }
    }

    private Vector2<uint> _size = new();
    public Vector2<uint> SvgSize
    {
        get => _size;
        set
        {
            _size = value;
            LoadFromFile(_path, _size);
        }
    }


    public void LoadFromFile(FileReference path, uint sizex, uint sizey)
        => LoadFromFile(path, new(sizex, sizey));

    public void LoadFromFile(FileReference path, Vector2<uint> size)
    {
        if (size == new Vector2<uint>()) return;

        var svgDocument = SvgDocument.Open(path.GlobalPath);

        svgDocument.Width = new SvgUnit(SvgUnitType.Pixel, size.X-1);
        svgDocument.Height = new SvgUnit(SvgUnitType.Pixel, size.Y-1);
        
        var bitmap = new Bitmap((int) size.X, (int) size.Y);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            svgDocument.Draw(graphics);
        }

        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        //copy scan0 to an array
        byte[] imageData = new byte[data.Stride * data.Height];
        Marshal.Copy(data.Scan0, imageData, 0, imageData.Length);

        LoadTextureBytes(imageData, new(size.X, size.Y));

    }

}