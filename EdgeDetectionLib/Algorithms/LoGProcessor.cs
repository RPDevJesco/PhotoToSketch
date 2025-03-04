namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;

    public static class LoGProcessor
    {
        public static GrayscaleImage Apply(GrayscaleImage image)
        {
            int[,] kernel = {
                { 0, 0, -1, 0, 0 },
                { 0, -1, -2, -1, 0 },
                { -1, -2, 16, -2, -1 },
                { 0, -1, -2, -1, 0 },
                { 0, 0, -1, 0, 0 }
            };

            return Convolve(image, kernel);
        }

        private static GrayscaleImage Convolve(GrayscaleImage image, int[,] kernel)
        {
            int width = image.Width, height = image.Height;
            byte[] result = new byte[width * height];

            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    int sum = 0;

                    for (int j = -2; j <= 2; j++)
                    {
                        for (int i = -2; i <= 2; i++)
                        {
                            sum += image.Pixels[(y + j) * width + (x + i)] * kernel[j + 2, i + 2];
                        }
                    }
                    result[y * width + x] = (byte)Math.Clamp(sum, 0, 255);
                }
            }

            return new GrayscaleImage(width, height, result);
        }
    }
}