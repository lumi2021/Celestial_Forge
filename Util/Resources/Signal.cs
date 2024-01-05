namespace GameEngine.Util.Resources;

public class Signal : Resource
{
    private event SignalHandler? SignalEvent;

    public void Emit(object from, dynamic[] args)
    {
        SignalEvent?.Invoke(from, args);
    }
    public void Emit(dynamic[] args)
    {
        SignalEvent?.Invoke(null, args);
    }

    public void Connect(Action<object?, dynamic[]> callable)
    {
        SignalEvent += new SignalHandler(callable);
    }
    public void Disconnect(Action<object?, dynamic[]> callable)
    {
        SignalEvent -= new SignalHandler(callable);
    }

    public delegate void SignalHandler(object? sender, dynamic[] args);
}
