namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;
    using System;

    /// <summary>
    /// Canny edge detection processor with Gaussian smoothing, gradient calculation,
    /// non-maximum suppression, and hysteresis thresholding.
    /// </summary>
    public static class CannyProcessor
    {
        public static GrayscaleImage Apply(GrayscaleImage image)
        {
            return Apply(image, sigma: 1.4, lowThreshold: 50, highThreshold: 100);
        }

        public static GrayscaleImage Apply(GrayscaleImage image, double sigma, int lowThreshold, int highThreshold)
        {
            int width = image.Width;
            int height = image.Height;

            // Step 1: Gaussian smoothing
            var smoothed = GaussianBlurProcessor.Apply(image, sigma);

            // Step 2: Compute gradients
            double[,] gradientMagnitude = new double[width, height];
            double[,] gradientDirection = new double[width, height];
            ComputeGradients(smoothed, gradientMagnitude, gradientDirection);

            // Step 3: Non-maximum suppression
            byte[,] suppressed = NonMaximumSuppression(gradientMagnitude, gradientDirection, width, height);

            // Step 4: Hysteresis thresholding
            byte[] result = HysteresisThresholding(suppressed, width, height, lowThreshold, highThreshold);

            return new GrayscaleImage(width, height, result);
        }

        private static void ComputeGradients(GrayscaleImage image, double[,] magnitude, double[,] direction)
        {
            int width = image.Width;
            int height = image.Height;

            // Sobel kernels
            int[,] sobelX = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] sobelY = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double gx = 0, gy = 0;

                    for (int j = -1; j <= 1; j++)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            int pixel = image.Pixels[(y + j) * width + (x + i)];
                            gx += pixel * sobelX[j + 1, i + 1];
                            gy += pixel * sobelY[j + 1, i + 1];
                        }
                    }

                    magnitude[x, y] = Math.Sqrt(gx * gx + gy * gy);
                    direction[x, y] = Math.Atan2(gy, gx);
                }
            }
        }

        private static byte[,] NonMaximumSuppression(double[,] magnitude, double[,] direction, int width, int height)
        {
            byte[,] result = new byte[width, height];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double angle = direction[x, y] * 180.0 / Math.PI;
                    if (angle < 0) angle += 180;

                    double q = 255, r = 255;

                    // Angle 0 (horizontal edge)
                    if ((0 <= angle && angle < 22.5) || (157.5 <= angle && angle <= 180))
                    {
                        q = magnitude[x, y + 1];
                        r = magnitude[x, y - 1];
                    }
                    // Angle 45 (diagonal edge)
                    else if (22.5 <= angle && angle < 67.5)
                    {
                        q = magnitude[x + 1, y - 1];
                        r = magnitude[x - 1, y + 1];
                    }
                    // Angle 90 (vertical edge)
                    else if (67.5 <= angle && angle < 112.5)
                    {
                        q = magnitude[x + 1, y];
                        r = magnitude[x - 1, y];
                    }
                    // Angle 135 (diagonal edge)
                    else if (112.5 <= angle && angle < 157.5)
                    {
                        q = magnitude[x - 1, y - 1];
                        r = magnitude[x + 1, y + 1];
                    }

                    if (magnitude[x, y] >= q && magnitude[x, y] >= r)
                    {
                        result[x, y] = (byte)Math.Clamp(magnitude[x, y], 0, 255);
                    }
                    else
                    {
                        result[x, y] = 0;
                    }
                }
            }

            return result;
        }

        private static byte[] HysteresisThresholding(byte[,] suppressed, int width, int height, int lowThreshold, int highThreshold)
        {
            byte[] result = new byte[width * height];
            bool[,] visited = new bool[width, height];

            // Classify pixels
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (suppressed[x, y] >= highThreshold)
                    {
                        result[y * width + x] = 255;
                        TraceEdge(suppressed, result, visited, x, y, width, height, lowThreshold);
                    }
                }
            }

            return result;
        }

        private static void TraceEdge(byte[,] suppressed, byte[] result, bool[,] visited, int x, int y, int width, int height, int lowThreshold)
        {
            if (x < 0 || x >= width || y < 0 || y >= height || visited[x, y])
                return;

            visited[x, y] = true;

            if (suppressed[x, y] >= lowThreshold)
            {
                result[y * width + x] = 255;

                // Trace 8-connected neighbors
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        TraceEdge(suppressed, result, visited, x + dx, y + dy, width, height, lowThreshold);
                    }
                }
            }
        }
    }
}