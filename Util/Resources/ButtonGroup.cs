namespace GameEngine.Util.Resources;

public class ButtonGroup : Resource
{
    public delegate void UnselectAllHandler();
    public event UnselectAllHandler? UnselectAll;

    public void InvokeUnselectAll() => UnselectAll?.Invoke();
}