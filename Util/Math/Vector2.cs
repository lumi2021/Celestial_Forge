namespace GameEngine.Util.Values;

public struct Vector2<T> where T : struct
{
    public T X { get; set; }
    public T Y { get; set; }

    public double Magnitude {
        get {
            double xDouble = Convert.ToDouble(X);
            double yDouble = Convert.ToDouble(Y);

            double magnitudeSquared = xDouble * xDouble + yDouble * yDouble;

            return Math.Sqrt(magnitudeSquared);
        }
    }

    public Vector2() {}
    public Vector2(T x, T y)
    {
        X = x;
        Y = y;
    }
    public Vector2(System.Numerics.Vector2 numericsVector2)
    {
        X = (T)Convert.ChangeType(numericsVector2.X, typeof(T));
        Y = (T)Convert.ChangeType(numericsVector2.Y, typeof(T));
    }

    public Vector2<T> Normalized()
    {
        return new Vector2<T>(X, Y) / Magnitude;
    }

    public System.Numerics.Vector2 GetAsNumerics()
    {
        return new System.Numerics.Vector2((float)Convert.ToDouble(X), (float)Convert.ToDouble(Y));
    }
    public Silk.NET.Maths.Vector2D<float> GetAsSilkFloat()
    {
        return new Silk.NET.Maths.Vector2D<float>(
            (float)Convert.ToDouble(X),
            (float)Convert.ToDouble(Y)
            );
    }
    public Silk.NET.Maths.Vector2D<double> GetAsSilkDouble()
    {
        return new Silk.NET.Maths.Vector2D<double>(
            Convert.ToDouble(X),
            Convert.ToDouble(Y)
            );
    }
    public Silk.NET.Maths.Vector2D<int> GetAsSilkInt()
    {
        return new Silk.NET.Maths.Vector2D<int>(
            (int)Convert.ToDouble(X),
            (int)Convert.ToDouble(Y)
            );
    }

    public static Vector2<T> operator + (Vector2<T> a, Vector2<int> b)
    {
        return DoAddition(
        new Vector2<double>(Convert.ToDouble(a.X), Convert.ToDouble(a.Y)),
        new Vector2<double>(Convert.ToDouble(b.X), Convert.ToDouble(b.Y))
        );
    }
    public static Vector2<T> operator + (Vector2<T> a, Vector2<float> b)
    {
        return DoAddition(
        new Vector2<double>(Convert.ToDouble(a.X), Convert.ToDouble(a.Y)),
        new Vector2<double>(Convert.ToDouble(b.X), Convert.ToDouble(b.Y))
        );
    }
    public static Vector2<T> operator + (Vector2<T> a, Vector2<double> b)
    {
        return DoAddition(
        new Vector2<double>(Convert.ToDouble(a.X), Convert.ToDouble(a.Y)),
        new Vector2<double>(Convert.ToDouble(b.X), Convert.ToDouble(b.Y))
        );
    }
    
    public static Vector2<T> operator - (Vector2<T> a, Vector2<int> b)
    {
        return DoSubtraction(
        new Vector2<double>(Convert.ToDouble(a.X), Convert.ToDouble(a.Y)),
        new Vector2<double>(Convert.ToDouble(b.X), Convert.ToDouble(b.Y))
        );
    }
    public static Vector2<T> operator - (Vector2<T> a, Vector2<float> b)
    {
        return DoSubtraction(
        new Vector2<double>(Convert.ToDouble(a.X), Convert.ToDouble(a.Y)),
        new Vector2<double>(Convert.ToDouble(b.X), Convert.ToDouble(b.Y))
        );
    }
    public static Vector2<T> operator - (Vector2<T> a, Vector2<double> b)
    {
        return DoSubtraction(
        new Vector2<double>(Convert.ToDouble(a.X), Convert.ToDouble(a.Y)),
        new Vector2<double>(Convert.ToDouble(b.X), Convert.ToDouble(b.Y))
        );
    }
    
    public static Vector2<T> operator * (Vector2<T> a, int b)
    {return DoMultiplication(a, b);}
    public static Vector2<T> operator * (Vector2<T> a, float b)
    {return DoMultiplication(a, b);}
    public static Vector2<T> operator * (Vector2<T> a, double b)
    {return DoMultiplication(a, b);}
    public static Vector2<T> operator * (Vector2<T> a, Vector2<T> b)
    {return DoVectorMultiplication(a, b);}
    
    public static Vector2<T> operator / (Vector2<T> a, int b)
    {return DoDivision(a, b);}
    public static Vector2<T> operator / (Vector2<T> a, float b)
    {return DoDivision(a, b);}
    public static Vector2<T> operator / (Vector2<T> a, double b)
    {return DoDivision(a, b);}
    public static Vector2<T> operator / (Vector2<T> a, Vector2<T> b)
    {return DoVectorDivision(a, b);}

    private static Vector2<T> DoAddition(Vector2<double> a, Vector2<double> b)
    {
        return new Vector2<T>(
            (T)Convert.ChangeType(a.X + b.X, typeof(T)),
            (T)Convert.ChangeType(a.Y + b.Y, typeof(T))
        );
    }
    private static Vector2<T> DoSubtraction(Vector2<double> a, Vector2<double> b)
    {
        return new Vector2<T>(
            (T)Convert.ChangeType(a.X - b.X, typeof(T)),
            (T)Convert.ChangeType(a.Y - b.Y, typeof(T))
        );
    }
    private static Vector2<T> DoMultiplication(Vector2<T> a, double b)
    {
        double x = Convert.ToDouble(a.X);
        double y = Convert.ToDouble(a.Y);

        return new Vector2<T>(
            (T)Convert.ChangeType(x*b, typeof(T)),
            (T)Convert.ChangeType(y*b, typeof(T))
        );
    }
    private static Vector2<T> DoVectorMultiplication(Vector2<T> a, Vector2<T> b)
    {
        double x1 = Convert.ToDouble(a.X);
        double y1 = Convert.ToDouble(a.Y);
        double x2 = Convert.ToDouble(b.X);
        double y2 = Convert.ToDouble(b.Y);

        return new Vector2<T>(
            (T)Convert.ChangeType(x1*x2, typeof(T)),
            (T)Convert.ChangeType(y1*y2, typeof(T))
        );
    }
    private static Vector2<T> DoDivision(Vector2<T> a, double b)
    {
        double x = Convert.ToDouble(a.X);
        double y = Convert.ToDouble(a.Y);

        return new Vector2<T>(
            (T)Convert.ChangeType(x/b, typeof(T)),
            (T)Convert.ChangeType(y/b, typeof(T))
        );
    }
    private static Vector2<T> DoVectorDivision(Vector2<T> a, Vector2<T> b)
    {
        double x1 = Convert.ToDouble(a.X);
        double y1 = Convert.ToDouble(a.Y);
        double x2 = Convert.ToDouble(b.X);
        double y2 = Convert.ToDouble(b.Y);

        return new Vector2<T>(
            (T)Convert.ChangeType(x1/x2, typeof(T)),
            (T)Convert.ChangeType(y1/y2, typeof(T))
        );
    }

    public override string ToString()
    {
        return string.Format("v2({0}, {1})", X, Y);
    }

}