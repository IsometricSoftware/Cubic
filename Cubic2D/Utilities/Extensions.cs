using System.Drawing;
using System.Numerics;

namespace Cubic2D.Utilities;

public static class Extensions
{
    internal static Vector4 Normalize(this Color color)
    {
        return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }

    public static Vector2 ToVector2(this Size size)
    {
        return new Vector2(size.Width, size.Height);
    }
}