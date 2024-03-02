using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace GameEngine.Util.Values;

public struct Vector2<T> where T : struct
{
    public T X { get; set; }
    public T Y { get; set; }

    public readonly double Magnitude {
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
    public Vector2(float[] data)
    {
        X = (T)Convert.ChangeType(data[0], typeof(T));
        Y = (T)Convert.ChangeType(data[1], typeof(T));
    }
    public Vector2(Vector2 numericsVector2)
    {
        X = (T)Convert.ChangeType(numericsVector2.X, typeof(T));
        Y = (T)Convert.ChangeType(numericsVector2.Y, typeof(T));
    }

    public Vector2<T> Normalized()
    {
        return new Vector2<T>(X, Y) / Magnitude;
    }

    public Vector2 GetAsNumerics()
    {
        return new Vector2((float)Convert.ToDouble(X), (float)Convert.ToDouble(Y));
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

    public static Vector2<T> operator - (Vector2<T> a)
    {
        return new (
            (T)Convert.ChangeType(-Convert.ToDouble(a.X), typeof(T)),
            (T)Convert.ChangeType(-Convert.ToDouble(a.Y), typeof(T))
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
    public static Vector2<T> operator * (Vector2<T> a, Matrix4x4 b)
    {return DoMatrixMultiplication(a, b);}
    
    public static Vector2<T> operator / (Vector2<T> a, int b)
    {return DoDivision(a, b);}
    public static Vector2<T> operator / (Vector2<T> a, float b)
    {return DoDivision(a, b);}
    public static Vector2<T> operator / (Vector2<T> a, double b)
    {return DoDivision(a, b);}
    public static Vector2<T> operator / (Vector2<T> a, Vector2<T> b)
    {return DoVectorDivision(a, b);}

    public static bool operator ==(Vector2<T> a, Vector2<T> b)
    {
        return Convert.ToDouble(a.X) == Convert.ToDouble(b.X)
            && Convert.ToDouble(a.Y) == Convert.ToDouble(b.Y);
    }
    public static bool operator !=(Vector2<T> a, Vector2<T> b)
    {
        return !(Convert.ToDouble(a.X) == Convert.ToDouble(b.X)
            && Convert.ToDouble(a.Y) == Convert.ToDouble(b.Y));
    }


    public static explicit operator Vector2<int>(Vector2<T> vec2)
    {
        return new(Convert.ToInt32(vec2.X), Convert.ToInt32(vec2.Y));
    }
    public static explicit operator Vector2<uint>(Vector2<T> vec2)
    {
        return new(Convert.ToUInt32(vec2.X), Convert.ToUInt32(vec2.Y));
    }
    public static explicit operator Vector2<float>(Vector2<T> vec2)
    {
        return new(Convert.ToSingle(vec2.X), Convert.ToSingle(vec2.Y));
    }
    public static explicit operator Vector2<double>(Vector2<T> vec2)
    {
        return new(Convert.ToDouble(vec2.X), Convert.ToDouble(vec2.Y));
    }
    public static explicit operator Vector2<byte>(Vector2<T> vec2)
    {
        return new(Convert.ToByte(vec2.X), Convert.ToByte(vec2.Y));
    }

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

    private static Vector2<T> DoMatrixMultiplication(Vector2<T> a, Matrix4x4 b)
    {
        Vector4 vec4 = new(
            (float) Convert.ToDouble(a.X),
            (float) Convert.ToDouble(a.Y),
            0.0f, 1.0f
        );
        var result = Vector4.Transform(vec4, b);
        return new(
            (T)Convert.ChangeType(result.X, typeof(T)),
            (T)Convert.ChangeType(result.Y, typeof(T))
        );
    }

    public override readonly string ToString()
    {
        return string.Format("v2({0}, {1})", X, Y);
    }

}