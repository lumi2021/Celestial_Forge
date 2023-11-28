using System.Numerics;
using GameEngine.Sys;
using GameEngine.Util.Resources;
using Silk.NET.OpenGL;

namespace GameEngine.Util.Nodes;

public class Label : NodeUI
{

    public string text = "";

    private static uint _vao;
    private static uint _vbo;
    private static uint _ebo;

    private static Material mat = new();

    private static uint _texture;

    private static Font font = new Font("../../../Assets/Fonts/calibri-regular.ttf", 24);
    protected override void Init_()
    {
        var gl = Engine.gl;

        _vao = gl.GenVertexArray();
        _vbo = gl.GenBuffer();
        _ebo = gl.GenBuffer();

        const string vertexCode = @"
        #version 330 core

        layout (location = 0) in vec2 aPosition;
        layout (location = 1) in vec2 aTextureCoord;

        uniform mat4 world;
        uniform mat4 proj;

        out vec2 UV;

        void main()
        {
            gl_Position = vec4(aPosition, 0, 1.0) * world * proj;
            UV = aTextureCoord;
        }";
        const string fragmentCode = @"
        #version 330 core

        in vec2 UV;

        out vec4 out_color;

        uniform sampler2D tex0;

        void main()
        {
            vec4 fontColor = vec4(0,0,0,1);

            vec4 color = texture(tex0, UV);
            if (color.r < 0.5) discard;
            out_color = fontColor;
        }";

        mat.LoadShaders(vertexCode, fragmentCode);

        gl.BindVertexArray(0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        
        _texture = gl.GenTexture();

        gl.BindTexture(GLEnum.Texture2D, _texture);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        gl.BindTexture(GLEnum.Texture2D, 0);
    }

    protected override unsafe void Draw(double deltaT)
    {
        var gl = Engine.gl;

        gl.BindVertexArray(_vao);
        mat.Use();
        gl.BindTexture(GLEnum.Texture2D, _texture);

        Character[] chars = font.CreateStringTexture(text);

        int posX = 0;
        int posY = 0;

        foreach (var j in chars)
        {
            if (j.Char == ' ')
            {
                posX += 10;
                continue;
            }
            if (j.Char == '\n')
            {
                posX = 0;
                posY += font.lineheight;
                continue;
            }

            List<float> v = new();
            uint[] i = new uint[] {0,1,3, 1,2,3};

            v.Add(0f);v.Add(0f); v.Add(0f);v.Add(0f);
            v.Add(1f);v.Add(0f); v.Add(1f);v.Add(0f);
            v.Add(1f);v.Add(1f); v.Add(1f);v.Add(1f);
            v.Add(0f);v.Add(1f); v.Add(0f);v.Add(1f);

            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (float* buf = v.ToArray())
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (v.Count * sizeof(float)), buf, BufferUsageARB.StreamDraw);
            
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            fixed (uint* buf = i)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (i.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*) 0);

            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*) (2*sizeof(float)));

            gl.PixelStore(GLEnum.UnpackAlignment, 1);
            fixed (byte* buf = j.Texture)
            gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba, j.TexSizeX, j.TexSizeY, 0, GLEnum.Red, GLEnum.UnsignedByte, buf);
            gl.GenerateMipmap(TextureTarget.Texture2D);

            var world = Matrix4x4.CreateScale(j.SizeX, j.SizeY, 1);
            world *= Matrix4x4.CreateTranslation(new Vector3(-Engine.window.Size.X/2, -Engine.window.Size.Y/2, 0));
            world *= Matrix4x4.CreateTranslation(new Vector3(posX+j.OffsetX, posY+j.OffsetY, 0));
            world *= Matrix4x4.CreateScale(1, -1, 1);
            var proj = Matrix4x4.CreateOrthographic(Engine.window.Size.X,Engine.window.Size.Y,-.1f,.1f);

            gl.UniformMatrix4(0, 1, true, (float*) &world);
            gl.UniformMatrix4(1, 1, true, (float*) &proj);

            gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*) 0);

            posX += (int) j.Advance;
        }
    }

}