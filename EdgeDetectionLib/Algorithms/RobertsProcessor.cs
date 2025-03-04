namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;

    public static class RobertsProcessor
    {
        public static GrayscaleImage Apply(GrayscaleImage image)
        {
            int width = image.Width, height = image.Height;
            byte[] result = new byte[width * height];

            for (int y = 0; y < height - 1; y++)
            for (int x = 0; x < width - 1; x++)
            {
                int index = y * width + x;
                int gx = image.Pixels[index] - image.Pixels[index + width + 1];
                int gy = image.Pixels[index + 1] - image.Pixels[index + width];
                int magnitude = (int)Math.Sqrt(gx * gx + gy * gy);
                result[index] = (byte)Math.Clamp(magnitude, 0, 255);
            }

            return new GrayscaleImage(width, height, result);
        }
    }
}