using System.Drawing;
using EventChainsCore;
using EdgeDetectionLib.Core;
using EdgeDetectionLib.Algorithms;

namespace ImageProcessing.Events
{
    /// <summary>
    /// Processes an image by dividing it into regions and applying different edge detection
    /// algorithms based on regional characteristics (variance, edge density, gradient).
    /// </summary>
    public class AutomaticRegionProcessingEvent : BaseEvent
    {
        private readonly int _blockSize;

        public AutomaticRegionProcessingEvent(int blockSize = 32)
        {
            _blockSize = blockSize;
        }

        public override string EventName => "AutomaticRegionProcessing";

        public override EventResult Execute(IEventContext context)
        {
            if (!context.TryGet<GrayscaleImage>("grayscale_image", out var grayscaleImage))
            {
                return Failure("Grayscale image not found in context", 0.0);
            }

            try
            {
                var rules = GenerateProcessingRules(grayscaleImage);
                byte[] finalPixels = new byte[grayscaleImage.Pixels.Length];

                int regionsProcessed = 0;
                foreach (var rule in rules)
                {
                    GrayscaleImage regionImage = ExtractRegion(grayscaleImage, rule.Region);
                    GrayscaleImage edges = rule.Processor(regionImage);
                    ApplyRegion(finalPixels, edges, grayscaleImage.Width, grayscaleImage.Height, rule.Region);
                    regionsProcessed++;
                }

                GrayscaleImage combinedImage = new GrayscaleImage(
                    grayscaleImage.Width, 
                    grayscaleImage.Height, 
                    finalPixels);
                
                context.Set("region_processed_image", combinedImage);
                context.Set("regions_processed", regionsProcessed);
                context.Set("block_size", _blockSize);

                // Precision based on successful region processing
                double precision = Math.Min(100.0, 70.0 + (regionsProcessed / 10.0));

                return Success(new 
                { 
                    RegionsProcessed = regionsProcessed,
                    BlockSize = _blockSize,
                    AlgorithmDistribution = GetAlgorithmDistribution(rules)
                }, precision);
            }
            catch (Exception ex)
            {
                return Failure($"Region processing failed: {ex.Message}", 0.0);
            }
        }

        private List<RegionProcessingRule> GenerateProcessingRules(GrayscaleImage image)
        {
            var rules = new List<RegionProcessingRule>();

            for (int y = 0; y < image.Height; y += _blockSize)
            {
                for (int x = 0; x < image.Width; x += _blockSize)
                {
                    var region = new Rectangle(
                        x, y, 
                        Math.Min(_blockSize, image.Width - x), 
                        Math.Min(_blockSize, image.Height - y));
                    
                    double variance = CalculateVariance(image, region);
                    double edgeDensity = CalculateEdgeDensity(image, region);
                    double gradient = CalculateAverageGradient(image, region);

                    Func<GrayscaleImage, GrayscaleImage> selectedProcessor;
                    string algorithmName;

                    // Ultra-smooth background
                    if (variance < 50 && edgeDensity < 0.05 && gradient < 10)
                    {
                        selectedProcessor = img => GaussianBlurProcessor.Apply(img, 1.0);
                        algorithmName = "GaussianBlur";
                    }
                    // Very high detail (textured clothing, grass, hair)
                    else if (gradient > 50 && variance > 400)
                    {
                        selectedProcessor = img => CombineTwo(img, SobelProcessor.Apply, DoGProcessor.Apply);
                        algorithmName = "Sobel+DoG";
                    }
                    // Curved/flowing edges (hair, natural objects)
                    else if (edgeDensity > 0.1)
                    {
                        selectedProcessor = img => CombineTwo(img, EdgeFlowProcessor.Apply, PhaseCongruencyProcessor.Apply);
                        algorithmName = "EdgeFlow+PhaseCongruency";
                    }
                    // Default (skin, face, gentle edges)
                    else
                    {
                        selectedProcessor = PhaseCongruencyProcessor.Apply;
                        algorithmName = "PhaseCongruency";
                    }

                    rules.Add(new RegionProcessingRule(region, selectedProcessor, algorithmName));
                }
            }

            return rules;
        }

        private GrayscaleImage CombineTwo(GrayscaleImage image, 
            Func<GrayscaleImage, GrayscaleImage> first, 
            Func<GrayscaleImage, GrayscaleImage> second)
        {
            var firstEdges = first(image);
            var secondEdges = second(image);
            
            byte[] combinedPixels = new byte[firstEdges.Width * firstEdges.Height];
            for (int i = 0; i < combinedPixels.Length; i++)
            {
                combinedPixels[i] = (byte)Math.Clamp((firstEdges.Pixels[i] + secondEdges.Pixels[i]) / 2, 0, 255);
            }
            return new GrayscaleImage(firstEdges.Width, firstEdges.Height, combinedPixels);
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

            return count > 0 ? totalGradient / count : 0;
        }

        private GrayscaleImage ExtractRegion(GrayscaleImage image, Rectangle region)
        {
            byte[] regionPixels = new byte[region.Width * region.Height];
            for (int y = 0; y < region.Height; y++)
            {
                for (int x = 0; x < region.Width; x++)
                {
                    regionPixels[y * region.Width + x] = 
                        image.Pixels[(y + region.Y) * image.Width + (x + region.X)];
                }
            }
            return new GrayscaleImage(region.Width, region.Height, regionPixels);
        }

        private void ApplyRegion(byte[] finalPixels, GrayscaleImage regionEdges, 
            int fullWidth, int fullHeight, Rectangle region)
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

        private Dictionary<string, int> GetAlgorithmDistribution(List<RegionProcessingRule> rules)
        {
            var distribution = new Dictionary<string, int>();
            foreach (var rule in rules)
            {
                if (!distribution.ContainsKey(rule.AlgorithmName))
                {
                    distribution[rule.AlgorithmName] = 0;
                }
                distribution[rule.AlgorithmName]++;
            }
            return distribution;
        }

        private class RegionProcessingRule
        {
            public Rectangle Region { get; }
            public Func<GrayscaleImage, GrayscaleImage> Processor { get; }
            public string AlgorithmName { get; }

            public RegionProcessingRule(Rectangle region, 
                Func<GrayscaleImage, GrayscaleImage> processor,
                string algorithmName)
            {
                Region = region;
                Processor = processor;
                AlgorithmName = algorithmName;
            }
        }
    }
}