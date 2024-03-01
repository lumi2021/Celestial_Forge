using System.Numerics;
using Silk.NET.GLFW;

namespace GameEngine.Util.Nodes;

public class Camera2D : Node2D
{

    public bool Current { get; set; }

    public Matrix4x4 GetProjection() =>
        Matrix4x4.CreateOrthographic(Viewport!.Size.X,Viewport!.Size.Y,-.1f,.1f);

    public Matrix4x4 GetViewOffset() =>
        Matrix4x4.CreateTranslation(
            (-Viewport!.Size.X/2) - position.X,
            (-Viewport!.Size.Y/2) - position.Y,
            0
        );


    protected override void Process(double deltaT)
    {
       
        if (Input.IsActionPressed(Keys.W))
            position.Y += (float) (30 * deltaT);
        
        if (Input.IsActionPressed(Keys.S))
            position.Y -= (float) (30 * deltaT);
        
        if (Input.IsActionPressed(Keys.A))
            position.X -= (float) (30 * deltaT);
        
        if (Input.IsActionPressed(Keys.D))
            position.X += (float) (30 * deltaT);
        

    }

}