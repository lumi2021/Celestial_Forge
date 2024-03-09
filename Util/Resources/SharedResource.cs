using GameEngine.Core;

namespace GameEngine.Util.Resources;

public abstract class SharedResource : Resource
{

    protected SharedResource() => ResourceHeap.AddReference(this);
    
    public abstract bool AreEqualsTo(params object?[] args);

}