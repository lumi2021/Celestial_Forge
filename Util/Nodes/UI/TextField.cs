using System.Numerics;
using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

[Icon("./Assets/icons/Nodes/TextField.svg")]
public class TextField : NodeUI
{
    public enum Aligin {
        Start,
        Center,
        End
    };

    private string _text = "";
    protected string[] _textLines = [""];
    protected Character[][] charsList = [];
    public ColorSpan[] colorsList = [];

    protected Vector2<int> TextSize = new(); 

    [Inspect(InspectAttribute.Usage.multiline_text)]
    public virtual string Text
    {
        get { return _text; }
        set {
            _text = value;
            _textLines = _text.Split('\n');
            TextEdited();
        }
    }

    [Inspect]
    public bool ForceTextSize = false;

    public override Vector2<float> Size
    {
        get
        {
            if (!ForceTextSize)
                return base.Size;
            else
            {
                var bs = base.Size;
                return new(MathF.Max(bs.X, TextSize.X), MathF.Max(bs.Y, TextSize.Y));
            }
        }
    }

    private Color _color =  new(1f, 1f, 1f, 1f);
    [Inspect] public Color Color
    {
        get { return _color; }
        set {
            _color = value;
            material.SetUniform("color", _color);
            ReconfigurateDraw();
        }
    }
    [Inspect] public Aligin horizontalAligin = Aligin.Start;
    [Inspect] public Aligin verticalAligin = Aligin.Start;

    private readonly BitmapTexture tex = new();

    [Inspect] public Material material = new Material2D( Material2D.DrawTypes.Text );

    private Font _font = new("Assets/Fonts/calibri.ttf", 18);
    [Inspect] public Font Font
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

        DrawService.CreateBuffer(NID, "aPosition");
        DrawService.CreateBuffer(NID, "aTextureCoord");

        DrawService.CreateBuffer(NID, "aInstanceWorldMatrix");
        DrawService.CreateBuffer(NID, "aInstanceTexCoordMatrix");
        DrawService.CreateBuffer(NID, "aInstanceColor");
            
        float[] v = new float[] {0f,0f, 1f,0f, 1f,1f, 0f,1f};

        DrawService.SetBufferData(NID, "aPosition", v.ToArray(), 2);
        DrawService.SetBufferData(NID, "aTextureCoord", v.ToArray(), 2);

        DrawService.SetBufferData(NID, "aInstanceWorldMatrix", Matrix4x4.Identity.ToArray(), 16);
        DrawService.SetBufferData(NID, "aInstanceTexCoordMatrix", Matrix4x4.Identity.ToArray(), 16);
        DrawService.SetBufferData(NID, "aInstanceColor", Array.Empty<float>(), 4);

        DrawService.SetBufferAtribDivisor(NID, "aInstanceWorldMatrix", 1);
        DrawService.SetBufferAtribDivisor(NID, "aInstanceTexCoordMatrix", 1);
        DrawService.SetBufferAtribDivisor(NID, "aInstanceColor", 1);

        DrawService.SetElementBufferData(NID, new uint[] {0,1,3, 1,2,3});

        Font.FontUpdated += OnFontUpdate;

        DrawService.EnableAtributes(NID, material);

        tex.Filter = false;

        material.SetUniform("color", _color);

    }

    protected override void Draw(double deltaT)
    {
        tex.Use();
        material.Use();

        #region textPosY switch
        var textPosY = verticalAligin switch
        {
            Aligin.Center =>
            (int)(Size.Y / 2 - TextSize.Y / 2),
            Aligin.End =>
            (int)(Size.Y - TextSize.Y),
            _ => 0
        };
        #endregion
        #region textPosX switch
        var textPosX = horizontalAligin switch
        {
            Aligin.Center =>
            (int)(Size.X / 2 - TextSize.X / 2),
            Aligin.End =>
            (int)(Size.X - TextSize.X),
            _ => 0
        };
        #endregion

        var world = Matrix4x4.CreateTranslation(new Vector3(textPosX + Position.X, textPosY + Position.Y, 0))
        * Viewport!.Camera2D.GetViewOffset();
        var proj = Viewport!.Camera2D.GetProjection();

        material.SetTranslation(world);
        material.SetProjection(proj);

        DrawService.Draw(NID);
    }

    protected virtual void TextEdited()
    {
        ReconfigurateDraw();
    }

    protected virtual void OnFontUpdate()
    {
        TextEdited();
    }

    private void ReconfigurateDraw()
    {
        charsList = [];

        // Load character information
        for (int i = 0; i < _textLines.Length; i++)
        {
            string ln = _textLines[i];
            charsList = charsList.Append(Font.CreateStringTexture(ln)).ToArray();
        }

        // Load characters and text sizes
        TextSize = new();
        for (int i = 0; i < charsList.Length; i++)
        {
            int lineSize = 0;
            foreach (var j in charsList[i])
            {
                lineSize += (int) j.Advance;
                if (lineSize > TextSize.X) TextSize.X = lineSize;
            }
        }
        TextSize.Y = _font.lineheight * charsList.Length;

        // Load characters matrices
        uint charCount = 0;

        List<float> world = [];
        List<float> uv = [];
        List<float> color = [];
        int charPosX = 0;

        float textureSize = Font.AtlasSize.X;
        int charGlobalIndex = 0;

        for (int i = 0; i < charsList.Length; i++)
        {
            int carPosY = _font.lineheight * i;
            int lineSize = 0;
            //get line size
            foreach (var j in charsList[i]) lineSize += (int) j.Advance;

            int lineOffset = horizontalAligin switch
            {
                Aligin.Center   =>  (TextSize.X - lineSize) / 2,
                Aligin.End      =>  TextSize.X - lineSize,
                _               =>  0
            };

            foreach (var j in charsList[i])
            {
                var m = Matrix4x4.CreateScale(j.SizeX, j.SizeY, 1)
                * Matrix4x4.CreateTranslation(lineOffset + charPosX, carPosY, 0);
                world.AddRange(Matrix4x4.Transpose(m).ToArray());

                var u = MathHelper.Matrix4x4CreateRect(j.TexPosition, j.TexSize)
                * Matrix4x4.CreateOrthographic(textureSize*2,textureSize*2, -1f, 1f);
                uv.AddRange(Matrix4x4.Transpose(u).ToArray());

                ColorSpan col = colorsList.FirstOrDefault(e =>
                charGlobalIndex >= e.start && charGlobalIndex < e.end,
                new (0, 0, _color));

                color.Add(col.color);

                charPosX += (int) j.Advance;
                charCount++;

                charGlobalIndex++;
            }
        
            charGlobalIndex++;

            charPosX = 0;
        }

        DrawService.SetBufferData(NID, "aInstanceWorldMatrix", world.ToArray(), 16);
        DrawService.SetBufferData(NID, "aInstanceTexCoordMatrix", uv.ToArray(), 16);
        DrawService.SetBufferData(NID, "aInstanceColor", color.ToArray(), 4);

        DrawService.EnableInstancing(NID, charCount);
    
        // Update texture
        var size = Font.AtlasSize;
        tex.Load(Font.AtlasData, (uint) size.X, (uint) size.Y);
        tex.Filter = false;
    }


    #region inner types

    public readonly struct ColorSpan (int start, int end, Color color)
    {
        public readonly int start = start;
        public readonly int end = end;
        public readonly Color color = color;

        public override string ToString()
        {
            return $"ColorSpan({start} - {end})";
        }
    }

    #endregion

}