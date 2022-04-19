using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace Cubic.Render;

public class RenderTarget : Texture
{
    internal int Fbo;

    public RenderTarget(Size size, bool autoDispose) : base(autoDispose)
    {
        Fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);

        Handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.Width, size.Height, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, IntPtr.Zero);
        GL.BindTexture(TextureTarget.Texture2D, 0);

        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, Handle, 0);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        Size = size;
    }
    
    internal override void Bind(TextureUnit textureUnit = TextureUnit.Texture0)
    {
        GL.ActiveTexture(textureUnit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }
    
    internal override void Unbind()
    {
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public override void Dispose()
    {
        GL.DeleteFramebuffer(Fbo);
        
        base.Dispose();
    }
}