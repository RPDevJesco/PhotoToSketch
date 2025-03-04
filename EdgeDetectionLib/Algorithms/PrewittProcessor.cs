namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;

    public static class PrewittProcessor
    {
        private static readonly int[,] PrewittX =
        {
            { -1, 0, 1 },
            { -1, 0, 1 },
            { -1, 0, 1 }
        };

        private static readonly int[,] PrewittY =
        {
            { -1, -1, -1 },
            { 0, 0, 0 },
            { 1, 1, 1 }
        };

        public static GrayscaleImage Apply(GrayscaleImage image) =>
            ApplyConvolution(image, PrewittX, PrewittY);

        private static GrayscaleImage ApplyConvolution(GrayscaleImage image, int[,] kernelX, int[,] kernelY)
        {
            int width = image.Width;
            int height = image.Height;
            byte[] result = new byte[width * height];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int gx = 0, gy = 0;

                    for (int j = -1; j <= 1; j++)
                    for (int i = -1; i <= 1; i++)
                    {
                        int pixel = image.Pixels[(y + j) * width + (x + i)];
                        gx += pixel * kernelX[j + 1, i + 1];
                        gy += pixel * kernelY[j + 1, i + 1];
                    }

                    int magnitude = (int)Math.Sqrt(gx * gx + gy * gy);
                    result[y * width + x] = (byte)Math.Clamp(magnitude, 0, 255);
                }
            }

            return new GrayscaleImage(width, height, result);
        }
    }
}