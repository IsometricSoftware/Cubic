using System;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cubic.Mathematics;

public static class MathUtil
{
    /// <summary>
    /// Represents the ratio of the circumference of a circle to its diameter, specified by the constant, π.
    /// </summary>
    public const float Pi = MathF.PI;

    /// <summary>
    /// Converts a degree value to a radian value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value.</returns>
    public static float DegreesToRadians(float value)
    {
        return value * (Pi / 180.0f);
    }

    /// <summary>
    /// Converts a radian value to a degree value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value.</returns>
    public static float RadiansToDegrees(float value)
    {
        return value * (180.0f / Pi);
    }

    /// <summary>
    /// Linearly interpolates between two values by a given amount.
    /// </summary>
    /// <param name="from">The start value.</param>
    /// <param name="to">The end value.</param>
    /// <param name="amount">Interpolation amount.</param>
    /// <returns>The interpolated result between the two values.</returns>
    public static float Lerp(float from, float to, float amount)
    {
        return (1 - amount) * from + amount * to;
    }
}

