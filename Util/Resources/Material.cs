using GameEngine.Sys;
using Silk.NET.OpenGL;

namespace GameEngine.Util.Resources;

public class Material : Resource
{
    private uint _program;

    public Material() {}
    public Material(string vs, string fs)
    {LoadShaders(vs, fs);}

    public unsafe void Use() {Engine.gl.UseProgram(_program);}

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
    }

    public override void Dispose()
    {
        if (!_disposed) Engine.gl.DeleteProgram(_program);
        base.Dispose();
    }

    public int ALocation(string name)
    {
        return Engine.gl.GetAttribLocation(_program, name);
    }
    public int ULocation(string name)
    {
        return Engine.gl.GetUniformLocation(_program, name);
    }
}
