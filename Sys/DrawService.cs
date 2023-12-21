using System.Runtime.InteropServices;
using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;
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

    public static void CreateCanvasItem(uint RID)
    {
        ResourceData.Add(RID, new ResourceDrawData());
    }

    #region Operations with buffers

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
        ResourceData[RID].VertexBuffers[a.Key] = vertexData;
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
            uint loc = (uint) material.ALocation(i.Key);

            gl.EnableVertexAttribArray(loc);
            gl.VertexAttribPointer(loc, i.Value.size, VertexAttribPointerType.Float, false, (uint)(i.Value.size*Marshal.SizeOf(i.Value.type)), (void*) 0);
        }
    }
    
    #endregion

    public static unsafe void Draw(uint RID)
    {
        var res = ResourceData[RID];
        Engine.gl.BindVertexArray(res.VertexArray);

        Engine.gl.DrawElements(PrimitiveType.Triangles, res.elementsLength, DrawElementsType.UnsignedInt, (void*) 0);
    }

}

struct ResourceDrawData
{
    // opengl info
    public uint VertexArray = 0;
    public Dictionary<string, VertexData> VertexBuffers = new();
    public uint ElementBuffer = 0;
    public uint elementsLength = 0;

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

    public VertexData() {}
}