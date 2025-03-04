namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;
    using System;

    public static class GaussianBlurProcessor
    {
        public static GrayscaleImage Apply(GrayscaleImage image, double sigma = 1.0)
        {
            int radius = (int)Math.Ceiling(3 * sigma);
            int size = 2 * radius + 1;
            double[] kernel = new double[size];

            double sum = 0;
            for (int i = -radius; i <= radius; i++)
            {
                kernel[i + radius] = Math.Exp(-0.5 * i * i / (sigma * sigma));
                sum += kernel[i + radius];
            }
            for (int i = 0; i < size; i++) kernel[i] /= sum;

            return Convolve(image, kernel, radius);
        }

        private static GrayscaleImage Convolve(GrayscaleImage image, double[] kernel, int radius)
        {
            int width = image.Width, height = image.Height;
            byte[] temp = new byte[width * height];
            byte[] result = new byte[width * height];

            // Horizontal pass
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    double sum = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int px = Math.Clamp(x + k, 0, width - 1);
                        sum += image.Pixels[y * width + px] * kernel[k + radius];
                    }
                    temp[y * width + x] = (byte)Math.Clamp(sum, 0, 255);
                }

            // Vertical pass
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    double sum = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int py = Math.Clamp(y + k, 0, height - 1);
                        sum += temp[py * width + x] * kernel[k + radius];
                    }
                    result[y * width + x] = (byte)Math.Clamp(sum, 0, 255);
                }

            return new GrayscaleImage(width, height, result);
        }
    }
}