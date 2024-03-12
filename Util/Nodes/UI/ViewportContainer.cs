using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;

namespace GameEngine.Util.Nodes;

public class ViewportContainer : NodeUI
{

    [Inspect] public Viewport? linkedViewport = null;

    [Inspect] public Material material = new Material2D( Material2D.DrawTypes.Texture );

    protected override void Init_()
    {

        float[] v = [0.0f,0.0f, 1.0f,0.0f, 1.0f,1.0f, 0.0f,1.0f];
        float[] uv = [0f,1f, 1f,1f, 1f,0f, 0f,0f];
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
        
        if (linkedViewport == null) return;

        /* call the render process of the viewport */
        linkedViewport.Render( new((uint)Size.X, (uint)Size.Y) , deltaT);
        
        /* reconfig draw */
        if (parent is IClipChildren)
        {
            var clipRect = (parent as IClipChildren)!.GetClippingArea();
            clipRect = clipRect.InvertVerticallyIn( new(0, 0, Viewport!.Size.X, Viewport!.Size.Y) );
            Engine.gl.Scissor(clipRect);
        }
        else Engine.gl.Scissor(new(Position, Size));

        linkedViewport.Use();
        material.Use();

        var world = MathHelper.Matrix4x4CreateRect(Position, Size) * Viewport!.Camera2D.GetViewOffset();
        var proj = Viewport!.Camera2D.GetProjection();

        material.SetTranslation(world);
        material.SetProjection(proj);

        DrawService.Draw(NID);

    }

}
