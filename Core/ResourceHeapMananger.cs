using GameEngine.Util;
using GameEngine.Util.Resources;

namespace GameEngine.Core;

public static class ResourceHeap
{

    #region shader programs
    private static List<GlShaderProgram> _GlShaderPrograms = new();
    public static GlShaderProgram? GetShaderProgramReference(FileReference vs, FileReference fs, FileReference? gs)
    {
        foreach (var i in _GlShaderPrograms)
        if (i.vertexShader == vs && i.fragmentShader == fs && i.geometryShader == gs)
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
    #endregion

}
