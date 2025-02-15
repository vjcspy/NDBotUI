using System;
using System.IO;
using System.Runtime.CompilerServices;
using AdvancedSharpAdbClient.Models;
using SkiaSharp;

namespace NDBotUI.Modules.Core.Extensions;

/// <summary>
///     Provides extension methods of <see cref="SKBitmap" /> for the <see cref="Framebuffer" /> and
///     <see cref="FramebufferHeader" /> classes.
///     https://github.com/SharpAdb/AdvancedSharpAdbClient/wiki/Compatibility
/// </summary>
public static class FrameBufferSkiaExtensions
{
    /// <summary>
    ///     Converts the framebuffer data to a <see cref="SKBitmap" />.
    /// </summary>
    /// <param name="framebuffer">The framebuffer data.</param>
    /// <returns>A <see cref="SKBitmap" /> which represents the framebuffer data.</returns>
    public static SKBitmap? ToSKBitmap(this Framebuffer framebuffer)
    {
        framebuffer.EnsureNotDisposed();
        return framebuffer.Data == null
            ? throw new InvalidOperationException($"Call {nameof(framebuffer.Refresh)} first")
            : framebuffer.Header.ToSKBitmap(framebuffer.Data);
    }

    /// <summary>
    ///     Converts a <see cref="byte" /> array containing the raw frame buffer data to a <see cref="SKBitmap" />.
    /// </summary>
    /// <param name="header">The header containing the image metadata.</param>
    /// <param name="buffer">The buffer containing the image data.</param>
    /// <returns>
    ///     A <see cref="SKBitmap" /> that represents the image contained in the frame buffer, or <see langword="null" />
    ///     if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.
    /// </returns>
    public static SKBitmap? ToSKBitmap(this in FramebufferHeader header, byte[] buffer)
    {
        // Initial parameter validation.
        ArgumentNullException.ThrowIfNull(buffer);

        // This happens, for example, when DRM is enabled. In that scenario, no screenshot is taken on the device and an empty
        // framebuffer is returned; we'll just return null.
        if (header.Width == 0 || header.Height == 0 || header.Bpp == 0) return null;

        // The pixel format of the framebuffer may not be one that .NET recognizes, so we need to fix that
        var colorType = header.StandardizePixelFormat(ref buffer, out var alphaType);

        SKBitmap bitmap = new((int)header.Width, (int)header.Height, colorType, alphaType);

        var index = 0;
        for (var col = 0; col < bitmap.Height; col++)
        for (var row = 0; row < bitmap.Width; row++)
            bitmap.SetPixel(row, col, new SKColor(ReadUInt32(buffer)));

        return bitmap;

        uint ReadUInt32(byte[] data)
        {
            return (uint)(data[index++] | (data[index++] << 8) | (data[index++] << 16) | (data[index++] << 24));
        }
    }

