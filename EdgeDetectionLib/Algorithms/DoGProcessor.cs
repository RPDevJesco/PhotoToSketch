namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;

    /// <summary>
    /// Difference of Gaussians (DoG) edge detection processor.
    /// </summary>
    public static class DoGProcessor
    {
        /// <summary>
        /// Applies DoG with default sigma values.
        /// </summary>
        /// <param name="image">The input grayscale image.</param>
        /// <returns>Processed grayscale image with edges detected.</returns>
        public static GrayscaleImage Apply(GrayscaleImage image)
        {
            return Apply(image, 1.0, 2.0);  // Default sigmas for edge detection
        }

        /// <summary>
        /// Applies DoG with specified sigma values.
        /// </summary>
        /// <param name="image">The input grayscale image.</param>
        /// <param name="sigma1">The first Gaussian blur sigma (smaller scale).</param>
        /// <param name="sigma2">The second Gaussian blur sigma (larger scale).</param>
        /// <returns>Processed grayscale image with edges detected.</returns>
        public static GrayscaleImage Apply(GrayscaleImage image, double sigma1, double sigma2)
        {
            var blurred1 = GaussianBlurProcessor.Apply(image, sigma1);
            var blurred2 = GaussianBlurProcessor.Apply(image, sigma2);

            byte[] result = new byte[image.Width * image.Height];

            for (int i = 0; i < result.Length; i++)
            {
                int difference = blurred1.Pixels[i] - blurred2.Pixels[i] + 128;
                result[i] = (byte)Math.Clamp(difference, 0, 255);
            }

            return new GrayscaleImage(image.Width, image.Height, result);
        }
    }
}