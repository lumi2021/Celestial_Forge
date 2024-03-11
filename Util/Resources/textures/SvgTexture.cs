using System.Drawing;
using System.Drawing.Imaging;
using GameEngine.Util.Values;
using System.Runtime.InteropServices;
using Svg;

namespace GameEngine.Util.Resources;

public unsafe class SvgTexture : Texture
{

    public void LoadFromFile(string path, uint sizex, uint sizey)
    { LoadFromFile(path, new(sizex, sizey)); }
    public void LoadFromFile(string path, Vector2<uint> size)
    {
        var fileRef = new FileReference(path);
        var svgDocument = SvgDocument.Open(fileRef.GlobalPath);

        svgDocument.Width = new SvgUnit(SvgUnitType.Pixel, size.X);
        svgDocument.Height = new SvgUnit(SvgUnitType.Pixel, size.Y);
        
        var bitmap = new Bitmap((int) size.X, (int) size.Y);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            //graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            svgDocument.Draw(graphics);
        }

        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
        ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        //copy scan0 to an array
        byte[] imageData = new byte[data.Stride * data.Height];
        Marshal.Copy(data.Scan0, imageData, 0, imageData.Length);

        LoadTextureBytes(imageData, new(size.X, size.Y));

    }

}