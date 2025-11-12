using EventChainsCore;
using EdgeDetectionLib.Core;

namespace ImageProcessing.Events
{
    /// <summary>
    /// Combines multiple edge-detected images from context into a single result.
    /// </summary>
    public class EdgeCombinationEvent : BaseEvent
    {
        private readonly List<string> _inputKeys;
        private readonly string _outputKey;
        private readonly CombinationMethod _method;

        public enum CombinationMethod
        {
            Average,
            Max,
            Min,
            WeightedAverage
        }

        public EdgeCombinationEvent(
            List<string> inputKeys,
            string outputKey = "combined_edges",
            CombinationMethod method = CombinationMethod.Average)
        {
            _inputKeys = inputKeys;
            _outputKey = outputKey;
            _method = method;
        }

        public override string EventName => $"CombineEdges_{_method}";

        public override EventResult Execute(IEventContext context)
        {
            var images = new List<GrayscaleImage>();
            
            foreach (var key in _inputKeys)
            {
                if (!context.TryGet<GrayscaleImage>(key, out var image))
                {
                    return Failure($"Edge image '{key}' not found in context", 0.0);
                }
                images.Add(image);
            }

            if (images.Count == 0)
            {
                return Failure("No edge images to combine", 0.0);
            }

            try
            {
                var combined = CombineImages(images);
                context.Set(_outputKey, combined);

                return Success(new 
                { 
                    InputCount = images.Count,
                    Method = _method.ToString(),
                    OutputKey = _outputKey
                }, 100.0);
            }
            catch (Exception ex)
            {
                return Failure($"Edge combination failed: {ex.Message}", 0.0);
            }
        }

        private GrayscaleImage CombineImages(List<GrayscaleImage> images)
        {
            var first = images[0];
            byte[] combinedPixels = new byte[first.Width * first.Height];

            for (int i = 0; i < combinedPixels.Length; i++)
            {
                switch (_method)
                {
                    case CombinationMethod.Average:
                        int sum = 0;
                        foreach (var img in images)
                        {
                            sum += img.Pixels[i];
                        }
                        combinedPixels[i] = (byte)Math.Clamp(sum / images.Count, 0, 255);
                        break;

                    case CombinationMethod.Max:
                        byte max = 0;
                        foreach (var img in images)
                        {
                            if (img.Pixels[i] > max) max = img.Pixels[i];
                        }
                        combinedPixels[i] = max;
                        break;

                    case CombinationMethod.Min:
                        byte min = 255;
                        foreach (var img in images)
                        {
                            if (img.Pixels[i] < min) min = img.Pixels[i];
                        }
                        combinedPixels[i] = min;
                        break;

                    case CombinationMethod.WeightedAverage:
                        // Weight later images more heavily
                        double weighted = 0;
                        double totalWeight = 0;
                        for (int j = 0; j < images.Count; j++)
                        {
                            double weight = j + 1;
                            weighted += images[j].Pixels[i] * weight;
                            totalWeight += weight;
                        }
                        combinedPixels[i] = (byte)Math.Clamp(weighted / totalWeight, 0, 255);
                        break;
                }
            }

            return new GrayscaleImage(first.Width, first.Height, combinedPixels);
        }
    }
}