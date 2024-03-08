using System.Numerics;
using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;

namespace GameEngine.Util.Nodes;

public class TextureRect : NodeUI, ICanvasItem
{

    [Inspect]
    public bool Visible { get; set; } = true;

    [Inspect]
    public Texture? texture = null;
    [Inspect]
    public Material material = new Material2D( Material2D.DrawTypes.Texture );

    protected override void Init_()
    {

        float[] v = [0.0f,0.0f, 1.0f,0.0f, 1.0f,1.0f, 0.0f,1.0f];
        float[] uv = [0f,0f, 1f,0f, 1f,1f, 0f,1f];
        uint[] i = [0,1,3, 1,2,3];

        DrawService.CreateBuffer(NID, "aPosition");
        DrawService.SetBufferData(NID, "aPosition", v, 2);

        DrawService.CreateBuffer(NID, "aTextureCoord");
        DrawService.SetBufferData(NID, "aTextureCoord", uv, 2);
            
        DrawService.SetElementBufferData(NID, i);

        DrawService.EnableAtributes(NID, material);

    }

    protected override void Draw(double deltaT)
    {
        texture?.Use();
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