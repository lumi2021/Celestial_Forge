namespace GameEngine.Util.Resources;

public class Resource
{
    private bool _disposed = false;

    
    public virtual void Dispose() {
        _disposed = true;
    }

    ~Resource()
    {
        Dispose();
        if (_disposed)
        {
            // Alert of insecure dispose of the class
        }
    }
}
