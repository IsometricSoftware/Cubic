using System;
using System.Drawing;

namespace Cubic.Utilities;

public static class Utils
{
    public static Color ColorFromHex(int hexValue)
    {
        return Color.FromArgb(hexValue >> 16, (hexValue >> 8) & 0xFF, hexValue & 0xFF);
    }

    // thanks https://www.rapidtables.com/convert/color/hsv-to-rgb.html for the math
    public static Color ColorFromHsv(float hue, float saturation = 1, float value = 1)
    {
        float c = value * saturation;
        float x = c * (1 - MathF.Abs((hue / 60f) % 2 - 1));
        float m = value - c;

        float dR, dG, dB;
        switch (hue)
        {
            case >= 0 and < 60:
                dR = c;
                dG = x;
                dB = 0;
                break;
            case >= 60 and < 120:
                dR = x;
                dG = c;
                dB = 0;
                break;
            case >= 120 and < 180:
                dR = 0;
                dG = c;
                dB = x;
                break;
            case >= 180 and < 240:
                dR = 0;
                dG = x;
                dB = c;
                break;
            case >= 240 and < 300:
                dR = x;
                dG = 0;
                dB = c;
                break;
            case >= 300 and < 360:
                dR = c;
                dG = 0;
                dB = x;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(hue), hue, "Hue must be between 0 and 360.");
        }

        float r = (dR + m) * 255;
        float g = (dG + m) * 255;
        float b = (dB + m) * 255;
        
        return Color.FromArgb(255, (int) r, (int) g, (int) b);
    }
}