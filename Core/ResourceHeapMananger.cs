using GameEngine.Util.Resources;
using Silk.NET.OpenGL;

namespace GameEngine.Core;

public static class ResourceHeap
{

    private static byte _lastFullColect = 0;

    #region to delete
    private static List<uint> _texturesToDelete = [];
    private static List<uint> _shaderProgsToDelete = [];

    public enum DeleteTarget
    {
        Texture,
        ShaderProgram
    }
    public static void Delete(uint id, DeleteTarget target)
    {
        switch (target)
        {
            case DeleteTarget.Texture:
                _texturesToDelete.Add(id); break;
            
            case DeleteTarget.ShaderProgram:
                _shaderProgsToDelete.Add(id); break;

            default: return;
        }
    }
    #endregion

    #region shared resources management
    private static List<WeakReference<SharedResource>> _sharedResources = [];

    public static void AddReference(SharedResource resource)
    {
        _sharedResources.Add(new WeakReference<SharedResource>(resource));
    }
    public static T? TryGetReference<T>(params object?[] args) where T : SharedResource
    {
        
        for (int i = 0; i < _sharedResources.Count; i++)
        {
            var refRes = _sharedResources[i];

            if (refRes.TryGetTarget(out var result))
            {
                if (result is T t && result.AreEqualsTo(args))
                    return t;
            }
            else
            {
                _sharedResources.RemoveAt(i);
                i--;
            }
        }

        return null;
    }
    #endregion

    public static void Collect()
    {
        var gl = Engine.gl;

        if (_texturesToDelete.Count > 0)
        {
            gl.DeleteTextures((uint) _texturesToDelete.Count, _texturesToDelete.ToArray());
            _texturesToDelete.Clear();
        }
        if (_shaderProgsToDelete.Count > 0)
        {
            foreach (var i in _shaderProgsToDelete) gl.DeleteProgram( i );
            _shaderProgsToDelete.Clear();
        }
    
        // execute a full garbage collection after 100 frames
        if (_lastFullColect >= 100)
        {

            for (int i = 0; i < _sharedResources.Count; i++)
            if (!_sharedResources[i].TryGetTarget(out var _))
            {
                _sharedResources.RemoveAt(i);
                i--;
            }


            _lastFullColect = 0;
        }
        else _lastFullColect++;
    }
    public static void CallProcess()
    {

        Collect();
    }

}
