using GameEngine.Sys;
using GameEngine.Util.Values;
using Silk.NET.OpenGL;

namespace GameEngine.Util.Resources;

public abstract class Texture : Resource
{

    protected uint _textureId = 0;

    protected byte[] _data = Array.Empty<byte>();
    public byte[] Data { get{ return _data; } }

    public Vector2<uint> Size { get; protected set; }


    public Texture()
    {
        _textureId = Engine.gl.GenTexture();
    }

    protected virtual void LoadTextureBytes(byte[] data, Vector2<uint> size, bool updateParams=true)
    {
        var gl = Engine.gl;

        gl.BindTexture(GLEnum.Texture2D, _textureId);

        if (updateParams) UpdateParameters();
    
        gl.TexImage2D<byte>(
            GLEnum.Texture2D, 0, InternalFormat.Rgba,
            size.X, size.Y, 0,
            PixelFormat.Rgba, GLEnum.UnsignedByte, data
        );
        
        //gl.GenerateMipmap(TextureTarget.Texture2D);

        _data = data;
        Size = size;
    }

    protected void UpdateParameters()
    {
        var gl = Engine.gl;
        gl.BindTexture(GLEnum.Texture2D, _textureId);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    public void Use() { Engine.gl.BindTexture(GLEnum.Texture2D, _textureId); }

    #pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public override void Dispose()
    {
        Engine.gl.DeleteTexture(_textureId);
        base.Dispose();
    }

}