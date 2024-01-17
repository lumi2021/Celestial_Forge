using GameEngine.Util.Values;
using System.Numerics;

namespace GameEngine.Util;

public static class MathHelper
{

    public static float[] ToArray(this Matrix4x4 matrix)
    {
        float[] array = new float[16];

        array[0] = matrix.M11;
        array[1] = matrix.M12;
        array[2] = matrix.M13;
        array[3] = matrix.M14;
        array[4] = matrix.M21;
        array[5] = matrix.M22;
        array[6] = matrix.M23;
        array[7] = matrix.M24;
        array[8] = matrix.M31;
        array[9] = matrix.M32;
        array[10] = matrix.M33;
        array[11] = matrix.M34;
        array[12] = matrix.M41;
        array[13] = matrix.M42;
        array[14] = matrix.M43;
        array[15] = matrix.M44;

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