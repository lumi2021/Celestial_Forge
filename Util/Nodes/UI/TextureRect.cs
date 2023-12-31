using System.Numerics;
using GameEngine.Sys;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;

namespace GameEngine.Util.Nodes;

public class TextureRect : NodeUI, ICanvasItem
{

    public bool Visible { get; set; } = true;

    public Texture? texture = null;
    private Material mat = DrawService.Standard2DMaterial;

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

    }

    protected override void Draw(double deltaT)
    {
        mat.Use( RID );
        texture?.Use();

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