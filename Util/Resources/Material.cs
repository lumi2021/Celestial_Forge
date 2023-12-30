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

    public Material() {}
    public Material(string vs, string fs)
    {LoadShaders(vs, fs);}

    public unsafe void Use()
    {
        var gl = Engine.gl;

        gl.UseProgram(_program);

        foreach (var i in uniforms) SetUniform(i.Value);

        // Set projection and world matrices
        Engine.gl.UniformMatrix4(projLoc, 1, true, projectionMatrix.ToArray());
        Engine.gl.UniformMatrix4(worldLoc, 1, true, worldMatrix.ToArray());
    }
    
    private static void SetUniform(UniformConfig u)
    {
        var gl = Engine.gl;

        switch (u.type)
        {
            case UniformType.FloatMat4:
                gl.UniformMatrix4((int) u.loc, 1, true, ((Matrix4x4)u.data).ToArray());
                break;

            case UniformType.FloatVec4:
                gl.Uniform4((int) u.loc, (Vector4) u.data);
                break;
        }
    }

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
    }

    public override void Dispose()
    {
        if (!_disposed) Engine.gl.DeleteProgram(_program);
        base.Dispose();
    }

    public void SetShaderParameter(string name, Color value)
    {
        var gl = Engine.gl;
        int loc = ULocation(name);

        if (loc > 0)
        {

            UniformType t = UType(name);

            if (t != UniformType.FloatVec4)
                throw new ApplicationException(string.Format(
                    "Error! Uniform {0} is of type {1} and can't accept type {2}",
                    name, t, value.GetType().Name));

            var uni = new UniformConfig
            {
                loc = (uint)loc,
                data = value.GetAsNumerics(),
                type = t
            };

            if (uniforms.ContainsKey(name))
                uniforms[name] = uni;
            else uniforms.Add(name, uni);

            SetUniform(uni);

        }
        else Console.WriteLine("Error! Uniform {0} don't exist!", name );

    }
    public void SetShaderParameter(string name, Matrix4x4 value)
    {
        var gl = Engine.gl;
        int loc = ULocation(name);

        if (loc >= 0)
        {

            UniformType t = UType(name);

            if (t != UniformType.FloatMat4)
                throw new ApplicationException(string.Format(
                    "Error! Uniform {0} is of type {1} and can't accept type {2}",
                    name, t.ToString(), value.GetType().Namespace));

            var uni = new UniformConfig
            {
                loc = (uint)loc,
                data = value,
                type = t
            };

            if (uniforms.ContainsKey(name))
                uniforms[name] = uni;
            else uniforms.Add(name, uni);

            SetUniform(uni);

        }
        else throw new ApplicationException(string.Format( "Error! Uniform {0} don't exist!", name ));

    }

    public void SetShaderProjectionMatrix(Matrix4x4 value)
    {
        projectionMatrix = value;
        Engine.gl.UniformMatrix4(projLoc, 1, true, value.ToArray());
    }
    public void SetShaderWorldMatrix(Matrix4x4 value)
    {
        worldMatrix = value;
        Engine.gl.UniformMatrix4(worldLoc, 1, true, value.ToArray());
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


    private struct UniformConfig
    {
        public uint loc;
        public UniformType type;
        public object data;
    }
}