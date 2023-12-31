using System.Numerics;
using GameEngine.Sys;
using GameEngine.Util.Values;
using Silk.NET.OpenGL;

namespace GameEngine.Util.Resources;

public class Material : Resource
{
    private uint _program;

    private readonly Dictionary<string, UniformConfig> uniforms = new();

    private Matrix4x4 projectionMatrix = Matrix4x4.Identity;
    private Matrix4x4 worldMatrix = Matrix4x4.Identity;

    private int projLoc = -1;
    private int worldLoc = -1;
    private int drawTypeLoc = -1;

    public Material() {}
    public Material(string vs, string fs)
    {LoadShaders(vs, fs);}

    public void Bind()
    {
        Engine.gl.UseProgram(_program);

    }
    public void Use( uint RID )
    {
        DrawService.UseShader(RID);

    }

    public void SetShaderParameter(uint RID, string name, Color value)
    { DrawService.SetShaderParameter(RID, this, name, value); }
    public void SetShaderParameter(uint RID, string name, Matrix4x4 value)
    { DrawService.SetShaderParameter(RID, this, name, value); }

    public void LoadShaders(string vertexCode, string fragmentCode)
    {
        var gl = Engine.gl;

        uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, vertexCode);

        gl.CompileShader(vertexShader);

        gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + gl.GetShaderInfoLog(vertexShader));

         uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, fragmentCode);

        gl.CompileShader(fragmentShader);

        gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + gl.GetShaderInfoLog(fragmentShader));

        _program = gl.CreateProgram();

        gl.AttachShader(_program, vertexShader);
        gl.AttachShader(_program, fragmentShader);

        gl.LinkProgram(_program);

        gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + gl.GetProgramInfoLog(_program));

        gl.DetachShader(_program, vertexShader);
        gl.DetachShader(_program, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        // get default information
        projLoc = ULocation("projection");
        worldLoc = ULocation("world");
        drawTypeLoc = ULocation("configDrawType");
    }

    public override void Dispose()
    {
        if (!_disposed) Engine.gl.DeleteProgram(_program);
        base.Dispose();
    }

    public void SetShaderProjectionMatrix(Matrix4x4 value)
    {
        projectionMatrix = value * Matrix4x4.CreateScale(1, -1, 1);
        Engine.gl.UniformMatrix4(projLoc, 1, true, projectionMatrix.ToArray());
    }
    public void SetShaderWorldMatrix(Matrix4x4 value)
    {
        worldMatrix = value;
        Engine.gl.UniformMatrix4(worldLoc, 1, true, worldMatrix.ToArray());
    }

    public int ALocation(string name)
    {
        return Engine.gl.GetAttribLocation(_program, name);
    }
    public int ULocation(string name)
    {
        return Engine.gl.GetUniformLocation(_program, name);
    }
    public UniformType UType(string name)
    {
        int loc = ULocation(name);

        int size;
        UniformType type;

        Engine.gl.GetActiveUniform(_program, (uint) loc, out size, out type);

        return type;
    }


    public enum DrawType
    {
        SolidColor,
        Texture,
        Text
    }

}