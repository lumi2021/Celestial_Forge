using GameEngine.Core;

namespace GameEngine.Util.Resources;

public class Material2D : Material
{

    private const string _vsp = "./Data/Shaders/standard2dMaterial.vs";
    private const string _fsp = "./Data/Shaders/standard2dMaterial.fs";

    public enum DrawTypes
    {
        SolidColor,
        Texture,
        Text
    }

    protected readonly DrawTypes DrawType = 0;
    protected readonly int DrawTypeLocation;

    public Material2D(DrawTypes type): base(_vsp, _fsp)
    {
        DrawType = type;
        DrawTypeLocation = GetULocation("configDrawType");
    }

    public override void Use()
    {
        base.Use();
        Engine.gl.Uniform1(DrawTypeLocation, (int) DrawType);
    }

}
