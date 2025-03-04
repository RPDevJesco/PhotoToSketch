
namespace EdgeDetectionLib.Core
{
    public class GrayscaleImage
    {
        public int Width { get; }
        public int Height { get; }
        public byte[] Pixels { get; }

        public GrayscaleImage(int width, int height, byte[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels ?? throw new ArgumentNullException(nameof(pixels));
        }
    }
}
