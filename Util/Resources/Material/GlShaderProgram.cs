using GameEngine.Core;
using Silk.NET.OpenGL;

namespace GameEngine.Util.Resources;

public sealed class GlShaderProgram : Resource
{

    private uint _program;
    public uint Handler { get { return _program; }}

    public FileReference vertexShader;
    public FileReference fragmentShader;
    public FileReference? geometryShader;

    private GlShaderProgram(string vertexFilePath, string fragmentFilePath, string? geometryFilePath=null)
    {
        vertexShader = new(vertexFilePath);
        fragmentShader = new(fragmentFilePath);
        if (geometryFilePath != null)
        geometryShader = new(geometryFilePath);

        string vertexCode = vertexShader.ReadAllFile();
        string fragmentCode = fragmentShader.ReadAllFile();
        string? geometryCode = geometryFilePath != null ? geometryShader?.ReadAllFile() : null;

        Compile(vertexCode, fragmentCode, geometryCode);

        ResourceHeap.AddShaderProgramReference(this);
    }
    private GlShaderProgram(FileReference vertexFile, FileReference fragmentFile, FileReference? geometryFile=null)
    {
        vertexShader = vertexFile;
        fragmentShader = fragmentFile;
        geometryShader = geometryFile;

        string vertexCode = vertexShader.ReadAllFile();
        string fragmentCode = fragmentShader.ReadAllFile();
        string? geometryCode = geometryShader != null ? geometryShader?.ReadAllFile() : null;

        Compile(vertexCode, fragmentCode, geometryCode);

        ResourceHeap.AddShaderProgramReference(this);
    }

    private void Compile(string vertexCode, string fragmentCode, string? geometryCode=null)
    {
        var gl = Engine.gl;
        bool useGeometry = geometryCode != null;

        #region vertex creation/compilation & error handler
        uint vertexSdr = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexSdr, vertexCode);
    
        gl.CompileShader(vertexSdr);

        gl.GetShader(vertexSdr, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + gl.GetShaderInfoLog(vertexSdr));
        #endregion

        #region geometry creation/compilation & error handler
        uint geometrySdr = gl.CreateShader(ShaderType.GeometryShader);
        if (useGeometry) {
            gl.ShaderSource(geometrySdr, geometryCode);

            gl.CompileShader(geometrySdr);

            gl.GetShader(geometrySdr, ShaderParameterName.CompileStatus, out int gStatus);
            if (gStatus != (int) GLEnum.True)
                throw new Exception("Geometry shader failed to compile: " + gl.GetShaderInfoLog(geometrySdr));
        }
        #endregion

        #region fragment creation/compilation & error handler
        uint fragmentSdr = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentSdr, fragmentCode);

        gl.CompileShader(fragmentSdr);

        gl.GetShader(fragmentSdr, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + gl.GetShaderInfoLog(fragmentSdr));
        #endregion

        _program = gl.CreateProgram();

        gl.AttachShader(_program, vertexSdr);
        if (useGeometry) gl.AttachShader(_program, geometrySdr);
        gl.AttachShader(_program, fragmentSdr);

        gl.LinkProgram(_program);

        gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + gl.GetProgramInfoLog(_program));

        gl.DetachShader(_program, vertexSdr);
        if (useGeometry) gl.DetachShader(_program, geometrySdr);
        gl.DetachShader(_program, fragmentSdr);
        gl.DeleteShader(vertexSdr);
        gl.DeleteShader(geometrySdr);
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

    public static GlShaderProgram CreateOrGet(string vertexFilePath, string fragmentFilePath, string? geometryFilePath)
    {
        FileReference vertex = new(vertexFilePath);
        FileReference fragment = new(fragmentFilePath);
        FileReference? geometry = geometryFilePath != null ? new(geometryFilePath) : null;

        var res = ResourceHeap.GetShaderProgramReference(vertex, fragment, geometry);

        if (res != null) return res;
        else return new GlShaderProgram(vertexFilePath, fragmentFilePath, geometryFilePath);
    }
    public static GlShaderProgram CreateOrGet(FileReference vertexFile, FileReference fragmentFile, FileReference geometryFile)
    {
        var res = ResourceHeap.GetShaderProgramReference(vertexFile, fragmentFile, geometryFile);

        if (res != null) return res;
        else return new GlShaderProgram(vertexFile, fragmentFile, geometryFile);
    }

}