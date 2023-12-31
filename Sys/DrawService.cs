using System.Numerics;
using System.Runtime.InteropServices;
using GameEngine.Util;
using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;

namespace GameEngine.Sys;

public static class DrawService
{
    /* see about performace
    
    ** as niki said, the use of a dictionaty to get the buffers using IDs
    ** can be really bad when the dictionary have to be consulted a lot
    ** of times :(
    */

    public enum BufferUsage { Static, Dynamic, Stream };

    private static Dictionary<uint, ResourceDrawData> ResourceData = new();

    /* Standard material reference */
    public static readonly Material Standard2DMaterial;

    static DrawService()
    {
        Standard2DMaterial = new(
            FileService.GetFile("./Data/Shaders/standard2dMaterial.vert"),
            FileService.GetFile("./Data/Shaders/standard2dMaterial.frag")
        );
    }

    public static void CreateCanvasItem(uint RID)
    {
        ResourceData.Add(RID, new ResourceDrawData());
    }

    public static uint CreateBuffer(uint RID, string bufferName)
    {
        return ResourceData[RID].CreateBuffer(bufferName);
    }

    #region SetBufferData Methods
    public static unsafe void SetBufferData(uint RID, string buffer, float[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        VertexData vertexData = ResourceData[RID].VertexBuffers[buffer];

        gl.BindVertexArray(ResourceData[RID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(float* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(float)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(float);
        ResourceData[RID].VertexBuffers[buffer] = vertexData;
    }
    public static unsafe void SetBufferData(uint RID, string buffer, double[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        VertexData vertexData = ResourceData[RID].VertexBuffers[buffer];

        gl.BindVertexArray(ResourceData[RID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(double* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(double)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(double);
        ResourceData[RID].VertexBuffers[buffer] = vertexData;
    }
    public static unsafe void SetBufferData(uint RID, string buffer, byte[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        VertexData vertexData = ResourceData[RID].VertexBuffers[buffer];

        gl.BindVertexArray(ResourceData[RID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(byte* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(byte)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(byte);
        ResourceData[RID].VertexBuffers[buffer] = vertexData;
    }
    public static unsafe void SetBufferData(uint RID, string buffer, int[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        VertexData vertexData = ResourceData[RID].VertexBuffers[buffer];

        gl.BindVertexArray(ResourceData[RID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(int* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(int)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(int);
        ResourceData[RID].VertexBuffers[buffer] = vertexData;
    }
    
    public static unsafe void SetBufferData(uint RID, uint id, float[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        var a = ResourceData[RID].VertexBuffers.ToArray()[id];
        VertexData vertexData = a.Value;

        gl.BindVertexArray(ResourceData[RID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(float* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(float)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(float);
        ResourceData[RID].VertexBuffers[a.Key] = vertexData;
    }
    public static unsafe void SetBufferData(uint RID, uint id, double[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        var a = ResourceData[RID].VertexBuffers.ToArray()[id];
        VertexData vertexData = a.Value;

        gl.BindVertexArray(ResourceData[RID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(double* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(double)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(double);
        ResourceData[RID].VertexBuffers[a.Key] = vertexData;
    }
    public static unsafe void SetBufferData(uint RID, uint id, byte[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        var a = ResourceData[RID].VertexBuffers.ToArray()[id];
        VertexData vertexData = a.Value;

        gl.BindVertexArray(ResourceData[RID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(byte* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(byte)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(byte);
        ResourceData[RID].VertexBuffers[a.Key] = vertexData;
    }
    public static unsafe void SetBufferData(uint RID, uint id, int[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        var a = ResourceData[RID].VertexBuffers.ToArray()[id];
        VertexData vertexData = a.Value;

        gl.BindVertexArray(ResourceData[RID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(int* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(int)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(int);
        ResourceData[RID].VertexBuffers[a.Key] = vertexData;
    }
    #endregion

    #region Operations with instances
    public static void SetBufferAtribDivisor(uint RID, string buffer, uint divisor)
    {
        var gl = Engine.gl;

        VertexData vertexData = ResourceData[RID].VertexBuffers[buffer];
        vertexData.divisions = divisor;
        ResourceData[RID].VertexBuffers[buffer] = vertexData;
    }
    public static void SetBufferAtribDivisor(uint RID, uint id, uint divisor)
    {
        var gl = Engine.gl;

        var a = ResourceData[RID].VertexBuffers.ToArray()[id];
        VertexData vertexData = a.Value;
        vertexData.divisions = divisor;
        ResourceData[RID].VertexBuffers[a.Key] = vertexData;
    }
    
    public static void EnableInstancing(uint RID, uint instanceCount)
    {
        var gl = Engine.gl;

        var res = ResourceData[RID];

        if (instanceCount > 0)
        {
            res.useInstancing = true;
            res.instanceCount = instanceCount;
        }
        else res.useInstancing = false;

        ResourceData[RID] = res;
    }
    #endregion

    #region Operations with Shaders
    public static void SetShaderParameter(uint RID, Material mat, string name, Color value)
    {
        ResourceDrawData res = ResourceData[RID];
        int loc = mat.ULocation(name);

        if (loc > 0)
        {

            UniformType t = mat.UType(name);

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

            if (res.shaderUniforms.ContainsKey(name))
                res.shaderUniforms[name] = uni;
            else res.shaderUniforms.Add(name, uni);
        }
        else Console.WriteLine("Error! Uniform {0} don't exist!", name );

    }
    public static void SetShaderParameter(uint RID, Material mat, string name, Matrix4x4 value)
    {
        ResourceDrawData res = ResourceData[RID];
        int loc = mat.ULocation(name);

        if (loc >= 0)
        {

            UniformType t = mat.UType(name);

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

            if (res.shaderUniforms.ContainsKey(name))
                res.shaderUniforms[name] = uni;
            else res.shaderUniforms.Add(name, uni);

            ResourceData[RID] = res;

        }
        else Console.WriteLine("Error! Uniform {0} don't exist!", name );

    }

    public static void UseShader(uint RID)
    {
        ResourceDrawData res = ResourceData[RID];
        foreach (var i in res.shaderUniforms)
        SetUniform(i.Value);
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
    #endregion

    public static unsafe void SetElementBufferData(uint RID, uint[] data, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        uint bufferId = ResourceData[RID].ElementBuffer;

        gl.BindVertexArray(ResourceData[RID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(uint* buf = data)
        gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(data.Length * sizeof(uint)), buf, currentUsage);

        var a = ResourceData[RID];
        a.elementsLength = (uint) data.Length;
        ResourceData[RID] = a;
    }
    
    public static unsafe void EnableAtributes(uint RID, Material material)
    {
        var gl = Engine.gl;
        var res = ResourceData[RID];

        foreach (var i in res.VertexBuffers)
        {
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, i.Value.bufferId);
            int iloc = material.ALocation(i.Key);
            if (iloc < 0) continue;

            uint loc = (uint) iloc;
            var ts = Marshal.SizeOf(i.Value.type);

            #region get correct type
            VertexAttribPointerType type = VertexAttribPointerType.Float;
            if (i.Value.type == typeof(int))
                type = VertexAttribPointerType.Int;
            else if (i.Value.type == typeof(byte))
                type = VertexAttribPointerType.Byte;
            else if (i.Value.type == typeof(double))
                type = VertexAttribPointerType.Double;
            #endregion

            if (i.Value.size < 16)
            {
                gl.EnableVertexAttribArray(loc);
                gl.VertexAttribPointer(loc, i.Value.size, type, false, (uint)(i.Value.size*ts), (void*) 0);
                gl.VertexAttribDivisor(loc, i.Value.divisions);
            }
            else for (uint j = 0; j < 4; j++)
            {
                gl.EnableVertexAttribArray(loc+j);
                gl.VertexAttribPointer(loc+j, 4, type, false, (uint)(16*ts), (void*) (j*4*ts));
                gl.VertexAttribDivisor(loc+j, i.Value.divisions);
            }
        }
    }
    
    public static unsafe void Draw(uint RID)
    {
        var res = ResourceData[RID];
        Engine.gl.BindVertexArray(res.VertexArray);

        if (!res.useInstancing)
            Engine.gl.DrawElements(PrimitiveType.Triangles, res.elementsLength,
            DrawElementsType.UnsignedInt, (void*) 0);
        else
            Engine.gl.DrawElementsInstanced(PrimitiveType.Triangles, res.elementsLength,
            DrawElementsType.UnsignedInt, (void*) 0, res.instanceCount);
    }

}

struct ResourceDrawData
{
    // opengl info
    public uint VertexArray = 0;
    public Dictionary<string, VertexData> VertexBuffers = new();
    public uint ElementBuffer = 0;
    public uint elementsLength = 0;
    public uint instanceCount = 0;

    public Dictionary<string, UniformConfig> shaderUniforms = new();

    // configs
    public bool useInstancing = false;


    public ResourceDrawData()
    {
        VertexArray = Engine.gl.GenVertexArray();
        ElementBuffer = Engine.gl.GenBuffer();
    }

    public uint CreateBuffer(string bufferName)
    {
        if (VertexBuffers.ContainsKey(bufferName))
            throw new ApplicationException(string.Format("Buffer {0} already exists inside this resource!", bufferName));

        var vertData = new VertexData();
        var id = Engine.gl.GenBuffer();
        vertData.bufferId = id;
        VertexBuffers.Add(bufferName, vertData);

        return (uint)(VertexBuffers.Count - 1);
    }
}
struct VertexData
{
    public uint bufferId = 0;
    public int size = 0;
    public Type type = typeof(float);
    public uint divisions = 0;

    public VertexData() {}
}
struct UniformConfig
{
    public uint loc;
    public UniformType type;
    public object data;
}