namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;
    using System;

    public static class EdgeFlowProcessor
    {
        public static GrayscaleImage Apply(GrayscaleImage image)
        {
            int width = image.Width;
            int height = image.Height;
            byte[] result = new byte[width * height];

            // Compute gradient magnitudes and directions (simple Sobel-like gradients)
            double[,] gradientX = new double[width, height];
            double[,] gradientY = new double[width, height];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double gx = (
                        -1 * image.Pixels[(y - 1) * width + (x - 1)] +
                        1 * image.Pixels[(y - 1) * width + (x + 1)] +
                        -2 * image.Pixels[y * width + (x - 1)] +
                        2 * image.Pixels[y * width + (x + 1)] +
                        -1 * image.Pixels[(y + 1) * width + (x - 1)] +
                        1 * image.Pixels[(y + 1) * width + (x + 1)]
                    );

                    double gy = (
                        -1 * image.Pixels[(y - 1) * width + (x - 1)] +
                        -2 * image.Pixels[(y - 1) * width + x] +
                        -1 * image.Pixels[(y - 1) * width + (x + 1)] +
                        1 * image.Pixels[(y + 1) * width + (x - 1)] +
                        2 * image.Pixels[(y + 1) * width + x] +
                        1 * image.Pixels[(y + 1) * width + (x + 1)]
                    );

                    gradientX[x, y] = gx;
                    gradientY[x, y] = gy;
                }
            }

            // Compute flow-guided edge strength
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double gx = gradientX[x, y];
                    double gy = gradientY[x, y];
                    double magnitude = Math.Sqrt(gx * gx + gy * gy);

                    // Compute neighboring magnitudes along the gradient direction
                    int offsetX = (gx > 0) ? 1 : -1;
                    int offsetY = (gy > 0) ? 1 : -1;

                    double neighbor1 = gradientMagnitude(gradientX, gradientY, x + offsetX, y + offsetY, width, height);
                    double neighbor2 = gradientMagnitude(gradientX, gradientY, x - offsetX, y - offsetY, width, height);

                    // Suppress non-maximum edges
                    if (magnitude >= neighbor1 && magnitude >= neighbor2)
                    {
                        result[y * width + x] = (byte)Math.Clamp(magnitude, 0, 255);
                    }
                    else
                    {
                        result[y * width + x] = 0;
                    }
                }
            }

            return new GrayscaleImage(width, height, result);
        }

        private static double gradientMagnitude(double[,] gradientX, double[,] gradientY, int x, int y, int width, int height)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return 0;
            double gx = gradientX[x, y];
            double gy = gradientY[x, y];
            return Math.Sqrt(gx * gx + gy * gy);
        }
    }
}