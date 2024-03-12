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

    private Color _bgColor = new(100, 100, 100, 0.9f);
    private Color _strokeColor = new(0, 0, 0, 0.9f);
    private uint _strokeSize = 0;

    private Vector4<uint> _cornerRadius = new(0,0,0,0);

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
    public Color StrokeColor
    {
        get { return _strokeColor; }
        set {
            _strokeColor = value;
            material.SetUniform("strokeColor", _strokeColor);
        }
    }
    [Inspect]
    public uint StrokeSize
    {
        get { return _strokeSize; }
        set {
            _strokeSize = value;
            material.SetUniform("strokeSize", _strokeSize);
        }
    }

    [Inspect]
    public Vector4<uint> CornerRadius
    {
        get { return _cornerRadius; }
        set {
            _cornerRadius = value;
            material.SetUniform("cornerRadius", _cornerRadius);
        }
    }

    [Inspect]
    public Material material = new Material2D( Material2D.DrawTypes.SolidColor );

    protected override void Init_()
    {
        float[] v = [ 0.0f,0.0f, 1.0f,0.0f, 1.0f,1.0f, 0.0f,1.0f ];
        float[] uv = [ 0f,0f, 1f,0f, 1f,1f, 0f,1f ];
        uint[] i = [ 0,1,3, 1,2,3 ];

        DrawService.CreateBuffer(NID, "aPosition");
        DrawService.SetBufferData(NID, "aPosition", v, 2);

        DrawService.CreateBuffer(NID, "aTextureCoord");
        DrawService.SetBufferData(NID, "aTextureCoord", uv, 2);
            
        DrawService.SetElementBufferData(NID, i);

        DrawService.EnableAtributes(NID, material);

        material.SetUniform("color", _bgColor);
        material.SetUniform("strokeColor", _strokeColor);
        material.SetUniform("strokeSize", _strokeSize);
        material.SetUniform("cornerRadius", _cornerRadius);
    }

    protected override unsafe void Draw(double deltaT)
    {
        var gl = Engine.gl;

        material.Use();

        var fPos = Position - new Vector2<float>(_strokeSize, _strokeSize);
        var fSize = Size + new Vector2<float>(_strokeSize * 2, _strokeSize * 2);

        var world = MathHelper.Matrix4x4CreateRect(fPos, fSize) * Viewport!.Camera2D.GetViewOffset();
        var proj = Viewport!.Camera2D.GetProjection();

        material.SetTranslation(world);
        material.SetProjection(proj);

        material.SetUniform("pixel_size", new Vector2<float>(1,1) / fSize);
        material.SetUniform("size_in_pixels", (Vector2<uint>)fSize);

        DrawService.Draw(NID);
    }

}
