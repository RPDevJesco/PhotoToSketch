using System;
using System.Collections.Generic;
using System.Drawing;
using EdgeDetectionLib.Core;
using EdgeDetectionLib.Algorithms;

namespace PhotoToSketchApp
{
    public class AutomaticRegionProcessor
    {
        private const int BLOCK_SIZE = 32; // Adjustable region size

        public Bitmap Process(Bitmap original)
        {
            var grayscale = ImageConversionHelper.BitmapToGrayscaleImage(original);
            var rules = GenerateProcessingRules(grayscale);

            byte[] finalPixels = new byte[grayscale.Pixels.Length];

            foreach (var rule in rules)
            {
                GrayscaleImage regionImage = ExtractRegion(grayscale, rule.Region);
                GrayscaleImage edges = rule.Processor(regionImage);
                ApplyRegion(finalPixels, edges, grayscale.Width, grayscale.Height, rule.Region);
            }

            GrayscaleImage combinedImage = new GrayscaleImage(grayscale.Width, grayscale.Height, finalPixels);
            var blurred = GaussianBlurProcessor.Apply(combinedImage, 1.0);
            var inverted = InvertImage(blurred);
            return ImageConversionHelper.GrayscaleImageToBitmap(inverted);
        }

        private List<RegionProcessingRule> GenerateProcessingRules(GrayscaleImage image)
        {
            var rules = new List<RegionProcessingRule>();

            for (int y = 0; y < image.Height; y += BLOCK_SIZE)
            {
                for (int x = 0; x < image.Width; x += BLOCK_SIZE)
                {
                    var region = new Rectangle(x, y, Math.Min(BLOCK_SIZE, image.Width - x), Math.Min(BLOCK_SIZE, image.Height - y));
                    double variance = CalculateVariance(image, region);
                    double edgeDensity = CalculateEdgeDensity(image, region);
                    double gradient = CalculateAverageGradient(image, region);

                    Func<GrayscaleImage, GrayscaleImage> selectedProcessor;

                    // Ultra-smooth background
                    if (variance < 50 && edgeDensity < 0.05 && gradient < 10)
                    {
                        selectedProcessor = img => GaussianBlurProcessor.Apply(img, 1.0);
                    }
                    // Very high detail (textured clothing, grass, hair)
                    else if (gradient > 50 && variance > 400)
                    {
                        selectedProcessor = img => EdgeCombinationProcessor.CombineTwo(img, SobelProcessor.Apply, DoGProcessor.Apply);
                    }
                    // Curved/flowing edges (hair, natural objects)
                    else if (edgeDensity > 0.1)
                    {
                        selectedProcessor = img => EdgeCombinationProcessor.CombineTwo(img, EdgeFlowProcessor.Apply, PhaseCongruencyProcessor.Apply);
                    }
                    // Default (skin, face, gentle edges)
                    else
                    {
                        selectedProcessor = PhaseCongruencyProcessor.Apply;
                    }


                    rules.Add(new RegionProcessingRule(region, selectedProcessor));
                }
            }

            return rules;
        }

        private double CalculateVariance(GrayscaleImage image, Rectangle region)
        {
            double sum = 0, sumSquares = 0;
            int count = 0;

            for (int y = region.Top; y < region.Bottom; y++)
            {
                for (int x = region.Left; x < region.Right; x++)
                {
                    int pixel = image.Pixels[y * image.Width + x];
                    sum += pixel;
                    sumSquares += pixel * pixel;
                    count++;
                }
            }

            double mean = sum / count;
            return (sumSquares / count) - (mean * mean);
        }

        private double CalculateEdgeDensity(GrayscaleImage image, Rectangle region)
        {
            int count = 0, edgePixels = 0;

            for (int y = region.Top; y < region.Bottom; y++)
            {
                for (int x = region.Left; x < region.Right; x++)
                {
                    if (image.Pixels[y * image.Width + x] > 128) 
                        edgePixels++;
                    count++;
                }
            }

            return (double)edgePixels / count;
        }

        private double CalculateAverageGradient(GrayscaleImage image, Rectangle region)
        {
            double totalGradient = 0;
            int count = 0;

            for (int y = region.Top + 1; y < region.Bottom - 1; y++)
            {
                for (int x = region.Left + 1; x < region.Right - 1; x++)
                {
                    int gx = image.Pixels[y * image.Width + x + 1] - image.Pixels[y * image.Width + x - 1];
                    int gy = image.Pixels[(y + 1) * image.Width + x] - image.Pixels[(y - 1) * image.Width + x];
                    double magnitude = Math.Sqrt(gx * gx + gy * gy);

                    totalGradient += magnitude;
                    count++;
                }
            }

            return totalGradient / count;
        }

        private GrayscaleImage ExtractRegion(GrayscaleImage image, Rectangle region)
        {
            byte[] regionPixels = new byte[region.Width * region.Height];
            for (int y = 0; y < region.Height; y++)
            {
                for (int x = 0; x < region.Width; x++)
                {
                    regionPixels[y * region.Width + x] = image.Pixels[(y + region.Y) * image.Width + (x + region.X)];
                }
            }
            return new GrayscaleImage(region.Width, region.Height, regionPixels);
        }

        private void ApplyRegion(byte[] finalPixels, GrayscaleImage regionEdges, int fullWidth, int fullHeight, Rectangle region)
        {
            for (int y = 0; y < region.Height; y++)
            {
                for (int x = 0; x < region.Width; x++)
                {
                    int fullIndex = (y + region.Y) * fullWidth + (x + region.X);
                    finalPixels[fullIndex] = regionEdges.Pixels[y * region.Width + x];
                }
            }
        }

        private GrayscaleImage InvertImage(GrayscaleImage image)
        {
            byte[] invertedPixels = new byte[image.Width * image.Height];
            for (int i = 0; i < invertedPixels.Length; i++)
            {
                invertedPixels[i] = (byte)(255 - image.Pixels[i]);
            }
            return new GrayscaleImage(image.Width, image.Height, invertedPixels);
        }
    }
}