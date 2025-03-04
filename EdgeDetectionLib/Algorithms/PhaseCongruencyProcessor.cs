namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;
    using System;

    public static class PhaseCongruencyProcessor
    {
        public static GrayscaleImage Apply(GrayscaleImage image)
        {
            return Apply(image, 4, 4); // Defaults: 4 orientations, 4 scales
        }
        
        public static GrayscaleImage Apply(GrayscaleImage image, int numOrientations = 4, int numScales = 4)
        {
            int width = image.Width;
            int height = image.Height;
            byte[] result = new byte[width * height];

            // Placeholder frequency and sigma values for filter bank
            double[] frequencies = { 1.0, 2.0, 4.0, 8.0 };
            double sigmaOnf = 0.55;

            double[,] phaseCongruency = new double[width, height];

            for (int orientation = 0; orientation < numOrientations; orientation++)
            {
                double theta = orientation * Math.PI / numOrientations;
                double cosTheta = Math.Cos(theta);
                double sinTheta = Math.Sin(theta);

                for (int scale = 0; scale < numScales; scale++)
                {
                    double frequency = frequencies[scale];
                    double sigma = frequency * sigmaOnf;

                    double[,] realResponse = new double[width, height];
                    double[,] imagResponse = new double[width, height];

                    ApplyGaborFilter(image, frequency, sigma, cosTheta, sinTheta, realResponse, imagResponse);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            double magnitude = Math.Sqrt(realResponse[x, y] * realResponse[x, y] +
                                                         imagResponse[x, y] * imagResponse[x, y]);
                            double phase = Math.Atan2(imagResponse[x, y], realResponse[x, y]);
                            phaseCongruency[x, y] += magnitude * (1.0 - Math.Cos(phase));
                        }
                    }
                }
            }

            // Normalize phase congruency and convert to byte image
            double maxPC = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    maxPC = Math.Max(maxPC, phaseCongruency[x, y]);
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[y * width + x] = (byte)Math.Clamp(phaseCongruency[x, y] / maxPC * 255, 0, 255);
                }
            }

            return new GrayscaleImage(width, height, result);
        }

        private static void ApplyGaborFilter(GrayscaleImage image, double frequency, double sigma, double cosTheta, double sinTheta, double[,] real, double[,] imag)
        {
            int width = image.Width;
            int height = image.Height;
            int radius = (int)Math.Ceiling(3 * sigma);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double realSum = 0;
                    double imagSum = 0;

                    for (int j = -radius; j <= radius; j++)
                    {
                        for (int i = -radius; i <= radius; i++)
                        {
                            int sampleX = Math.Clamp(x + i, 0, width - 1);
                            int sampleY = Math.Clamp(y + j, 0, height - 1);

                            double xTheta = i * cosTheta + j * sinTheta;
                            double yTheta = -i * sinTheta + j * cosTheta;

                            double envelope = Math.Exp(-(xTheta * xTheta + yTheta * yTheta) / (2 * sigma * sigma));
                            double phase = 2 * Math.PI * frequency * xTheta;

                            realSum += image.Pixels[sampleY * width + sampleX] * envelope * Math.Cos(phase);
                            imagSum += image.Pixels[sampleY * width + sampleX] * envelope * Math.Sin(phase);
                        }
                    }

                    real[x, y] = realSum;
                    imag[x, y] = imagSum;
                }
            }
        }
    }
}