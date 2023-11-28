namespace GameEngine.Util.Values;

public struct Rect
{

    Vector2<float> Position = new();
    Vector2<float> Size = new();

    float X
    {
        get {return Position.X;}
        set {Position.X = value;}
    }
    float Y
    {
        get {return Position.Y;}
        set {Position.Y = value;}
    }
    float Width
    {
        get {return Size.X;}
        set {Size.X = value;}
    }
    float Height
    {
        get {return Size.Y;}
        set {Size.Y = value;}
    }


    public Rect()
    {
    }
}