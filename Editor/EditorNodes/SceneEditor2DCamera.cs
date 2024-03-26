﻿using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
using GameEngine.Util.Enums;

namespace GameEngineEditor.EditorNodes;

internal class SceneEditor2DCamera : Camera2D
{

    private const float zoomSens = 0.5f;
    private const float speed = 30.0f;

    protected override void Process(double deltaT)
    {

        if (Input.IsActionPressed(Keys.W))
            position.Y -= (float)(speed / zoom.Y * deltaT);

        if (Input.IsActionPressed(Keys.S))
            position.Y += (float)(speed / zoom.Y * deltaT);

        if (Input.IsActionPressed(Keys.A))
            position.X -= (float)(speed / zoom.X * deltaT);

        if (Input.IsActionPressed(Keys.D))
            position.X += (float)(speed / zoom.X * deltaT);

    }

    protected override void OnInputEvent(InputEvent e)
    {
        
        if (e.Is<MouseScrollInputEvent>(out var scroll))
        {
            
            if (scroll.offset.Y != 0)
            {
                if (scroll.offset.Y > 0)
                    zoom /= Math.Abs(scroll.offset.Y * zoomSens);
                else
                    zoom *= Math.Abs(@scroll.offset.Y * zoomSens);
            }

        }

    }

}
