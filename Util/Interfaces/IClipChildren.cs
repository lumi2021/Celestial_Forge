using GameEngine.Util.Values;

namespace GameEngine.Util.Interfaces;

public interface IClipChildren
{
    public bool ClipChildren { get; set; }
    public Rect GetClippingArea();

}