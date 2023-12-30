using System.Numerics;
using GameEngine.Sys;
using GameEngine.Text;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using Silk.NET.OpenGL;

namespace GameEngine.Util.Nodes;

public class Label : NodeUI, ICanvasItem
{

    public bool Visible { get; set; } = true;

    public enum Aligin {
        Start,
        Center,
        End
    };

    private string _text = "";
    protected string[] _textLines = new string[] {""};
    public string Text
    {
        get { return _text; }
        set {
            _text = value;
            _textLines = _text.Split('\n');
            TextEdited();
        }
    }
    protected Character[][] charsList = Array.Empty<Character[]>();

    public Color color = new(0f, 0f, 0, 1f);
    public Aligin horisontalAligin = Aligin.Start;
    public Aligin verticalAligin = Aligin.Start;

    private Material mat = new();
    private BitmapTexture tex = new();

    private Font _font = new("Assets/Fonts/calibri-regular.ttf", 24);
    public Font Font
    {
        get { return _font; }
        set
        {
            _font = value;
            _font.FontUpdated += OnFontUpdate;
            OnFontUpdate();
        }
    }
    
    private readonly Dictionary<char, BitmapTexture> textures = new();

    
    protected override void Init_()
    {

        var gl = Engine.gl;

        const string vertexCode = @"

        #version 330 core

        in vec2 aPosition;
        in vec2 aTextureCoord;

        in mat4 aWorldMatrix;
        in mat4 aUvMatrix;

        uniform mat4 world;
        uniform mat4 proj;

        out vec2 UV;

        void main()
        {
            gl_Position = vec4(aPosition, 0, 1.0) * aWorldMatrix * world * proj;
            UV = (vec4(aTextureCoord, 0, 1.0) * aUvMatrix).xy;
        }";
        const string fragmentCode = @"
        #version 330 core

        in vec2 UV;

        out vec4 out_color;

        uniform vec4 fontColor;
        uniform sampler2D tex0;

        void main()
        {
            out_color.rgb = fontColor.rgb;
            out_color.a = texture(tex0, UV).r;
        }";

        mat.LoadShaders(vertexCode, fragmentCode);

        DrawService.CreateBuffer(RID, "aPosition");
        DrawService.CreateBuffer(RID, "aTextureCoord");

        DrawService.CreateBuffer(RID, "aWorldMatrix");
        DrawService.CreateBuffer(RID, "aUvMatrix");
            
        float[] v = new float[] {0f,0f, 1f,0f, 1f,1f, 0f,1f};

        DrawService.SetBufferData(RID, "aPosition", v.ToArray(), 2);
        DrawService.SetBufferData(RID, "aTextureCoord", v.ToArray(), 2);

        DrawService.SetBufferData(RID, "aWorldMatrix", Matrix4x4.Identity.ToArray(), 16);
        DrawService.SetBufferData(RID, "aUvMatrix", Matrix4x4.Identity.ToArray(), 16);
        DrawService.SetBufferAtribDivisor(RID, "aWorldMatrix", 1);
        DrawService.SetBufferAtribDivisor(RID, "aUvMatrix", 1);

        DrawService.SetElementBufferData(RID, new uint[] {0,1,3, 1,2,3});

        DrawService.EnableAtributes(RID, mat);

        Font.FontUpdated += OnFontUpdate;
    }

    protected override unsafe void Draw(double deltaT)
    {
        var gl = Engine.gl;

        mat.Use();
        tex.Use();

        var world = Matrix4x4.CreateTranslation(new Vector3(-Engine.window.Size.X/2, -Engine.window.Size.Y/2, 0));
        world *= Matrix4x4.CreateTranslation(new Vector3(Position.X, Position.Y, 0));
        world *= Matrix4x4.CreateScale(1, -1, 1);

        var proj = Matrix4x4.CreateOrthographic(Engine.window.Size.X,Engine.window.Size.Y,-.1f,.1f);

        gl.UniformMatrix4(0, 1, true, (float*) &world);
        gl.UniformMatrix4(1, 1, true, (float*) &proj);

        mat.SetShaderParameter("fontColor", color);

        gl.PixelStore(GLEnum.UnpackAlignment, 1);

        DrawService.Draw(RID);
    }

    protected virtual void TextEdited()
    {
        ReconfigurateDraw();
    }

    protected virtual void OnFontUpdate()
    {
        TextEdited();
    }

    public void Show() { Visible = true; }
    public void Hide() { Visible = false; }


    private void ReconfigurateDraw()
    {
        charsList = Array.Empty<Character[]>();

        // Load character information
        for (int i = 0; i < _textLines.Length; i++)
        {
            string ln = _textLines[i];
            charsList = charsList.Append(Font.CreateStringTexture(ln)).ToArray();
        }

        // Load characters matrices
        uint charCount = 0;

        List<float> world = new();
        List<float> uv = new();
        int charPosX = 0;

        float textureSize = Font.AtlasSize.X;
        for (int i = 0; i < charsList.Length; i++)
        {
            int charPosY = _font.lineheight * i;

            foreach (var j in charsList[i])
            {
                var m = Matrix4x4.CreateScale(j.SizeX, j.SizeY, 1)
                * Matrix4x4.CreateTranslation(charPosX + j.OffsetX, charPosY + j.OffsetY, 0);
                world.AddRange(Matrix4x4.Transpose(m).ToArray());

                var u = Matrix4x4.CreateScale(j.TexSize.X, j.TexSize.Y, 1)
                * Matrix4x4.CreateTranslation(j.TexPosition.X, j.TexPosition.Y, 0)
                * Matrix4x4.CreateOrthographic(textureSize*2,textureSize*2, -1f, 1f);
                uv.AddRange(Matrix4x4.Transpose(u).ToArray());

                charPosX += (int) j.Advance;
                charCount++;
            }

            charPosX = 0;
        }

        DrawService.SetBufferData(RID, "aWorldMatrix", world.ToArray(), 16);
        DrawService.SetBufferData(RID, "aUvMatrix", uv.ToArray(), 16);

        DrawService.EnableInstancing(RID, charCount);
    
        // Update texture
        var size = Font.AtlasSize;
        tex.Load(Font.AtlasData, (uint) size.X, (uint) size.Y);
    }

}
