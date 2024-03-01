using GameEngine.Util.Nodes;
using Silk.NET.GLFW;

namespace GameEngineEditor.EditorNodes
{
    internal class SceneEditor2DCamera : Camera2D
    {

        protected override void Process(double deltaT)
        {

            if (Input.IsActionPressed(Keys.W))
                position.Y += (float)(30 * deltaT);

            if (Input.IsActionPressed(Keys.S))
                position.Y -= (float)(30 * deltaT);

            if (Input.IsActionPressed(Keys.A))
                position.X -= (float)(30 * deltaT);

            if (Input.IsActionPressed(Keys.D))
                position.X += (float)(30 * deltaT);

        }

    }
}
