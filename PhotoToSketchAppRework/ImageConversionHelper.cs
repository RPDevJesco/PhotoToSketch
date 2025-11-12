using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using EdgeDetectionLib.Core;

namespace PhotoToSketchApp
{
    public static class ImageConversionHelper
    {
        public static GrayscaleImage BitmapToGrayscaleImage(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[] pixels = new byte[width * height];

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[] buffer = new byte[data.Stride * height];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
            bitmap.UnlockBits(data);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (y * data.Stride) + (x * 3);
                    byte gray = (byte)((buffer[offset] * 0.114) + (buffer[offset + 1] * 0.587) + (buffer[offset + 2] * 0.299));
                    pixels[y * width + x] = gray;
                }
            }

            return new GrayscaleImage(width, height, pixels);
        }

        public static Bitmap GrayscaleImageToBitmap(GrayscaleImage image)
        {
            Bitmap bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            byte[] buffer = new byte[data.Stride * image.Height];

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    byte gray = image.Pixels[y * image.Width + x];
                    int offset = (y * data.Stride) + (x * 3);
                    buffer[offset] = gray;
                    buffer[offset + 1] = gray;
                    buffer[offset + 2] = gray;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }
    }
}