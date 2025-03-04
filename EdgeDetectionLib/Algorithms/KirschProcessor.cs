namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;

    public static class KirschProcessor
    {
        private static readonly int[][] KirschKernels =
        {
            new[] { 5, 5, 5, -3, 0, -3, -3, -3, -3 },
            new[] { -3, 5, 5, -3, 0, 5, -3, -3, -3 },
            new[] { -3, -3, 5, -3, 0, 5, -3, -3, 5 },
            new[] { -3, -3, -3, -3, 0, 5, -3, 5, 5 },
            new[] { -3, -3, -3, -3, 0, -3, 5, 5, 5 },
            new[] { -3, -3, -3, 5, 0, -3, 5, 5, -3 },
            new[] { 5, -3, -3, 5, 0, -3, 5, -3, -3 },
            new[] { 5, 5, -3, 5, 0, -3, -3, -3, -3 }
        };

        public static GrayscaleImage Apply(GrayscaleImage image)
        {
            int width = image.Width, height = image.Height;
            byte[] result = new byte[width * height];

            for (int y = 1; y < height - 1; y++)
            for (int x = 1; x < width - 1; x++)
            {
                int maxResponse = 0;
                foreach (var kernel in KirschKernels)
                {
                    int response = 0;
                    int k = 0;
                    for (int j = -1; j <= 1; j++)
                    for (int i = -1; i <= 1; i++)
                        response += image.Pixels[(y + j) * width + (x + i)] * kernel[k++];
                    maxResponse = Math.Max(maxResponse, Math.Abs(response));
                }
                result[y * width + x] = (byte)Math.Clamp(maxResponse, 0, 255);
            }
            return new GrayscaleImage(width, height, result);
        }
    }
}