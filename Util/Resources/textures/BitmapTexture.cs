using GameEngine.Core;
using GameEngine.Util.Values;
using Silk.NET.OpenGL;

namespace GameEngine.Util.Resources;

public unsafe class BitmapTexture : Texture
{

    public void Load(byte[] bitmap, Vector2<uint> size) {
        LoadTextureBytes(bitmap, size);
    }
    public void Load(byte[] bitmap, uint sizeX, uint sizeY) {
        LoadTextureBytes(bitmap, new(sizeX, sizeY));
    }

    protected override void LoadTextureBytes(byte[] data, Vector2<uint> size)
    {
        var gl = Engine.gl;

        gl.BindTexture(GLEnum.Texture2D, _textureId);
    
        fixed (byte* buf = data)
        gl.TexImage2D(
            GLEnum.Texture2D, 0, InternalFormat.Rgba,
            size.X, size.Y, 0,
            GLEnum.Red, GLEnum.UnsignedByte, buf
        );
        
        gl.GenerateMipmap(TextureTarget.Texture2D);

        _data = data;
        Size = size;
    }

}