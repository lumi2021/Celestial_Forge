using GameEngine.Util.Values;
using System.Numerics;

namespace GameEngine.Util;

public static class MathHelper
{

    public static float[] ToArray(this Matrix4x4 matrix)
    {
        float[] array =
        [
            matrix.M11,
            matrix.M12,
            matrix.M13,
            matrix.M14,
            matrix.M21,
            matrix.M22,
            matrix.M23,
            matrix.M24,
            matrix.M31,
            matrix.M32,
            matrix.M33,
            matrix.M34,
            matrix.M41,
            matrix.M42,
            matrix.M43,
            matrix.M44,
        ];
        return array;
    }

    public static Matrix4x4 Matrix4x4CreateRect<T>(Vector2<T> position, Vector2<T> size) where T : struct
    {
        float px = Convert.ToSingle(position.X); float py = Convert.ToSingle(position.Y);
        float sx = Convert.ToSingle(size.X); float sy = Convert.ToSingle(size.Y);

        return Matrix4x4.CreateScale(sx, sy, 1) *
            Matrix4x4.CreateTranslation(px, py, 0);
    }

}