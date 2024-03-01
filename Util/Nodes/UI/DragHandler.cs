using System.Numerics;
using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using Silk.NET.GLFW;

namespace GameEngine.Util.Nodes;

public class DragHandler : NodeUI, ICanvasItem
{

    [Inspect]
    public bool Visible { get; set; } = true;

    public enum Axis {any, XAxis, YAxis}
    [Inspect]
    public Axis dragAxis = Axis.any;

    [Inspect] public NodeUI? nodeA;
    [Inspect] public uint nodeASizeMin = 0;
    [Inspect] public uint nodeASizeMax = 0;

    [Inspect] public NodeUI? nodeB;
    [Inspect] public uint nodeBSizeMin = 0;
    [Inspect] public uint nodeBSizeMax = 0;

    [Inspect] public Color defaultColor = new(0.3f, 0.3f, 0.3f);
    [Inspect] public Color holdingColor = new(0.8f, 0.8f, 0.8f);

    private Color _color = new(0.3f, 0.3f, 0.3f);
    private Color Color
    {
        get { return _color; }
        set {
            _color = value;
            material.SetUniform("color", _color);
        }
    }

    private bool holding = false;

    [Inspect]
    public Material material = new Material2D( Material2D.DrawTypes.SolidColor );

    protected override void Init_()
    {

        Color = defaultColor;

        float[] v = new float[] { 0.0f,0.0f, 1.0f,0.0f, 1.0f,1.0f, 0.0f,1.0f };
        float[] uv = new float[] { 0f,0f, 1f,0f, 1f,1f, 0f,1f };
        uint[] i = new uint[] {0,1,3, 1,2,3};

        DrawService.CreateBuffer(NID, "aPosition");
        DrawService.SetBufferData(NID, "aPosition", v, 2);

        DrawService.CreateBuffer(NID, "aTextureCoord");
        DrawService.SetBufferData(NID, "aTextureCoord", uv, 2);
            
        DrawService.SetElementBufferData(NID, i);

        DrawService.EnableAtributes(NID, material);

    }

    protected override void OnUIInputEvent(Window.InputHandler.InputEvent e)
    {

        var mousePos = Input.GetMousePosition();

        if (mousePos.X > Position.X && mousePos.Y > Position.Y &&
        mousePos.X < Position.X+Size.X && mousePos.Y < Position.Y+Size.Y)
        {
            Color = holdingColor;
            if (Input.IsActionJustPressed(MouseButton.Left))
            {
                holding = true;
                switch (dragAxis)
                {
                    case Axis.any:
                        Input.SetCursorShape(CursorShape.Crosshair); break;
                    case Axis.XAxis:
                        Input.SetCursorShape(CursorShape.HResize); break;
                    case Axis.YAxis:
                        Input.SetCursorShape(CursorShape.VResize); break;
                }
            }

            Viewport?.SupressInputEvent();
        }
        else {
            Color = defaultColor;
        }

        if (holding && Input.IsActionJustReleased(MouseButton.Left))
        {
            holding = false;
            Input.SetCursorShape(CursorShape.Arrow);
        }
        
        if (holding)
        {
            if (dragAxis != Axis.YAxis)
            {
                var d = Input.GetMousePosition().X - Position.X - Size.X/2;

                if (d > 0)
                {
                    if (nodeA != null && nodeASizeMax != 0 && nodeA.Size.X + d >= nodeASizeMax)
                    {
                        var dif = nodeA.Size.X+d - nodeASizeMax;
                        d -= dif;
                    }
                    if (nodeB != null && nodeB.Size.X - d <= nodeBSizeMin)
                    {
                        var dif = nodeBSizeMin - nodeB.Size.X+d;
                        d -= dif;
                    }
                }
                else {
                    if (nodeA != null && nodeA.Size.X + d <= nodeASizeMin)
                    {
                        var dif = nodeA.Size.X+d - nodeASizeMin;
                        d -= dif;
                    }
                    if (nodeB != null && nodeBSizeMax != 0 && nodeB.Size.X - d >= nodeBSizeMax)
                    {
                        var dif = nodeBSizeMax - nodeB.Size.X+d;
                        d -= dif;
                    }
                }

                positionPixels.X += (int) d;
                if (nodeA != null)
                {
                    nodeA.sizePixels.X += (int) d;
                }
                if (nodeB != null)
                {
                    if (nodeB.anchor != ANCHOR.TOP_RIGHT &&
                        nodeB.anchor != ANCHOR.CENTER_RIGHT &&
                        nodeB.anchor != ANCHOR.BOTTOM_RIGHT)
                        nodeB.positionPixels.X += (int) d;
                    
                    nodeB.sizePixels.X -= (int) d;
                }
            }
            if (dragAxis != Axis.XAxis)
            {
                var d = Input.GetMousePosition().Y - Position.Y - Size.Y/2;

                if (d > 0)
                {
                    if (nodeA != null && nodeASizeMax != 0 && nodeA.Size.Y + d >= nodeASizeMax)
                    {
                        var dif = nodeA.Size.Y+d - nodeASizeMax;
                        d -= dif;
                    }
                    if (nodeB != null && nodeB.Size.Y - d <= nodeBSizeMin)
                    {
                        var dif = nodeBSizeMin - nodeB.Size.Y+d;
                        d -= dif;
                    }
                }
                else {
                    if (nodeA != null && nodeA.Size.Y + d <= nodeASizeMin)
                    {
                        var dif = nodeA.Size.Y+d - nodeASizeMin;
                        d -= dif;
                    }
                    if (nodeB != null && nodeBSizeMax != 0 && nodeB.Size.Y - d >= nodeBSizeMax)
                    {
                        var dif = nodeBSizeMax - nodeB.Size.Y+d;
                        d -= dif;
                    }
                }

                positionPixels.Y += (int) d;
                if (nodeA != null)
                {
                    nodeA.sizePixels.Y += (int) d;
                }
                if (nodeB != null)
                {
                    if (nodeB.anchor != ANCHOR.BOTTOM_RIGHT &&
                        nodeB.anchor != ANCHOR.BOTTOM_CENTER &&
                        nodeB.anchor != ANCHOR.BOTTOM_RIGHT)
                        nodeB.positionPixels.Y += (int) d;
                    
                    nodeB.sizePixels.Y -= (int) d;
                }
            }
        }

    }

    protected override unsafe void Draw(double deltaT)
    {
        var gl = Engine.gl;

        material.Use();

        var world = MathHelper.Matrix4x4CreateRect(Position, Size)
        * Matrix4x4.CreateTranslation(new Vector3(-Viewport!.Size.X/2, -Viewport!.Size.Y/2, 0));
        var proj = Matrix4x4.CreateOrthographic(Viewport.Size.X, Viewport.Size.Y,-.1f,.1f);

        material.SetTranslation(world);
        material.SetProjection(proj);

        DrawService.Draw(NID);
    }

    public void Show() { Visible = true; }
    public void Hide() { Visible = false; }

}