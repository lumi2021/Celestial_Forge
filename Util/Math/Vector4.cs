using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace GameEngine.Util.Values;

public partial struct Vector4<T> where T : struct
{

    public T X { get; set; }
    public T Y { get; set; }
    public T Z { get; set; }
    public T W { get; set; }

    public readonly double Magnitude {
        get {
            double xDouble = Convert.ToDouble(X);
            double yDouble = Convert.ToDouble(Y);
            double zDouble = Convert.ToDouble(Z);
            double wDouble = Convert.ToDouble(W);

            double magnitudeSquared = xDouble * xDouble + yDouble * yDouble
                                    + zDouble * zDouble + wDouble * wDouble;

            return Math.Sqrt(magnitudeSquared);
        }
    }

    public Vector4() {}
    public Vector4(T x, T y, T z, T w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }
    public Vector4(float[] data)
    {
        X = (T)Convert.ChangeType(data[0], typeof(T));
        Y = (T)Convert.ChangeType(data[1], typeof(T));
        Z = (T)Convert.ChangeType(data[2], typeof(T));
        W = (T)Convert.ChangeType(data[3], typeof(T));
    }
    public Vector4(Vector4 numericsVector4)
    {
        X = (T)Convert.ChangeType(numericsVector4.X, typeof(T));
        Y = (T)Convert.ChangeType(numericsVector4.Y, typeof(T));
        Z = (T)Convert.ChangeType(numericsVector4.Z, typeof(T));
        W = (T)Convert.ChangeType(numericsVector4.W, typeof(T));
    }

    public readonly Vector4<float> Normalized()
    {
        var m = Magnitude;
        return new Vector4<float>(
            Convert.ToSingle(X) / (float)m,
            Convert.ToSingle(Y) / (float)m,
            Convert.ToSingle(Z) / (float)m,
            Convert.ToSingle(W) / (float)m
        );
    }

    public readonly Vector4 GetAsNumerics()
    {
        return new Vector4(Convert.ToSingle(X), Convert.ToSingle(Y),
                           Convert.ToSingle(Z), Convert.ToSingle(W));
    }
    public readonly Silk.NET.Maths.Vector4D<float> GetAsSilkFloat()
    {
        return new Silk.NET.Maths.Vector4D<float>(
            Convert.ToSingle(X),
            Convert.ToSingle(Y),
            Convert.ToSingle(Z),
            Convert.ToSingle(W)
            );
    }
    public readonly Silk.NET.Maths.Vector4D<double> GetAsSilkDouble()
    {
        return new Silk.NET.Maths.Vector4D<double>(
            Convert.ToDouble(X),
            Convert.ToDouble(Y),
            Convert.ToDouble(Z),
            Convert.ToDouble(W)
            );
    }
    public readonly Silk.NET.Maths.Vector4D<int> GetAsSilkInt()
    {
        return new Silk.NET.Maths.Vector4D<int>(
            Convert.ToInt32(X),
            Convert.ToInt32(Y),
            Convert.ToInt32(Z),
            Convert.ToInt32(W)
            );
    }

    public static bool operator == (Vector4<T> a, object? b) => a.Equals(b);
    public static bool operator != (Vector4<T> a, object? b) => !a.Equals(b);

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        var objt = obj?.GetType() ?? typeof(object);
        if (obj != null && objt.IsGenericType && objt.GetGenericTypeDefinition() == typeof(Vector4<>))
        {
            var vec4 = (Vector4<double>)obj;

            return Convert.ToDouble(X) == vec4.X &&   
                   Convert.ToDouble(Y) == vec4.Y &&   
                   Convert.ToDouble(Z) == vec4.Z &&   
                   Convert.ToDouble(W) == vec4.W;
        }

        else return false;
    }
    public override readonly int GetHashCode() => base.GetHashCode();

    public static explicit operator Vector4<double>(Vector4<T> vec4)
    {
        return new(
            Convert.ToDouble(vec4.X),
            Convert.ToDouble(vec4.Y),
            Convert.ToDouble(vec4.Z),
            Convert.ToDouble(vec4.W)
        );
    }


    public override readonly string ToString()
    {
        return string.Format("v4({0}, {1}, {2}, {3})", X, Y, Z, W);
    }

}