using GameEngine.Util;
using GameEngine.Util.Resources;
using Silk.NET.Core;

namespace GameEngine.Core;

public static class ResourceHeap
{

    private static List<GlShaderProgram> _GlShaderPrograms = new();
    public static GlShaderProgram? GetShaderProgramReference(FileReference vs, FileReference fs)
    {
        foreach (var i in _GlShaderPrograms)
        if (i.vertexShader == vs && i.fragmentShader == fs)
            return i;
        return null;
    }
    public static void AddShaderProgramReference(GlShaderProgram program)
    {
        _GlShaderPrograms.Add(program);
    }
    public static void RemoveShaderProgramReference(GlShaderProgram program)
    {
        _GlShaderPrograms.Remove(program);
        program.Dispose();
    }

}
