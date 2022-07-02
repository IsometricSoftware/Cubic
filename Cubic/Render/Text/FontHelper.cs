using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using Cubic.Freetype;
using static Cubic.Freetype.FreeType;

namespace Cubic.Render.Text;

/// <summary>
/// Represents helper methods for font drawing.
/// </summary>
public static class FontHelper
{
    #region Internal bits

    internal static readonly IntPtr FreeType;

    internal static readonly Dictionary<string, Color> ColorNames;

    static unsafe FontHelper()
    {
        IntPtr ft;
        FT_Init_FreeType(&ft);
        FreeType = ft;

        ColorNames = new Dictionary<string, Color>();
        PropertyInfo[] colorProperties = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);
        foreach (PropertyInfo prop in colorProperties)
            ColorNames.Add(prop.Name.ToLower(), (Color) prop.GetValue(null, null));
    }

    internal static unsafe Dictionary<char, Character> CreateFontTexture(Texture2D texture, FontFace face,
        uint fontSize, uint asciiRangeStart, uint asciiRangeEnd)
    {
        Dictionary<char, Character> characters = new Dictionary<char, Character>();
        FT_Set_Pixel_Sizes(face.Face, 0, fontSize);

        uint offsetX = 0;
        uint offsetY = 0;

        // First we run through and gather metrics on each character, generating a texture for the first 128 ASCII
        // characters
        for (uint c = asciiRangeStart; c < asciiRangeEnd; c++)
        {
            FT_Load_Char(face.Face, c, FTLoad.Render);

            // Calculate our character's offset based on the last glyph's size.
            // Here, we wrap the glyph to the next "line" of the texture if it overflows the texture's size.
            FT_Bitmap glyph = face.Face->Glyph->Bitmap;
            if (offsetX + glyph.Width >= texture.Size.Width)
            {
                offsetY += fontSize;
                offsetX = 0;
            }

            // Ooh boy, pointer garbage!
            // FreeType provides us a font in grayscale, in this case typically the "red" channel if you will.
            // Problem is we can't really draw this. So we need to convert it into RGBA colour space.
            // This garbage does this.
            // First calculate the grayscale size of the glyph. Easy enough, just the width x height (fontsize),
            // multiplied by 8 (as each char is 8 bits).
            uint size = glyph.Width * glyph.Rows * 8;
            int offset = 0;
            // We need to create an array that is 4x this size, as we need 4 channels while the current data only has
            // one channel.
            byte[] data = new byte[size * 4];
            // Convert the glyph's buffer to a byte ptr.
            byte* buf = glyph.Buffer;
            // We repeat this 4 times for the 4 different channels.
            for (int n = 0; n < 4; n++)
            {
                // currOffset stores which byte of the glyph we want.
                int currOffset = 0;
                // Set the R, G, and B channels of the glyph to 255 each, as they just need to be white.
                // The alpha channel is set to the value of the native glyph, producing the result we want.
                for (int i = 0; i < glyph.Width * glyph.Rows; i++)
                {
                    data[offset++] = 255;
                    data[offset++] = 255;
                    data[offset++] = 255;
                    data[offset++] = buf[currOffset++];
                }
            }
            
            texture.SetData(data, (int) offsetX, (int) offsetY, (int) glyph.Width, (int) glyph.Rows);

            // Load each character into the character dictionary so it can be referenced later.
            Character chr = new Character()
            {
                Position = new Point((int) offsetX, (int) offsetY),
                Size = new Size((int) glyph.Width, (int) glyph.Rows),
                Bearing = new Point(face.Face->Glyph->BitmapLeft, face.Face->Glyph->BitmapTop),
                Advance = face.Face->Glyph->Advance.X
            };
            characters.Add((char) c, chr);

            offsetX += glyph.Width;
        }

        return characters;
    }

    internal static Color GetColor(string name)
    {
        try
        {
            return ColorNames[name];
        }
        catch (KeyNotFoundException)
        {
            throw new CubicException($"The given colour is not a valid colour (Color name: '{name}')");
        }
    }

    // Interpret string for any potential params, such as unicode, new line delimeters, and colour arguments.
    internal static StringParam CheckParams(ref char c, ref int i, string text, bool ignoreParams)
    {
        StringParam param = new StringParam()
        {
            Type = ParamType.None
        };
        
        switch (c)
        {
            // If newline argument is given, actually create a new line in our text drawing.
            case '\n':
                param.Type = ParamType.NewLine;
                break;
            case '\\':
                if (text[i + 1] == '[')
                    param.Type = ParamType.EscapeChar;
                break;
            // '[' characters potentially represents commands the renderer can follow.
            case '[':
                if (ignoreParams)
                    break;
                // Backslashes are ignored.
                if (i - 1 > -1 && text[i - 1] == '\\')
                    break;
                string potentialParam = "";
                int texPtr = i;
                while (text[++texPtr] != ']')
                    potentialParam += text[texPtr];
                int len = potentialParam.Length + 1;
                potentialParam = potentialParam.Replace(" ", "").ToLower();
                if (potentialParam.StartsWith('u'))
                {
                    i += len;
                    c = (char) int.Parse(potentialParam[1..], NumberStyles.HexNumber);
                    break;
                }
                if (potentialParam[1] != '=')
                    break;
                switch (potentialParam[0])
                {
                    case 'c':
                        param.Type = ParamType.Color;
                        param.Color = GetColor(potentialParam[2..]);
                        i += len;
                        break;
                }

                break;
        }

        return param;
    }

    internal enum ParamType
    {
        None,
        NewLine,
        EscapeChar,
        Color,
    }

    internal ref struct StringParam
    {
        public ParamType Type;

        public Color Color;
    }

    internal struct Character
    {
        public Point Position;
        public Size Size;
        public Point Bearing;
        public int Advance;
    }
    
    #endregion

    #region Public bits

    /// <summary>
    /// Create a custom colour that can be used in text drawing. You can either create a new colour entirely, or
    /// overwrite an already existing colour. NOTE: This is a GLOBAL change. It's not recommended that you overwrite an
    /// existing colour.
    /// </summary>
    /// <param name="colorName">The name of the colour.</param>
    /// <param name="color">The colour itself.</param>
    public static void SetCustomColor(string colorName, Color color)
    {
        if (ColorNames.ContainsKey(colorName.ToLower()))
            ColorNames[colorName.ToLower()] = color;
        else
            ColorNames.Add(colorName.ToLower(), color);
    }

    #endregion
}