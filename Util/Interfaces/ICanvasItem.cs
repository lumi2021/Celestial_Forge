namespace GameEngine.Util.Interfaces;

public interface ICanvasItem
{

    public bool Visible { get; set; }
    public void Show();
    public void Hide();

    public int ZIndex { get; set; }
    public int GlobalZIndex { get; }

}
