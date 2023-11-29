namespace GameEngine.Util.Resources;

public class Resource : IDisposable
{
    protected bool _disposed = false;

    public virtual void Dispose() {
        if (!_disposed)
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    ~Resource()
    {
        Dispose();
        // Alert of insecure dispose of the class
    }
}
