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

    public Rect(Vector2<int> positionPixels) {}
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

    public bool Intersects(Vector2<int> vec2)
    {
        return X <= vec2.X && X+Width >= vec2.X && Y <= vec2.Y && Y+Height >= vec2.Y;
    }
    public bool Intersects(Vector2<float> vec2)
    {
        return X <= vec2.X && X+Width >= vec2.X && Y <= vec2.Y && Y+Height >= vec2.Y;
    }

    public Rect InvertVerticallyIn(Rect rect)
    {
        float invertedX = X;
        float invertedY = rect.Height - Y - Height;

        return new(invertedX, invertedY, Width, Height);
    }
    public Rect InvertHorizontallyIn(Rect rect)
    {
        float invertedX = rect.Width - X - Width;
        float invertedY = Y;

        return new(invertedX, invertedY, Width, Height);
    }

    public readonly Rect FitInside(Rect rect)
    {
        return FitInside(this, rect);
    }

    public static Rect operator + (Rect a, Vector2<int> b) { return AddRectVector2(a, b); }
    public static Rect operator + (Rect a, Vector2<float> b) { return AddRectVector2(a, b); }
    public static Rect operator + (Rect a, Vector2<double> b) { return AddRectVector2(a, b); }

    public static Rect FitInside(Rect rectA, Rect rectB)
    {
        var posDif = rectB.Position - rectA.Position;

        Rect baseRect = new(
            MathF.Min(rectA.X, rectB.X),
            MathF.Min(rectA.Y, rectB.Y),
            MathF.Max(rectA.Width, posDif.X + rectB.Width),
            MathF.Max(rectA.Height, posDif.Y + rectB.Height)
        );

        return baseRect;
    }

    private static Rect AddRectVector2<T>(Rect a, Vector2<T> b) where T : struct
    {
        var nRect = a;

        Vector2<double> v2 = new(Convert.ToDouble(b.X), Convert.ToDouble(b.Y));
        nRect.Position += v2;

        return nRect;
    }

    public override string ToString()
    {
        return string.Format("Rect(X {0}, Y {1}, W {2}, H {3})", X, Y, Width, Height);
    }

}