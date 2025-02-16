// <copyright file="FrameBufferSkiaExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Runtime.CompilerServices;
using AdvancedSharpAdbClient.Models;
using SkiaSharp;

/// <summary>
///     Provides extension methods of <see cref="SKBitmap" /> for the <see cref="Framebuffer" /> and
///     <see cref="FramebufferHeader" /> classes.
/// </summary>
public static class FrameBufferSkiaExtensions
{
    public static SKBitmap? ToSKBitmap(this Framebuffer framebuffer)
    {
        framebuffer.EnsureNotDisposed();
        return framebuffer.Data == null
            ? throw new InvalidOperationException($"Call {nameof(framebuffer.Refresh)} first")
            : framebuffer.Header.ToSKBitmap(framebuffer.Data);
    }

    public static SKBitmap? ToSKBitmap(this in FramebufferHeader header, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        // Check for invalid framebuffer data
        if (header.Width == 0 || header.Height == 0 || header.Bpp == 0)
        {
            return null;
        }

        var colorType = header.StandardizePixelFormat(ref buffer, out var alphaType);

        // Directly work with bitmap's pixel buffer for better performance
        SKBitmap bitmap = new((int)header.Width, (int)header.Height, colorType, alphaType);

        // Access the bitmap's underlying buffer directly and write pixel data in a bulk operation
        var pixelCount = (int)(header.Width * header.Height);
        unsafe
        {
            var ptr = bitmap.GetPixels();

            // Loop through each pixel and write to bitmap's buffer
            var index = 0; // Starting index for the buffer
            for (var i = 0; i < pixelCount; i++)
            {
                if (index + 3 >= buffer.Length)
                {
                    break; // Ensure we don't go out of bounds
                }

                var pixelData = ReadUInt32(buffer, ref index);

                // If the color format is BGRA, swap red and blue bytes
                *(uint*)(ptr + i * 4) = SwapByteOrderIfNeeded(pixelData);
            }
        }

        return bitmap;
    }

// ReadUInt32 helper method to decode pixel data (with ref to avoid index reset every call)
    private static uint ReadUInt32(byte[] buffer, ref int index)
    {
        // Ensure we don't go out of bounds while reading
        if (index + 3 >= buffer.Length)
        {
            throw new ArgumentOutOfRangeException("Buffer is too small to read pixel data.");
        }

        return (uint)(buffer[index++] | (buffer[index++] << 8) | (buffer[index++] << 16) | (buffer[index++] << 24));
    }

// Swap byte order if needed (for BGRA -> RGBA conversion)
    private static uint SwapByteOrderIfNeeded(uint pixelData)
    {
        // If pixel format is BGRA, swap the red and blue channels
        // Adjust this condition based on your framebuffer format
        return ((pixelData & 0xFF0000) >> 16)
               | // Blue channel -> Red
               (pixelData & 0x00FF00)
               | // Green channel stays in place
               ((pixelData & 0x0000FF) << 16)
               | // Red channel -> Blue
               (pixelData & 0xFF000000); // Alpha channel stays in place
    }


    private static SKColorType StandardizePixelFormat(
        this in FramebufferHeader header,
        ref byte[] buffer,
        out SKAlphaType alphaType
    )
    {
        // Initial parameter validation.
        ArgumentNullException.ThrowIfNull(buffer);

        if (buffer.Length < header.Width * header.Height * (header.Bpp / 8))
        {
            throw new ArgumentOutOfRangeException(
                nameof(buffer),
                $"The buffer length {buffer.Length} is less than expected buffer "
                + $"length ({header.Width * header.Height * (header.Bpp / 8)}) for a picture of width {header.Width}, height {header.Height} and pixel depth {header.Bpp}"
            );
        }

        if (header.Width == 0 || header.Height == 0 || header.Bpp == 0)
        {
            throw new InvalidOperationException("Cannot cannulate the pixel format of an empty framebuffer");
        }

        // By far, the most common format is a 32-bit pixel format, which is either
        // RGB or RGBA, where each color has 1 byte.
        if (header.Bpp == 8 * 4)
        {
            // Require at least RGB to be present; and require them to be exactly one byte (8 bits) long.
            if (header.Red.Length != 8 || header.Blue.Length != 8 || header.Green.Length != 8)
            {
                throw new ArgumentOutOfRangeException(
                    $"The pixel format with with RGB lengths of {header.Red.Length}:{header.Blue.Length}:{header.Green.Length} is not supported"
                );
            }

            // Alpha can be present or absent, but must be 8 bytes long
            alphaType = header.Alpha.Length switch
            {
                0 => SKAlphaType.Opaque,
                8 => SKAlphaType.Unpremul,
                _ => throw new ArgumentOutOfRangeException($"The alpha length {header.Alpha.Length} is not supported"),
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
}