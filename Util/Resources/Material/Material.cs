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

    public Material(string vertexPath, string fragmentPath, string? geometryPath=null)
    {
        _program = GlShaderProgram.CreateOrGet(vertexPath, fragmentPath, geometryPath);

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
                UniformType.FloatVec4   =>  typeof(Color),
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
        var gl = Engine.gl;
        _program.TryBind();

        foreach (var i in _shaderUniforms)
        {
            if (i.Value.value == null) continue;

            if (i.Value.type == typeof(int))
                gl.Uniform1(i.Value.location, (int) i.Value.value);

            else if (i.Value.type == typeof(Color))
                Engine.gl.UniformColor(i.Value.location, (Color) i.Value.value);

            else if (i.Value.type == typeof(Matrix4x4))
                Console.WriteLine("{0} is a Matrix!", i.Key);

        }
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

    public Uniform? GetUInformation(string name)
    {
        if (_shaderUniforms.ContainsKey(name))
            return _shaderUniforms[name];
        else return null;
    }

    #region SetUniforms
    public unsafe void SetUniform(string name, int value)
    {
        var uInfoRes = GetUInformation(name);
       
        if (uInfoRes.HasValue)
        {
            var uInfo = uInfoRes.Value;
            
            if (uInfo.type == typeof(int))
            {
                uInfo.value = value;
                Engine.gl.Uniform1(uInfo.location, value);

                _shaderUniforms[name] = uInfo;
            }
            else throw new ApplicationException(
                string.Format("Uniform {0} is of type {1} and can't use type {2}!",
                name, uInfo.type.Name, typeof(int).Name)
                );
        }
        else Console.WriteLine("Uniform \"{0}\" don't exist!", name);
    }
    public unsafe void SetUniform(string name, Color value)
    {
        var uInfoRes = GetUInformation(name);
       
        if (uInfoRes.HasValue)
        {
            var uInfo = uInfoRes.Value;
            
            if (uInfo.type == typeof(Color))
            {
                uInfo.value = value;
                Engine.gl.UniformColor(uInfo.location, value);

                _shaderUniforms[name] = uInfo;
            }
            else throw new ApplicationException(
                string.Format("Uniform {0} is of type {1} and can't use type {2}!",
                name, uInfo.type.Name, typeof(Color).Name)
                );
        }
        else Console.WriteLine("Uniform \"{0}\" don't exist!", name);
    }
    public unsafe void SetUniform(string name, Matrix4x4 matrix)
    {
        var uInfoRes = GetUInformation(name);
       
        if (uInfoRes.HasValue)
        {
            var uInfo = uInfoRes.Value;
            
            if (uInfo.type == typeof(Matrix4x4))
            {
                uInfo.value = matrix;
                Engine.gl.UniformMatrix4(uInfo.location, 1, true, (float*) &matrix);

                _shaderUniforms[name] = uInfo;
            }
            else throw new ApplicationException(
                string.Format("Uniform {0} is of type {1} and can't use type {2}!",
                name, uInfo.type.Name, typeof(Matrix4x4).Name)
                );
        }
        else Console.WriteLine("Uniform \"{0}\" don't exist!", name);
    }
    
    public unsafe void SetTranslation(Matrix4x4 matrix)
    {
        Engine.gl.UniformMatrix4(worldMatrixLocation, 1, true, (float*) &matrix);
    }
    public unsafe void SetProjection(Matrix4x4 matrix)
    {
        var m = Matrix4x4.CreateScale(1, -1, 1) * matrix;
        Engine.gl.UniformMatrix4(projMatrixLocation, 1, true, (float*) &m);
    }
    #endregion

    public struct Uniform
    {
        public int location;
        public int size;
        public Type type;
        public object? value;
    }

}
