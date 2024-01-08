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
        set {alpha = MathF.Min(MathF.Max(0, value), 1);}
    }

    public Color() { alpha = 1.0f; }
    public Color(float[] data)
    {
        R = (int) data[0];
        G = (int) data[1];
        B = (int) data[2];

        if (data.Length > 3) A = data[3];
        else A = 1f;
    }

    public Color(int red, int green, int blue)
    {
        R = red;
        G = green;
        B = blue;
        A = 1f;
    }
    public Color(float red, float green, float blue)
    {
        NormalR = red;
        NormalG = green;
        NormalB = blue;
        A = 1f;
    }
    public Color(int red, int green, int blue, float alpha)
    {
        R = red;
        G = green;
        B = blue;
        A = alpha;
    }
    public Color(float red, float green, float blue, float alpha)
    {
        NormalR = red;
        NormalG = green;
        NormalB = blue;
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

    public override readonly string ToString()
    {
        return string.Format("Col(R {0}, G {1}, B {2}, A {3})", red, green, blue, alpha);
    }

    public float[] ToArray()
    {
        return new float[] {NormalR, NormalG, NormalB, A};
    }
}