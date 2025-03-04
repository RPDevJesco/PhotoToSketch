namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;
    using System;

    public static class GaborFilterProcessor
    {
        public static GrayscaleImage Apply(GrayscaleImage image, double theta, double lambda, double sigma, double gamma)
        {
            int width = image.Width;
            int height = image.Height;
            byte[] result = new byte[width * height];

            double cosTheta = Math.Cos(theta);
            double sinTheta = Math.Sin(theta);
            int radius = (int)Math.Ceiling(3 * sigma);

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                double sum = 0;
                for (int j = -radius; j <= radius; j++)
                for (int i = -radius; i <= radius; i++)
                {
                    int sampleX = Math.Clamp(x + i, 0, width - 1);
                    int sampleY = Math.Clamp(y + j, 0, height - 1);

                    double xTheta = i * cosTheta + j * sinTheta;
                    double yTheta = -i * sinTheta + j * cosTheta;

                    double gabor = Math.Exp(-(xTheta * xTheta + gamma * gamma * yTheta * yTheta) / (2 * sigma * sigma)) *
                                   Math.Cos(2 * Math.PI * xTheta / lambda);

                    sum += image.Pixels[sampleY * width + sampleX] * gabor;
                }
                result[y * width + x] = (byte)Math.Clamp(sum, 0, 255);
            }

            return new GrayscaleImage(width, height, result);
        }
    }
}