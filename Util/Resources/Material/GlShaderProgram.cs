using GameEngine.Core;
using Silk.NET.OpenGL;

namespace GameEngine.Util.Resources;

public sealed class GlShaderProgram : Resource
{

    private uint _program;
    public uint Handler { get { return _program; }}

    public FileReference vertexShader;
    public FileReference fragmentShader;

    private GlShaderProgram(string vertexFilePath, string fragmentFilePath)
    {
        vertexShader = new(vertexFilePath);
        fragmentShader = new(fragmentFilePath);

        string vertexCode = vertexShader.ReadAllFile();
        string fragmentCode = fragmentShader.ReadAllFile();

        Compile(vertexCode, fragmentCode);

        ResourceHeap.AddShaderProgramReference(this);
    }
    private GlShaderProgram(FileReference vertexFile, FileReference fragmentFile)
    {
        vertexShader = vertexFile;
        fragmentShader = fragmentFile;

        string vertexCode = vertexShader.ReadAllFile();
        string fragmentCode = fragmentShader.ReadAllFile();

        Compile(vertexCode, fragmentCode);

        ResourceHeap.AddShaderProgramReference(this);
    }

    private void Compile(string vertexCode, string fragmentCode)
    {
        var gl = Engine.gl;

        uint vertexSdr = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexSdr, vertexCode);

        gl.CompileShader(vertexSdr);

        gl.GetShader(vertexSdr, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + gl.GetShaderInfoLog(vertexSdr));

         uint fragmentSdr = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentSdr, fragmentCode);

        gl.CompileShader(fragmentSdr);

        gl.GetShader(fragmentSdr, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + gl.GetShaderInfoLog(fragmentSdr));

        _program = gl.CreateProgram();

        gl.AttachShader(_program, vertexSdr);
        gl.AttachShader(_program, fragmentSdr);

        gl.LinkProgram(_program);

        gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + gl.GetProgramInfoLog(_program));

        gl.DetachShader(_program, vertexSdr);
        gl.DetachShader(_program, fragmentSdr);
        gl.DeleteShader(vertexSdr);
        gl.DeleteShader(fragmentSdr);
    }

    public void TryBind()
    {

        if (DrawService.GlBinded_ShaderProgram != _program)
        {
            Engine.gl.UseProgram(_program);
            DrawService.GlBinded_ShaderProgram = (int) _program;
        }

    }

    public override void Dispose()
    {
        Engine.gl.DeleteProgram(_program);
        //ResourceHeap.RemoveShaderProgramReference(this);
        base.Dispose();
    }

    public static GlShaderProgram CreateOrGet(string vertexFilePath, string fragmentFilePath)
    {
        var vertex = new FileReference(vertexFilePath);
        var fragment = new FileReference(fragmentFilePath);

        var res = ResourceHeap.GetShaderProgramReference(vertex, fragment);

        if (res != null) return res;
        else return new GlShaderProgram(vertexFilePath, fragmentFilePath);
    }
    public static GlShaderProgram CreateOrGet(FileReference vertexFile, FileReference fragmentFile)
    {
        var res = ResourceHeap.GetShaderProgramReference(vertexFile, fragmentFile);

        if (res != null) return res;
        else return new GlShaderProgram(vertexFile, fragmentFile);
    }

}