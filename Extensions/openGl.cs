using GameEngine.Util.Values;
using Silk.NET.OpenGL;

namespace GameEngine;

public static class OpenglCustomExtensionMethods
{

    public static void Scissor(this GL gl, Rect rect)
    {
        gl.Scissor((int)rect.X, (int)rect.Y,(uint)rect.Width, (uint)rect.Height);
    }

    public static void UniformColor(this GL gl, int location, Color color)
    {
        gl.Uniform4(location, color.GetAsNumerics());
    }

}