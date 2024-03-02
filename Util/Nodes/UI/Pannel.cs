using System.Numerics;
using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

[Icon("./Assets/icons/Nodes/Panel.svg")]
public class Pannel : NodeUI, ICanvasItem
{

    [Inspect]
    public bool Visible { get; set; } = true;

    private Color _bgColor = new(100, 100, 100, 0.9f);
    [Inspect]
    public Color BackgroundColor
    {
        get { return _bgColor; }
        set {
            _bgColor = value;
            material.SetUniform("color", _bgColor);
        }
    }

    [Inspect]
    public Material material = new Material2D( Material2D.DrawTypes.SolidColor );

    protected override void Init_()
    {
        float[] v = new float[] { 0.0f,0.0f, 1.0f,0.0f, 1.0f,1.0f, 0.0f,1.0f };
        float[] uv = new float[] { 0f,0f, 1f,0f, 1f,1f, 0f,1f };
        uint[] i = new uint[] {0,1,3, 1,2,3};

        DrawService.CreateBuffer(NID, "aPosition");
        DrawService.SetBufferData(NID, "aPosition", v, 2);

        DrawService.CreateBuffer(NID, "aTextureCoord");
        DrawService.SetBufferData(NID, "aTextureCoord", uv, 2);
            
        DrawService.SetElementBufferData(NID, i);

        DrawService.EnableAtributes(NID, material);

        material.SetUniform("color", _bgColor);
    }

    protected override unsafe void Draw(double deltaT)
    {
        var gl = Engine.gl;

        material.Use();

        var world = MathHelper.Matrix4x4CreateRect(Position, Size) * Viewport!.Camera2D.GetViewOffset();
        var proj = Viewport!.Camera2D.GetProjection();

        material.SetTranslation(world);
        material.SetProjection(proj);

        DrawService.Draw(NID);
    }

    public void Show() { Visible = true; }
    public void Hide() { Visible = false; }

}
