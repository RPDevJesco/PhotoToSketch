namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// SIFT-inspired edge detection processor that uses Difference-of-Gaussian (DoG)
    /// pyramid to detect scale-invariant features and edges.
    /// </summary>
    public static class SIFTProcessor
    {
        private class ScaleSpaceImage
        {
            public GrayscaleImage Image { get; set; }
            public double Sigma { get; set; }

            public ScaleSpaceImage(GrayscaleImage image, double sigma)
            {
                Image = image;
                Sigma = sigma;
            }
        }

        public static GrayscaleImage Apply(GrayscaleImage image)
        {
            // Default SIFT parameters
            int octaves = 4;
            int scalesPerOctave = 5;
            double initialSigma = 1.6;
            double contrastThreshold = 0.03;

            return Apply(image, octaves, scalesPerOctave, initialSigma, contrastThreshold);
        }

        public static GrayscaleImage Apply(GrayscaleImage image, int octaves, int scalesPerOctave, double initialSigma, double contrastThreshold)
        {
            int width = image.Width;
            int height = image.Height;
            byte[] result = new byte[width * height];

            // Build Gaussian scale space pyramid
            var gaussianPyramid = BuildGaussianPyramid(image, octaves, scalesPerOctave, initialSigma);

            // Build DoG pyramid
            var dogPyramid = BuildDoGPyramid(gaussianPyramid);

            // Detect extrema in DoG pyramid
            var keypoints = DetectExtrema(dogPyramid, width, height, contrastThreshold);

            // Mark keypoint locations as edges
            foreach (var keypoint in keypoints)
            {
                int x = (int)Math.Round(keypoint.X);
                int y = (int)Math.Round(keypoint.Y);

                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    // Draw a small circle around keypoint
                    int radius = (int)Math.Max(2, keypoint.Scale * 2);
                    DrawCircle(result, width, height, x, y, radius, 255);
                }
            }

            return new GrayscaleImage(width, height, result);
        }

        private static List<List<ScaleSpaceImage>> BuildGaussianPyramid(GrayscaleImage image, int octaves, int scalesPerOctave, double initialSigma)
        {
            var pyramid = new List<List<ScaleSpaceImage>>();
            double k = Math.Pow(2.0, 1.0 / scalesPerOctave);

            var currentImage = image;

            for (int octave = 0; octave < octaves; octave++)
            {
                var octaveImages = new List<ScaleSpaceImage>();
                double sigma = initialSigma;

                for (int scale = 0; scale < scalesPerOctave + 3; scale++)
                {
                    var blurred = GaussianBlurProcessor.Apply(currentImage, sigma);
                    octaveImages.Add(new ScaleSpaceImage(blurred, sigma));
                    sigma *= k;
                }

                pyramid.Add(octaveImages);

                // Downsample for next octave
                if (octave < octaves - 1)
                {
                    currentImage = Downsample(octaveImages[scalesPerOctave]);
                }
            }

            return pyramid;
        }

        private static List<List<GrayscaleImage>> BuildDoGPyramid(List<List<ScaleSpaceImage>> gaussianPyramid)
        {
            var dogPyramid = new List<List<GrayscaleImage>>();

            foreach (var octave in gaussianPyramid)
            {
                var dogOctave = new List<GrayscaleImage>();

                for (int i = 0; i < octave.Count - 1; i++)
                {
                    var dog = SubtractImages(octave[i + 1].Image, octave[i].Image);
                    dogOctave.Add(dog);
                }

                dogPyramid.Add(dogOctave);
            }

            return dogPyramid;
        }

        private static GrayscaleImage SubtractImages(GrayscaleImage img1, GrayscaleImage img2)
        {
            byte[] result = new byte[img1.Width * img1.Height];

            for (int i = 0; i < result.Length; i++)
            {
                int diff = img1.Pixels[i] - img2.Pixels[i] + 128;
                result[i] = (byte)Math.Clamp(diff, 0, 255);
            }

            return new GrayscaleImage(img1.Width, img1.Height, result);
        }

        private static List<Keypoint> DetectExtrema(List<List<GrayscaleImage>> dogPyramid, int width, int height, double contrastThreshold)
        {
            var keypoints = new List<Keypoint>();
            int threshold = (int)(contrastThreshold * 255);

            for (int octave = 0; octave < dogPyramid.Count; octave++)
            {
                var octaveImages = dogPyramid[octave];

                // Check middle scales only (need neighbors above and below)
                for (int scale = 1; scale < octaveImages.Count - 1; scale++)
                {
                    var current = octaveImages[scale];
                    var below = octaveImages[scale - 1];
                    var above = octaveImages[scale + 1];

                    int w = current.Width;
                    int h = current.Height;

                    for (int y = 1; y < h - 1; y++)
                    {
                        for (int x = 1; x < w - 1; x++)
                        {
                            int centerValue = current.Pixels[y * w + x];

                            // Check if it's a local extremum
                            if (Math.Abs(centerValue - 128) < threshold)
                                continue;

                            bool isExtremum = IsLocalExtremum(current, below, above, x, y, centerValue);

                            if (isExtremum)
                            {
                                // Scale coordinates back to original image size
                                double scaleFactor = Math.Pow(2, octave);
                                keypoints.Add(new Keypoint
                                {
                                    X = x * scaleFactor,
                                    Y = y * scaleFactor,
                                    Scale = scale,
                                    Octave = octave,
                                    Response = Math.Abs(centerValue - 128)
                                });
                            }
                        }
                    }
                }
            }

            return keypoints;
        }

        private static bool IsLocalExtremum(GrayscaleImage current, GrayscaleImage below, GrayscaleImage above, int x, int y, int centerValue)
        {
            int w = current.Width;
            bool isMax = true;
            bool isMin = true;

            // Check 3x3x3 neighborhood
            for (int dz = -1; dz <= 1; dz++)
            {
                GrayscaleImage img = dz == -1 ? below : (dz == 0 ? current : above);

                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0 && dz == 0)
                            continue;

                        int neighborValue = img.Pixels[(y + dy) * w + (x + dx)];

                        if (centerValue <= neighborValue)
                            isMax = false;
                        if (centerValue >= neighborValue)
                            isMin = false;

                        if (!isMax && !isMin)
                            return false;
                    }
                }
            }

            return isMax || isMin;
        }

        private static GrayscaleImage Downsample(ScaleSpaceImage scaleImage)
        {
            var img = scaleImage.Image;
            int newWidth = img.Width / 2;
            int newHeight = img.Height / 2;
            byte[] downsampled = new byte[newWidth * newHeight];

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    downsampled[y * newWidth + x] = img.Pixels[(y * 2) * img.Width + (x * 2)];
                }
            }

            return new GrayscaleImage(newWidth, newHeight, downsampled);
        }

        private static void DrawCircle(byte[] image, int width, int height, int cx, int cy, int radius, byte value)
        {
            for (int y = Math.Max(0, cy - radius); y <= Math.Min(height - 1, cy + radius); y++)
            {
                for (int x = Math.Max(0, cx - radius); x <= Math.Min(width - 1, cx + radius); x++)
                {
                    double dist = Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    
                    if (dist <= radius && dist >= radius - 1)
                    {
                        image[y * width + x] = value;
                    }
                }
            }
        }

        private class Keypoint
        {
            public double X { get; set; }
            public double Y { get; set; }
            public int Scale { get; set; }
            public int Octave { get; set; }
            public int Response { get; set; }
        }
    }
}