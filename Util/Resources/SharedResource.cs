namespace GameEngine.Util.Resources;

public abstract class SharedResource : Resource
{

    protected SharedResource()
    {

    }

    public abstract bool AreEqualsTo(params object?[] args);

}