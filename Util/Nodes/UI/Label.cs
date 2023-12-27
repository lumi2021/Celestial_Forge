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
    private readonly List<CharDetails> charsToDrawPool = new();

    public Color color = new(0f, 0f, 0, 1f);
    public Aligin horisontalAligin = Aligin.Start;
    public Aligin verticalAligin = Aligin.Start;

    private Material mat = new();
    private BitmapTexture texture = new();

    private uint vPos = 0;
    private uint vUv = 0;

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
    protected override void Init_()
    {

        var gl = Engine.gl;

        const string vertexCode = @"

        #version 330 core

        in vec2 aPosition;
        in vec2 aTextureCoord;

        in mat4 aWorldMatrix;

        uniform mat4 world;
        uniform mat4 proj;

        out vec2 UV;

        void main()
        {
            gl_Position = vec4(aPosition, 0, 1.0) * aWorldMatrix * world * proj;
            UV = aTextureCoord;
        }";
        const string fragmentCode = @"
        #version 330 core

        in vec2 UV;

        out vec4 out_color;

        uniform vec4 fontColor;
        uniform sampler2D tex0;

        void main()
        {
            out_color = vec4(UV.x, UV.y, 0, 1);
        }";

        mat.LoadShaders(vertexCode, fragmentCode);

        vPos = DrawService.CreateBuffer(RID, "aPosition");
        vUv =  DrawService.CreateBuffer(RID, "aTextureCoord");
        vUv =  DrawService.CreateBuffer(RID, "aWorldMatrix");
            
        float[] v = new float[] {0f,0f, 1f,0f, 1f,1f, 0f,1f};
        float[] tc = new float[] {0f,0f, 1f,0f, 1f,1f, 0f,1f};

        DrawService.SetBufferData(RID, "aPosition", v.ToArray(), 2);
        DrawService.SetBufferData(RID, "aTextureCoord", tc.ToArray(), 2);

        DrawService.SetBufferData(RID, "aWorldMatrix", ToArray(Matrix4x4.Identity), 16);
        DrawService.SetBufferAtribDivisor(RID, "aWorldMatrix", 1);

        DrawService.SetElementBufferData(RID, new uint[] {0,1,3, 1,2,3});

        DrawService.EnableAtributes(RID, mat);

        Font.FontUpdated += OnFontUpdate;
    }

    protected override unsafe void Draw(double deltaT)
    {
        var gl = Engine.gl;

        mat.Use();

        var world = Matrix4x4.CreateTranslation(new Vector3(-Engine.window.Size.X/2, -Engine.window.Size.Y/2, 0));
        world *= Matrix4x4.CreateTranslation(new Vector3(Position.X, Position.Y, 0));
        world *= Matrix4x4.CreateScale(1, -1, 1);

        var proj = Matrix4x4.CreateOrthographic(Engine.window.Size.X,Engine.window.Size.Y,-.1f,.1f);
        
        gl.UniformMatrix4(0, 1, true, (float*) &world);
        gl.UniformMatrix4(1, 1, true, (float*) &proj);
        gl.Uniform4(3, color.GetAsNumerics());

        gl.PixelStore(GLEnum.UnpackAlignment, 1);

        DrawService.Draw(RID);
    }

    protected virtual void TextEdited()
    {
        charsList = Array.Empty<Character[]>();

        for (int i = 0; i < _textLines.Length; i++)
        {
            string ln = _textLines[i];
            charsList = charsList.Append(Font.CreateStringTexture(ln)).ToArray();
        }

        charsToDrawPool.Clear();
        int charPosX = 0;

        for (int i = 0; i < charsList.Length; i++) {
            int charPosY = _font.lineheight * i;
            foreach (var j in charsList[i])
            {
                var c = new CharDetails();

                c.position.X = charPosX + j.OffsetX;
                c.position.Y = charPosY + j.OffsetY;
                c.size.X = (int) j.SizeX;
                c.size.Y = (int) j.SizeY;

                charsToDrawPool.Add(c);

                charPosX += (int) j.Advance;
            }
            charPosX = 0;
        }

        List<float> matrices = new();

        foreach (var i in charsToDrawPool)
        {
            var m = Matrix4x4.CreateScale(i.size.X, i.size.Y, 1)
            * Matrix4x4.CreateTranslation(i.position.X, i.position.Y, 0);
            matrices.AddRange(ToArray(Matrix4x4.Transpose(m)));
        }

        DrawService.SetBufferData(RID, "aWorldMatrix", matrices.ToArray(), 16);
        DrawService.EnableInstancing(RID, (uint) charsToDrawPool.Count);
    }

    protected virtual void OnFontUpdate()
    {
        TextEdited();
    }

    public void Show() { Visible = true; }
    public void Hide() { Visible = false; }

    static float[] ToArray(Matrix4x4 matrix)
    {
        float[] array = new float[16];

        // Preenchendo a array com os elementos da matriz
        array[0] = matrix.M11;
        array[1] = matrix.M12;
        array[2] = matrix.M13;
        array[3] = matrix.M14;
        array[4] = matrix.M21;
        array[5] = matrix.M22;
        array[6] = matrix.M23;
        array[7] = matrix.M24;
        array[8] = matrix.M31;
        array[9] = matrix.M32;
        array[10] = matrix.M33;
        array[11] = matrix.M34;
        array[12] = matrix.M41;
        array[13] = matrix.M42;
        array[14] = matrix.M43;
        array[15] = matrix.M44;

        return array;
    }


    #region inner classes/structs
    private struct CharDetails
    {
        public Vector2<int> position;
        public Vector2<int> size;
    }
    #endregion
}