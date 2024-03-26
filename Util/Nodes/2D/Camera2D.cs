using System.Numerics;
using GameEngine.Util.Attributes;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

public class Camera2D : Node2D
{

    [Inspect]
    public bool Current
    {
        get => Viewport!.Camera2D == this;
        set
        {
            if (!value && Viewport!.Camera2D == this)
                Viewport.SetCurrentCamera(null);

            else if (value && Viewport!.Camera2D != this)
                Viewport.SetCurrentCamera(this);
        }
    }

    [Inspect]
    public Vector2<float> zoom = new(1, 1);

    public Matrix4x4 GetProjection() =>
        Matrix4x4.CreateOrthographic(Viewport!.ViewportSize.X/zoom.X,Viewport!.ViewportSize.Y/zoom.Y,-.1f,.1f);

    public Matrix4x4 GetViewOffset() =>
        Matrix4x4.CreateTranslation((-Viewport!.ViewportSize.X/2) - position.X, (-Viewport!.ViewportSize.Y/2) - position.Y, 0);

}