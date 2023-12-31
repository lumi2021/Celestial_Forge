using System.Numerics;
using GameEngine.Sys;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

public class Pannel : NodeUI, ICanvasItem
{

    public bool Visible { get; set; } = true;

    private Color _bgColor = new(100, 100, 100, 0.9f);
    public Color BackgroundColor
    {
        get { return _bgColor; }
        set {
            _bgColor = value;
            mat.SetShaderParameter(RID, "backgroundColor", _bgColor);
        }
    }

    private Material mat = DrawService.Standard2DMaterial;
    private BitmapTexture tex = new();

    protected override void Init_()
    {
        float[] v = new float[] { 0.0f,0.0f, 1.0f,0.0f, 1.0f,1.0f, 0.0f,1.0f };
        float[] uv = new float[] { 0f,0f, 1f,0f, 1f,1f, 0f,1f };
        uint[] i = new uint[] {0,1,3, 1,2,3};

        DrawService.CreateBuffer(RID, "aPosition");
        DrawService.SetBufferData(RID, "aPosition", v, 2);

        DrawService.CreateBuffer(RID, "aTextureCoord");
        DrawService.SetBufferData(RID, "aTextureCoord", uv, 2);

        DrawService.EnableAtributes(RID, mat);
            
        DrawService.SetElementBufferData(RID, i);

        mat.SetShaderParameter(RID, "backgroundColor", _bgColor);
    }

    protected override unsafe void Draw(double deltaT)
    {
        var gl = Engine.gl;

        mat.Use( RID );
        tex.Use();

        var world = Matrix4x4.CreateScale(Size.X, Size.Y, 1);
        world *= Matrix4x4.CreateTranslation(new Vector3(-Engine.window.Size.X/2, -Engine.window.Size.Y/2, 0));
        world *= Matrix4x4.CreateTranslation(new Vector3(Position.X, Position.Y, 0));
        var proj = Matrix4x4.CreateOrthographic(Engine.window.Size.X,Engine.window.Size.Y,-.1f,.1f);

        mat.SetShaderWorldMatrix(world);
        mat.SetShaderProjectionMatrix(proj);

        DrawService.Draw(RID);
    }

    public void Show() { Visible = true; }
    public void Hide() { Visible = false; }

}