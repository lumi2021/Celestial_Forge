using System.Runtime.InteropServices;
using GameEngine.Util;
using GameEngine.Util.Resources;
using Silk.NET.OpenGL;

namespace GameEngine.Core;

public static class DrawService
{
    /* see about performace
    
    ** as niki said, the use of a dictionaty to get the buffers using IDs
    ** can be really bad when the dictionary have to be consulted a lot
    ** of times :(
    */

    public enum BufferUsage { Static, Dynamic, Stream };

    private static Dictionary<uint, ResourceDrawData> ResourceData = new();

    #region Gl Binded data
    public static int GlBinded_ShaderProgram = -1;
    #endregion

    public static void CreateCanvasItem(uint NID)
    {
        ResourceData.Add(NID, new ResourceDrawData());
    }
    public static void DeleteCanvasItem(uint NID)
    {
        if (ResourceData.ContainsKey(NID))
            ResourceData.Remove(NID);
    }

    public static uint CreateBuffer(uint NID, string bufferName)
    {
        return ResourceData[NID].CreateBuffer(bufferName);
    }

    #region SetBufferData Methods
    public static unsafe void SetBufferData(uint NID, string buffer, float[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        VertexData vertexData = ResourceData[NID].VertexBuffers[buffer];

        gl.BindVertexArray(ResourceData[NID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(float* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(float)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(float);
        ResourceData[NID].VertexBuffers[buffer] = vertexData;
    }
    public static unsafe void SetBufferData(uint NID, string buffer, double[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        VertexData vertexData = ResourceData[NID].VertexBuffers[buffer];

        gl.BindVertexArray(ResourceData[NID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(double* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(double)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(double);
        ResourceData[NID].VertexBuffers[buffer] = vertexData;
    }
    public static unsafe void SetBufferData(uint NID, string buffer, byte[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        VertexData vertexData = ResourceData[NID].VertexBuffers[buffer];

        gl.BindVertexArray(ResourceData[NID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(byte* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(byte)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(byte);
        ResourceData[NID].VertexBuffers[buffer] = vertexData;
    }
    public static unsafe void SetBufferData(uint NID, string buffer, int[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        VertexData vertexData = ResourceData[NID].VertexBuffers[buffer];

        gl.BindVertexArray(ResourceData[NID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(int* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(int)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(int);
        ResourceData[NID].VertexBuffers[buffer] = vertexData;
    }
    
    public static unsafe void SetBufferData(uint NID, uint id, float[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        var a = ResourceData[NID].VertexBuffers.ToArray()[id];
        VertexData vertexData = a.Value;

        gl.BindVertexArray(ResourceData[NID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(float* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(float)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(float);
        ResourceData[NID].VertexBuffers[a.Key] = vertexData;
    }
    public static unsafe void SetBufferData(uint NID, uint id, double[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        var a = ResourceData[NID].VertexBuffers.ToArray()[id];
        VertexData vertexData = a.Value;

        gl.BindVertexArray(ResourceData[NID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(double* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(double)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(double);
        ResourceData[NID].VertexBuffers[a.Key] = vertexData;
    }
    public static unsafe void SetBufferData(uint NID, uint id, byte[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        var a = ResourceData[NID].VertexBuffers.ToArray()[id];
        VertexData vertexData = a.Value;

        gl.BindVertexArray(ResourceData[NID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(byte* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(byte)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(byte);
        ResourceData[NID].VertexBuffers[a.Key] = vertexData;
    }
    public static unsafe void SetBufferData(uint NID, uint id, int[] data, int size, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        var a = ResourceData[NID].VertexBuffers.ToArray()[id];
        VertexData vertexData = a.Value;

        gl.BindVertexArray(ResourceData[NID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexData.bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(int* buf = data)
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(int)), buf, currentUsage);

        vertexData.size = size;
        vertexData.type = typeof(int);
        ResourceData[NID].VertexBuffers[a.Key] = vertexData;
    }
    #endregion

    #region Operations with instances
    public static void SetBufferAtribDivisor(uint NID, string buffer, uint divisor)
    {
        var gl = Engine.gl;

        VertexData vertexData = ResourceData[NID].VertexBuffers[buffer];
        vertexData.divisions = divisor;
        ResourceData[NID].VertexBuffers[buffer] = vertexData;
    }
    public static void SetBufferAtribDivisor(uint NID, uint id, uint divisor)
    {
        var gl = Engine.gl;

        var a = ResourceData[NID].VertexBuffers.ToArray()[id];
        VertexData vertexData = a.Value;
        vertexData.divisions = divisor;
        ResourceData[NID].VertexBuffers[a.Key] = vertexData;
    }
    
    public static void EnableInstancing(uint NID, uint instanceCount)
    {
        var gl = Engine.gl;

        var res = ResourceData[NID];

        if (instanceCount > 0)
        {
            res.useInstancing = true;
            res.instanceCount = instanceCount;
        }
        else res.useInstancing = false;

        ResourceData[NID] = res;
    }
    #endregion

    public static unsafe void SetElementBufferData(uint NID, uint[] data, BufferUsage usage=BufferUsage.Static)
    {
        var gl = Engine.gl;

        uint bufferId = ResourceData[NID].ElementBuffer;

        gl.BindVertexArray(ResourceData[NID].VertexArray);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, bufferId);

        BufferUsageARB currentUsage = 
            usage == BufferUsage.Static? BufferUsageARB.StaticDraw :
            usage == BufferUsage.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StreamDraw;
        
        fixed(uint* buf = data)
        gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(data.Length * sizeof(uint)), buf, currentUsage);

        var a = ResourceData[NID];
        a.elementsLength = (uint) data.Length;
        ResourceData[NID] = a;
    }
    
    
    public static unsafe void EnableAtributes(uint NID, Material material)
    {
        var gl = Engine.gl;
        var res = ResourceData[NID];

        foreach (var i in res.VertexBuffers)
        {
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, i.Value.bufferId);
            int iloc = material.GetALocation(i.Key);
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

    public static unsafe void Draw(uint NID)
    {
        var res = ResourceData[NID];
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