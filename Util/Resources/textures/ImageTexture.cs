using GameEngine.Core;
using StbImageSharp;

namespace GameEngine.Util.Resources;

public unsafe class ImageTexture : Texture
{

    private string _path = "";
    public string Path
    {
        get { return _path; }
        set { LoadFromFile(value); }
    }

    public void LoadFromFile(string path)
    {
        _path = FileService.GetGlobalPath(path);

        using FileStream stream = File.OpenRead(_path);

        ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        LoadTextureBytes(image.Data, new((uint)image.Width, (uint)image.Height));
    }

}