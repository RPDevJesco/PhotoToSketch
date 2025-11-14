namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;
    using System;

    /// <summary>
    /// Gabor Filter Bank processor that applies multiple Gabor filters at different
    /// orientations and scales, combining their responses for robust edge detection.
    /// </summary>
    public static class GaborFilterBankProcessor
    {
        public static GrayscaleImage Apply(GrayscaleImage image)
        {
            // Default configuration: 4 orientations, 3 scales
            int numOrientations = 4;
            double[] frequencies = { 0.05, 0.1, 0.2 }; // Low, medium, high frequency
            double sigma = 3.0;
            double gamma = 0.5;

            return Apply(image, numOrientations, frequencies, sigma, gamma);
        }

        public static GrayscaleImage Apply(GrayscaleImage image, int numOrientations, double[] frequencies, double sigma, double gamma)
        {
            int width = image.Width;
            int height = image.Height;
            double[,] responseSum = new double[width, height];
            double maxResponse = 0;

            // Process each orientation
            for (int orientation = 0; orientation < numOrientations; orientation++)
            {
                double theta = orientation * Math.PI / numOrientations;

                // Process each frequency scale
                foreach (double frequency in frequencies)
                {
                    var gaborResponse = ApplySingleGaborFilter(image, theta, frequency, sigma, gamma);

                    // Accumulate responses
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int idx = y * width + x;
                            double response = Math.Abs(gaborResponse.Pixels[idx]);
                            responseSum[x, y] += response;
                            
                            if (responseSum[x, y] > maxResponse)
                                maxResponse = responseSum[x, y];
                        }
                    }
                }
            }

            // Normalize to 0-255 range
            byte[] result = new byte[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;
                    result[idx] = (byte)Math.Clamp((responseSum[x, y] / maxResponse) * 255, 0, 255);
                }
            }

            return new GrayscaleImage(width, height, result);
        }

        private static GrayscaleImage ApplySingleGaborFilter(GrayscaleImage image, double theta, double frequency, double sigma, double gamma)
        {
            int width = image.Width;
            int height = image.Height;
            double[] result = new double[width * height];

            double cosTheta = Math.Cos(theta);
            double sinTheta = Math.Sin(theta);
            double lambda = 1.0 / frequency;
            int radius = (int)Math.Ceiling(3 * sigma);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double sum = 0;

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int sampleX = Math.Clamp(x + kx, 0, width - 1);
                            int sampleY = Math.Clamp(y + ky, 0, height - 1);

                            // Rotate coordinates
                            double xTheta = kx * cosTheta + ky * sinTheta;
                            double yTheta = -kx * sinTheta + ky * cosTheta;

                            // Gabor function
                            double gaussianEnvelope = Math.Exp(-(xTheta * xTheta + gamma * gamma * yTheta * yTheta) / (2 * sigma * sigma));
                            double sinusoidalCarrier = Math.Cos(2 * Math.PI * xTheta / lambda);
                            double gaborValue = gaussianEnvelope * sinusoidalCarrier;

                            sum += image.Pixels[sampleY * width + sampleX] * gaborValue;
                        }
                    }

                    result[y * width + x] = sum;
                }
            }

            // Convert to byte array (taking absolute values)
            byte[] byteResult = new byte[width * height];
            for (int i = 0; i < result.Length; i++)
            {
                byteResult[i] = (byte)Math.Clamp(Math.Abs(result[i]), 0, 255);
            }

            return new GrayscaleImage(width, height, byteResult);
        }

        /// <summary>
        /// Returns the maximum magnitude response across all orientations at each pixel.
        /// Useful for extracting dominant edge orientations.
        /// </summary>
        public static GrayscaleImage ApplyMaxOrientation(GrayscaleImage image)
        {
            int numOrientations = 8;
            double frequency = 0.1;
            double sigma = 3.0;
            double gamma = 0.5;

            int width = image.Width;
            int height = image.Height;
            double[,] maxResponse = new double[width, height];

            // Find maximum response across all orientations
            for (int orientation = 0; orientation < numOrientations; orientation++)
            {
                double theta = orientation * Math.PI / numOrientations;
                var gaborResponse = ApplySingleGaborFilter(image, theta, frequency, sigma, gamma);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * width + x;
                        double response = Math.Abs(gaborResponse.Pixels[idx]);
                        
                        if (response > maxResponse[x, y])
                            maxResponse[x, y] = response;
                    }
                }
            }

            // Convert to byte array
            byte[] result = new byte[width * height];
            double maxVal = 0;
            
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (maxResponse[x, y] > maxVal)
                        maxVal = maxResponse[x, y];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;
                    result[idx] = (byte)Math.Clamp((maxResponse[x, y] / maxVal) * 255, 0, 255);
                }
            }

            return new GrayscaleImage(width, height, result);
        }
    }
}