namespace GameEngine.Util.Values;

public struct Rect
{

    public Vector2<float> Position = new();
    public Vector2<float> Size = new();

    public float X
    {
        get {return Position.X;}
        set {Position.X = value;}
    }
    public float Y
    {
        get {return Position.Y;}
        set {Position.Y = value;}
    }
    public float Width
    {
        get {return Size.X;}
        set {Size.X = value;}
    }
    public float Height
    {
        get {return Size.Y;}
        set {Size.Y = value;}
    }

    public Rect() {}
    public Rect(Vector2<float> position, Vector2<float> size)
    {
        Position = position;
        Size = size;
    }
    public Rect(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    public Rect(float[] data)
    {
        Position.X = data[0];
        Position.Y = data[1];
        Size.Y = data[2];
        Size.Y = data[3];
    }

    public Rect Intersection(Rect anotherRect)
    {
        var nRect = new Rect()
        {
            X = MathF.Max(X, anotherRect.X),
            Y = MathF.Max(Y, anotherRect.Y),
            Width = MathF.Min(Width, anotherRect.Width),
            Height = MathF.Min(Height, anotherRect.Height)
        };

        return nRect;
    }


    public static Rect operator + (Rect a, Vector2<int> b) { return AddRectVector2(a, b); }
    public static Rect operator + (Rect a, Vector2<float> b) { return AddRectVector2(a, b); }
    public static Rect operator + (Rect a, Vector2<double> b) { return AddRectVector2(a, b); }

    private static Rect AddRectVector2<T>(Rect a, Vector2<T> b) where T : struct
    {
        var nRect = a;

        Vector2<double> v2 = new(Convert.ToDouble(b.X), Convert.ToDouble(b.Y));
        nRect.Position += v2;

        return nRect;
    }

}