    /// <summary>
    ///     Returns the <see cref="SKColorType" /> that describes pixel format of an image that is stored according to the
    ///     information
    ///     present in this <see cref="FramebufferHeader" />. Because the <see cref="SKColorType" /> enumeration does not allow
    ///     for all
    ///     formats supported by Android, this method also takes a <paramref name="buffer" /> and reorganizes the bytes in the
    ///     buffer to
    ///     match the return value of this function.
    /// </summary>
    /// <param name="header">The header containing the image metadata.</param>
    /// <param name="buffer">A byte array in which the images are stored according to this <see cref="FramebufferHeader" />.</param>
    /// <param name="alphaType">A <see cref="SKAlphaType" /> which describes how the alpha channel is stored.</param>
    /// <returns>
    ///     A <see cref="SKColorType" /> that describes how the image data is represented in this
    ///     <paramref name="buffer" />.
    /// </returns>
    private static SKColorType StandardizePixelFormat(this in FramebufferHeader header, ref byte[] buffer,
        out SKAlphaType alphaType)
    {
        // Initial parameter validation.
        ArgumentNullException.ThrowIfNull(buffer);

        if (buffer.Length < header.Width * header.Height * (header.Bpp / 8))
            throw new ArgumentOutOfRangeException(nameof(buffer),
                $"The buffer length {buffer.Length} is less than expected buffer " +
                $"length ({header.Width * header.Height * (header.Bpp / 8)}) for a picture of width {header.Width}, height {header.Height} and pixel depth {header.Bpp}");

        if (header.Width == 0 || header.Height == 0 || header.Bpp == 0)
            throw new InvalidOperationException("Cannot cannulate the pixel format of an empty framebuffer");

        // By far, the most common format is a 32-bit pixel format, which is either
        // RGB or RGBA, where each color has 1 byte.
        if (header.Bpp == 8 * 4)
        {
            // Require at least RGB to be present; and require them to be exactly one byte (8 bits) long.
            if (header.Red.Length != 8 || header.Blue.Length != 8 || header.Green.Length != 8)
                throw new ArgumentOutOfRangeException(
                    $"The pixel format with with RGB lengths of {header.Red.Length}:{header.Blue.Length}:{header.Green.Length} is not supported");

            // Alpha can be present or absent, but must be 8 bytes long
            alphaType = header.Alpha.Length switch
            {
                0 => SKAlphaType.Opaque,
                8 => SKAlphaType.Unpremul,
                _ => throw new ArgumentOutOfRangeException($"The alpha length {header.Alpha.Length} is not supported")
            };

            // Gets the index at which the red, bue, green and alpha values are stored.
            var redIndex = (int)header.Red.Offset / 8;
            var blueIndex = (int)header.Blue.Offset / 8;
            var greenIndex = (int)header.Green.Offset / 8;
            var alphaIndex = (int)header.Alpha.Offset / 8;

            var array = new byte[buffer.Length];
            // Loop over the array and re-order as required
            for (var i = 0; i < (int)header.Size; i += 4)
            {
                var red = buffer[i + redIndex];
                var blue = buffer[i + blueIndex];
                var green = buffer[i + greenIndex];
                var alpha = buffer[i + alphaIndex];

                // Convert to ARGB. Note, we're on a little endian system,
                // so it's really BGRA. Confusing!
                if (header.Alpha.Length == 8)
                {
                    array[i + 3] = alpha;
                    array[i + 2] = red;
                    array[i + 1] = green;
                    array[i + 0] = blue;
                }
                else
                {
                    array[i + 3] = 0xFF;
                    array[i + 2] = red;
                    array[i + 1] = green;
                    array[i + 0] = blue;
                }
            }

            buffer = array;

            // Returns RGB or RGBA, function of the presence of an alpha channel.
            return header.Alpha.Length == 0 ? SKColorType.Rgb888x : SKColorType.Rgba8888;
        }

        if (header.Bpp == 8 * 3)
        {
            // For 24-bit image depths, we only support RGB.
            if (header.Red.Offset == 0
                && header.Red.Length == 8
                && header.Green.Offset == 8
                && header.Green.Length == 8
                && header.Blue.Offset == 16
                && header.Blue.Length == 8
                && header.Alpha.Offset == 24
                && header.Alpha.Length == 0)
            {
                alphaType = SKAlphaType.Opaque;
                return SKColorType.Rgb888x;
            }
        }
        else if (header.Bpp == 5 + 6 + 5
                 && header.Red.Offset == 11
                 && header.Red.Length == 5
                 && header.Green.Offset == 5
                 && header.Green.Length == 6
                 && header.Blue.Offset == 0
                 && header.Blue.Length == 5
                 && header.Alpha.Offset == 0
                 && header.Alpha.Length == 0)
        {
            alphaType = SKAlphaType.Opaque;
            // For 16-bit image depths, we only support Rgb565.
            return SKColorType.Rgb565;
        }

        // If not caught by any of the statements before, the format is not supported.
        throw new NotSupportedException($"Pixel depths of {header.Bpp} are not supported");
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(EnsureNotDisposed))]
    private static extern void EnsureNotDisposed(this Framebuffer framebuffer);

    public static void SaveAsJpeg(this SKBitmap bitmap, string filePath)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        File.WriteAllBytes(filePath, data.ToArray());
    }

    public static void SaveAsPng(this SKBitmap bitmap, string filePath)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(filePath, data.ToArray());
    }
}