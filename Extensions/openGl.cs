using GameEngine.Util.Values;
using Silk.NET.OpenGL;

namespace GameEngine;

public static class OpenglCustomExtensionMethods
{
    public static void Viewport(this GL gl, Vector2<int> size)
    {
        gl.Viewport(0, 0, (uint) size.X, (uint) size.Y);
    }
    public static void Viewport(this GL gl, Vector2<uint> size)
    {
        gl.Viewport(0, 0, size.X, size.Y);
    }
    public static void Viewport(this GL gl, Rect rectangle)
    {
        gl.Viewport((int)rectangle.X, (int)rectangle.Y, (uint)rectangle.Width, (uint)rectangle.Height);
    }

    public static void Scissor(this GL gl, Rect rect)
    {
        gl.Scissor((int)rect.X, (int)rect.Y,(uint)rect.Width, (uint)rect.Height);
    }

    public static void UniformColor(this GL gl, int location, Color color)
    {
        gl.Uniform4(location, color.GetAsNumerics());
    }

}