using EdgeDetectionLib.Core;

namespace EdgeDetectionLib.Algorithms
{
    public static class EdgeCombinationProcessor
    {
        public static GrayscaleImage CombineTwo(GrayscaleImage image, 
                                                Func<GrayscaleImage, GrayscaleImage> first, 
                                                Func<GrayscaleImage, GrayscaleImage> second)
        {
            var firstEdges = first(image);
            var secondEdges = second(image);
            return CombineEdges(firstEdges, secondEdges);
        }

        public static GrayscaleImage CombineThree(GrayscaleImage image, 
                                                  Func<GrayscaleImage, GrayscaleImage> first, 
                                                  Func<GrayscaleImage, GrayscaleImage> second, 
                                                  Func<GrayscaleImage, GrayscaleImage> third)
        {
            var combined = CombineTwo(image, first, second);
            var thirdEdges = third(image);
            return CombineEdges(combined, thirdEdges);
        }

        public static GrayscaleImage CombineFour(GrayscaleImage image, 
                                                 Func<GrayscaleImage, GrayscaleImage> first, 
                                                 Func<GrayscaleImage, GrayscaleImage> second, 
                                                 Func<GrayscaleImage, GrayscaleImage> third, 
                                                 Func<GrayscaleImage, GrayscaleImage> fourth)
        {
            var combined = CombineThree(image, first, second, third);
            var fourthEdges = fourth(image);
            return CombineEdges(combined, fourthEdges);
        }

        private static GrayscaleImage CombineEdges(GrayscaleImage img1, GrayscaleImage img2)
        {
            byte[] combinedPixels = new byte[img1.Width * img1.Height];
            for (int i = 0; i < combinedPixels.Length; i++)
            {
                combinedPixels[i] = (byte)Math.Clamp((img1.Pixels[i] + img2.Pixels[i]) / 2, 0, 255);
            }
            return new GrayscaleImage(img1.Width, img1.Height, combinedPixels);
        }
    }
}
