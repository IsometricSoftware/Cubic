using System.Runtime.InteropServices;

namespace Cubic.Freetype;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_GlyphSlot
{
    public IntPtr Library;
    public FT_Face* Face;
    public FT_GlyphSlot* Next;
    public uint GlyphIndex;
    public FT_Generic Generic;

    public FT_Glyph_Metrics Metrics;
    public int LinearHoriAdvance;
    public int LinearVertAdvance;
    public FT_Vector Advance;

    public FT_Glyph_Format Format;

    public FT_Bitmap Bitmap;
    public int BitmapLeft;
    public int BitmapTop;

    public FT_Outline Outline;

    public uint NumSubglyphs;
    public IntPtr Subglyphs;

    public void* ControlData;
    public int ControlLength;

    public int LSBDelta;
    public int RSBDelta;

    public void* Other;

    public IntPtr Internal;
}