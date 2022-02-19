using System;
using System.Drawing;
using System.IO;
using Cubic2D.Scenes;
using Cubic2D.Windowing;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Cubic2D.Render;

public class Texture2D : IDisposable
{
    internal int Handle;

    public readonly Size Size;

    public Texture2D(string path)
    {
        using (Stream stream = File.OpenRead(path))
        {
            ImageResult result = ImageResult.FromStream(stream);

            Handle = CreateTexture(result.Width, result.Height, result.Data,
                result.Comp == ColorComponents.RedGreenBlueAlpha ? PixelFormat.Rgba : PixelFormat.Rgb);
            Size = new Size(result.Width, result.Height);
        }
        
        // Add this to the list of created resources the scene has so it can be disposed later.
        SceneManager.Active.CreatedResources.Add(this);
    }

    public Texture2D(int width, int height, byte[] data)
    {
        Handle = CreateTexture(width, height, data);
        Size = new Size(width, height);
        
        SceneManager.Active.CreatedResources.Add(this);
    }

    public Texture2D(int width, int height)
    {
        Handle = CreateTexture(width, height, null);
        Size = new Size(width, height);
        
        SceneManager.Active.CreatedResources.Add(this);
    }

    public void SetData(IntPtr data, uint x, uint y, uint width, uint height)
    {
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, (int) x, (int) y, (int) width, (int) height, PixelFormat.Rgba,
            PixelType.UnsignedByte, data);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    private static int CreateTexture(int width, int height, byte[] data, PixelFormat format = PixelFormat.Rgba)
    {
        int texture = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texture);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, format,
            PixelType.UnsignedByte, data);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        
        GL.BindTexture(TextureTarget.Texture2D, 0);

        return texture;
    }

    public void Dispose()
    {
        GL.DeleteTexture(Handle);
#if DEBUG
        Console.WriteLine("Texture disposed");
#endif
    }

    public static readonly Texture2D Blank = new Texture2D(1, 1, new byte[] {255, 255, 255, 255});
}