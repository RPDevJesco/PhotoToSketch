namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;
    using System;

    public static class LindebergScaleSpaceProcessor
    {
        public static GrayscaleImage Apply(GrayscaleImage image)
        {
            return Apply(image, 1.0, 5, 1.5);  // Default: sigma=1.0, 5 scales, step=1.5
        }
        
        public static GrayscaleImage Apply(GrayscaleImage image, double initialSigma = 1.0, int scales = 5, double scaleStep = 1.5)
        {
            int width = image.Width;
            int height = image.Height;
            byte[] result = new byte[width * height];

            double sigma = initialSigma;

            for (int s = 0; s < scales; s++)
            {
                var blurred = GaussianBlurProcessor.Apply(image, sigma);
                var laplacian = ApplyLaplacian(blurred);

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = (byte)Math.Max(result[i], laplacian.Pixels[i]);
                }

                sigma *= scaleStep;
            }

            return new GrayscaleImage(width, height, result);
        }

        private static GrayscaleImage ApplyLaplacian(GrayscaleImage image)
        {
            int width = image.Width, height = image.Height;
            byte[] result = new byte[width * height];

            int[,] kernel = {
                { 0, 1, 0 },
                { 1, -4, 1 },
                { 0, 1, 0 }
            };

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int sum = 0;
                    for (int j = -1; j <= 1; j++)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            sum += image.Pixels[(y + j) * width + (x + i)] * kernel[j + 1, i + 1];
                        }
                    }

                    result[y * width + x] = (byte)Math.Clamp(Math.Abs(sum), 0, 255);
                }
            }

            return new GrayscaleImage(width, height, result);
        }
    }
}