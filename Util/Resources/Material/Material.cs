using System.Drawing;
using System.Numerics;
using GameEngine.Core;
using GameEngine.Util.Values;
using Silk.NET.OpenGL;

namespace GameEngine.Util.Resources;

public abstract class Material : Resource
{

    protected GlShaderProgram _program;

    protected Matrix4x4 Transform = Matrix4x4.Identity;
    protected Matrix4x4 Projection = Matrix4x4.Identity;

    protected Dictionary<string, Uniform> _shaderUniforms = new();
    protected Dictionary<string, int> _shaderAttributes = new();

    protected int worldMatrixLocation;
    protected int projMatrixLocation;

    public Material(string VertexPath, string FragmentPath)
    {
        _program = GlShaderProgram.CreateOrGet(VertexPath, FragmentPath);

        int uniformsCount = Engine.gl.GetProgram(_program.Handler, GLEnum.ActiveUniforms);
        int attributCount = Engine.gl.GetProgram(_program.Handler, GLEnum.ActiveAttributes);
        LoadUniforms(uniformsCount);
        LoadAtributes(attributCount);

        worldMatrixLocation = GetULocation("world");
        projMatrixLocation  = GetULocation("projection");
    }
    private void LoadUniforms(int count)
    {
        var gl = Engine.gl;
        for (int i = 0; i < count; i++)
        {
            int size;
            UniformType glType;
            string name;

            name = gl.GetActiveUniform(_program.Handler, (uint) i, out size, out glType);

            Type type = glType switch
            {
                UniformType.Int         =>  typeof(int),
                UniformType.UnsignedInt =>  typeof(uint),
                UniformType.Float       =>  typeof(float),
                UniformType.Double      =>  typeof(double),
                UniformType.FloatVec2   =>  typeof(Vector2<float>),
                UniformType.Bool        =>  typeof(bool),
                UniformType.FloatMat4   =>  typeof(Matrix4x4),
                UniformType.Sampler2D   =>  typeof(Texture),
                _                       =>  typeof(void)
            };

            var nuni = new Uniform()
            { 
                location = gl.GetUniformLocation(_program.Handler, name),
                size = size,
                type = type
            };

            _shaderUniforms.Add(name, nuni);
        }
    }
    private void LoadAtributes(int count)
    {
        var gl = Engine.gl;
        for (int i = 0; i < count; i++)
        {
            int size;
            AttributeType glType;
            string name;

            name = gl.GetActiveAttrib(_program.Handler, (uint) i, out size, out glType);

            _shaderAttributes.Add(name, gl.GetAttribLocation(_program.Handler, name));
        }
    }

    public virtual void Use()
    {
        _program.TryBind();
    }

    public int GetULocation(string name)
    {
        if (_shaderUniforms.ContainsKey(name))
            return _shaderUniforms[name].location;
        else return -1;
    }
    public int GetALocation(string name)
    {
        if (_shaderAttributes.ContainsKey(name))
            return _shaderAttributes[name];
        else return -1;
    }

    public unsafe void SetUniform(string name, Matrix4x4 matrix)
    {
        Engine.gl.UniformMatrix4(GetULocation(name), 1, true, (float*) &matrix);
    }


    protected struct Uniform
    {
        public int location;
        public int size;
        public Type type;
        public object? Value;
    }

}
