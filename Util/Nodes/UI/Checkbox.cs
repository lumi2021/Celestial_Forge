using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Enums;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;

namespace GameEngine.Util.Nodes;

[Icon("./Assets/icons/Nodes/Checkbox.svg")]
internal class Checkbox : NodeUI
{

    [Inspect]
    public bool value = false;

    [Inspect]
    public Texture? actived_texture = null; 
    public Texture? unactived_texture = null;

    private Color _color = new();

    [Inspect]
    public Material material = new Material2D(Material2D.DrawTypes.Texture);

    public readonly Signal OnValueChange = new();

    protected override void Init_()
    {
        float[] v = new float[] { 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 1.0f };
        float[] uv = new float[] { 0f, 0f, 1f, 0f, 1f, 1f, 0f, 1f };
        uint[] i = new uint[] { 0, 1, 3, 1, 2, 3 };

        DrawService.CreateBuffer(NID, "aPosition");
        DrawService.SetBufferData(NID, "aPosition", v, 2);

        DrawService.CreateBuffer(NID, "aTextureCoord");
        DrawService.SetBufferData(NID, "aTextureCoord", uv, 2);

        DrawService.SetElementBufferData(NID, i);

        DrawService.EnableAtributes(NID, material);
    }
    protected override void Draw(double deltaT)
    {
        var gl = Engine.gl;

        material.Use();
        bool useTexture = false;

        if (value)
        {
            if (actived_texture != null)
            {
                actived_texture.Use();
                useTexture = true;
            }
            else
            {
                _color = new Color(0, 0, 255);
                useTexture = false;
            }
        }
        else
        {
            if (unactived_texture != null)
            {
                unactived_texture.Use();
                useTexture = true;
            }
            else
            {
                _color = new Color(0, 0, 0);
                useTexture = false;
            }
        }

        material.SetUniform("color", _color);
        material.SetUniform("configDrawType", useTexture? 1 : 0);

        var world = MathHelper.Matrix4x4CreateRect(Position, Size) * Viewport!.Camera2D.GetViewOffset();
        var proj = Viewport!.Camera2D.GetProjection();

        material.SetTranslation(world);
        material.SetProjection(proj);


        DrawService.Draw(NID);

    }

    protected override void OnUIInputEvent(InputEvent e)
    {
        if (e.Is<MouseInputEvent>())
        {
            if (mouseFilter == MouseFilter.Ignore) return;

            if (e.Is<MouseBtnInputEvent>(out var @bEvent))
            {

                if (new Rect(Position, Size).Intersects(@bEvent.position + Viewport!.Camera2D.position))
                {
                    if (@bEvent.action == InputAction.Press)
                    {
                        value = !value;
                        OnValueChange.Emit(this, value);
                        OnClick.Emit(this);
                    }

                    if (mouseFilter == MouseFilter.Block)
                        Viewport.SupressInputEvent();
                }
            }
        }
    }

}
