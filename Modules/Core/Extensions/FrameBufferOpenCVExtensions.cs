namespace NDBotUI.Modules.Core.Extensions;

// <copyright file="FrameBufferOpenCVExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>
using AdvancedSharpAdbClient.Models;
using OpenCvSharp;
using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides extension methods of <see cref="Mat"/> for the <see cref="Framebuffer"/> and <see cref="FramebufferHeader"/> classes.
/// </summary>
public static class FrameBufferOpenCVExtensions
{
    /// <summary>
    /// Converts the framebuffer data to a <see cref="Mat"/>.
    /// </summary>
    /// <param name="framebuffer">The framebuffer data.</param>
    /// <returns>A <see cref="Mat"/> which represents the framebuffer data.</returns>
    public static Mat? ToOpenCVMat(this Framebuffer framebuffer)
    {
        framebuffer.EnsureNotDisposed();
        return framebuffer.Data == null
            ? throw new InvalidOperationException($"Call {nameof(framebuffer.Refresh)} first")
            : framebuffer.Header.ToOpenCVMat(framebuffer.Data);
    }

    /// <summary>
    /// Converts a <see cref="byte"/> array containing the raw frame buffer data to a <see cref="Mat"/>.
    /// </summary>
    /// <param name="header">The header containing the image metadata.</param>
    /// <param name="buffer">The buffer containing the image data.</param>
    /// <returns>A <see cref="Mat"/> that represents the image contained in the frame buffer, or <see langword="null"/>
    /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.</returns>
    [Obsolete("Obsolete")]
    public static Mat? ToOpenCVMat(this in FramebufferHeader header, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (header.Width == 0 || header.Height == 0 || header.Bpp == 0)
        {
            return null;
        }

        int channels = header.StandardizePixelFormat(ref buffer);

        // Tạo Mat grayscale ngay từ đầu
        Mat mat = new((int)header.Width, (int)header.Height, MatType.CV_8UC1);
    
        unsafe
        {
            fixed (byte* pointer = buffer)
            {
                if (channels == 4)
                {
                    // BGRA -> Gray
                    Mat temp = new((int)header.Width, (int)header.Height, MatType.CV_8UC4, new IntPtr(pointer));
                    Cv2.CvtColor(temp, mat, ColorConversionCodes.BGRA2GRAY);
                }
                else if (channels == 3)
                {
                    // BGR -> Gray
                    Mat temp = new((int)header.Width, (int)header.Height, MatType.CV_8UC3, new IntPtr(pointer));
                    Cv2.CvtColor(temp, mat, ColorConversionCodes.BGR2GRAY);
                }
                else
                {
                    // Nếu ảnh gốc đã là grayscale thì copy trực tiếp
                    Buffer.MemoryCopy(pointer, mat.DataPointer, (int)header.Size, (int)header.Size);
                }
            }
        }

        return mat;
    }

    /// <summary>
    /// Returns the <see cref="int"/> that describes pixel format of an image that is stored according to the information
    /// present in this <see cref="FramebufferHeader"/>. Because the <see cref="int"/> enumeration does not allow for all
    /// formats supported by Android, this method also takes a <paramref name="buffer"/> and reorganizes the bytes in the buffer to
    /// match the return value of this function.
    /// </summary>
    /// <param name="header">The header containing the image metadata.</param>
    /// <param name="buffer">A byte array in which the images are stored according to this <see cref="FramebufferHeader"/>.</param>
    /// <returns>A <see cref="int"/> that describes how the image data is represented in this <paramref name="buffer"/>.</returns>
    private static int StandardizePixelFormat(this in FramebufferHeader header, ref byte[] buffer)
    {
        // Initial parameter validation.
        ArgumentNullException.ThrowIfNull(buffer);

        if (buffer.Length < header.Width * header.Height * (header.Bpp / 8))
        {
            throw new ArgumentOutOfRangeException(nameof(buffer),
                $"The buffer length {buffer.Length} is less than expected buffer " +
                $"length ({header.Width * header.Height * (header.Bpp / 8)}) for a picture of width {header.Width}, height {header.Height} and pixel depth {header.Bpp}");
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
                    $"The pixel format with with RGB lengths of {header.Red.Length}:{header.Blue.Length}:{header.Green.Length} is not supported");
            }

            // Alpha can be present or absent, but must be 8 bytes long
            if (header.Alpha.Length is not (0 or 8))
            {
                throw new ArgumentOutOfRangeException($"The alpha length {header.Alpha.Length} is not supported");
            }

            // Gets the index at which the red, bue, green and alpha values are stored.
            int redIndex = (int)header.Red.Offset / 8;
            int blueIndex = (int)header.Blue.Offset / 8;
            int greenIndex = (int)header.Green.Offset / 8;
            int alphaIndex = (int)header.Alpha.Offset / 8;

            byte[] array = new byte[buffer.Length];
            // Loop over the array and re-order as required
            for (int i = 0; i < (int)header.Size; i += 4)
            {
                byte red = buffer[i + redIndex];
                byte blue = buffer[i + blueIndex];
                byte green = buffer[i + greenIndex];
                byte alpha = buffer[i + alphaIndex];

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
            return 4;
        }
        else if (header.Bpp == 8 * 3)
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
                return 3;
            }
        }

        // If not caught by any of the statements before, the format is not supported.
        throw new NotSupportedException($"Pixel depths of {header.Bpp} are not supported");
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(EnsureNotDisposed))]
    private static extern void EnsureNotDisposed(this Framebuffer framebuffer);
}