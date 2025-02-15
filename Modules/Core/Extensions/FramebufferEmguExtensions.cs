using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AdvancedSharpAdbClient.Models;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Mat = Emgu.CV.Mat;

namespace NDBotUI.Modules.Core.Extensions;

public static class FramebufferEmguExtensions
{
    public static Mat ToEmguMat(this Framebuffer framebuffer, bool convertToGray = false)
    {
        framebuffer.EnsureNotDisposed();
        if (framebuffer.Data == null) throw new InvalidOperationException($"Call {nameof(framebuffer.Refresh)} first");
        return framebuffer.Header.ToEmguMat(framebuffer.Data, convertToGray);
    }

    private static Mat ToEmguMat(this in FramebufferHeader header, byte[] buffer, bool convertToGray = false)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (header.Width == 0 || header.Height == 0 || header.Bpp == 0)
            throw new InvalidOperationException("Framebuffer không hợp lệ.");

        var width = (int)header.Width;
        var height = (int)header.Height;
        var channels = (int)header.Bpp / 8; // Số kênh màu (3 cho RGB, 4 cho RGBA)
        var bufferSize = width * height * channels;

        if (buffer.Length < bufferSize)
            throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer quá nhỏ so với kích thước ảnh.");

        Mat mat;
        if (header.Bpp == 8) // Nếu đã là ảnh grayscale
        {
            mat = new Mat(height, width, DepthType.Cv8U, 1);

            // Dùng GCHandle để lấy con trỏ an toàn
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                using var wrappedMat = new Mat(height, width, DepthType.Cv8U, 1, handle.AddrOfPinnedObject(), width);
                mat = wrappedMat.Clone(); // Clone để đảm bảo bộ nhớ an toàn
            }
            finally
            {
                handle.Free();
            }
        }
        else
        {
            Mat colorMat;
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                colorMat = new Mat(height, width, DepthType.Cv8U, channels, handle.AddrOfPinnedObject(),
                    width * channels);
            }
            finally
            {
                handle.Free();
            }

            // Chỉ convert nếu ảnh có >1 kênh
            if (convertToGray && colorMat.NumberOfChannels > 1)
            {
                mat = new Mat();
                CvInvoke.CvtColor(colorMat, mat, ColorConversion.Bgra2Gray);
                colorMat.Dispose();
            }
            else
            {
                mat = colorMat; // Giữ nguyên màu
            }
        }

        return mat;
    }

    /// <summary>
    ///     Kiểm tra Framebuffer có bị dispose hay không.
    /// </summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(EnsureNotDisposed))]
    private static extern void EnsureNotDisposed(this Framebuffer framebuffer);
}