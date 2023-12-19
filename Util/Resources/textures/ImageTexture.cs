using StbImageSharp;

namespace GameEngine.Util.Resources;

public unsafe class ImageTexture : Texture
{

    public void LoadFromFile(string path)
    {
        using FileStream stream = File.OpenRead(path);

        ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        LoadTextureBytes(image.Data, new((uint)image.Width, (uint)image.Height));
    }

}