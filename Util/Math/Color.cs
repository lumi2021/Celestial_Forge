namespace GameEngine.Util.Values;

public struct Color
{

    private int red = 0;
    private int green = 0;
    private int blue = 0;
    private float alpha = 0;

    public int R
    {
        get {return red;}
        set {red = value;}
    }
    public int G
    {
        get {return green;}
        set {green = value;}
    }
    public int B
    {
        get {return blue;}
        set {blue = value;}
    }

    public float NormalR
    {
        get {return 1f/255f * red;}
        set {red = (int)(value*255);}
    }
    public float NormalG
    {
        get {return 1f/255f * green;}
        set {green = (int)(value*255);}
    }
    public float NormalB
    {
        get {return 1f/255f * blue;}
        set {blue = (int)(value*255);}
    }

    public float A
    {
        get {return alpha;}
        set {alpha = MathF.Min(MathF.Max(0, alpha), 1);}
    }

    public Color() {}
    public Color(int red, int green, int blue)
    {
        R = red;
        G = green;
        B = blue;
        A = 1;
    }
    public Color(int red, int green, int blue, float alpha)
    {
        R = red;
        G = green;
        B = blue;
        A = alpha;
    }

    public System.Numerics.Vector4 GetAsNumerics()
    {
        return new System.Numerics.Vector4(NormalR, NormalG, NormalB, A);
    }

    public static Color operator + (Color a, Color b)
    {
        return new Color(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
    }
    public static Color operator - (Color a, Color b)
    {
        return new Color(a.R - b.R, a.G - b.G, a.B - b.B, a.A - b.A);
    }
    public static Color operator * (Color a, Color b)
    {
        return new Color(a.R * b.R, a.G * b.G, a.B * b.B, a.A * b.A);
    }
    public static Color operator / (Color a, Color b)
    {
        return new Color(a.R / b.R, a.G / b.G, a.B / b.B, a.A / b.A);
    }
    
}