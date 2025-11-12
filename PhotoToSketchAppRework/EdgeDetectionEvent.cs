using EventChainsCore;
using EdgeDetectionLib.Core;

namespace ImageProcessing.Events
{
    /// <summary>
    /// Applies an edge detection algorithm to the grayscale image in context.
    /// </summary>
    public class EdgeDetectionEvent : BaseEvent
    {
        private readonly Func<GrayscaleImage, GrayscaleImage> _processor;
        private readonly string _algorithmName;
        private readonly string _outputKey;

        public EdgeDetectionEvent(
            Func<GrayscaleImage, GrayscaleImage> processor,
            string algorithmName,
            string outputKey = "edge_detected_image")
        {
            _processor = processor;
            _algorithmName = algorithmName;
            _outputKey = outputKey;
        }

        public override string EventName => $"EdgeDetection_{_algorithmName}";

        public override EventResult Execute(IEventContext context)
        {
            if (!context.TryGet<GrayscaleImage>("grayscale_image", out var grayscaleImage))
            {
                return Failure("Grayscale image not found in context", 0.0);
            }

            try
            {
                var edgeImage = _processor(grayscaleImage);
                context.Set(_outputKey, edgeImage);

                // Calculate precision based on edge detection quality (non-zero pixels)
                int nonZeroPixels = 0;
                for (int i = 0; i < edgeImage.Pixels.Length; i++)
                {
                    if (edgeImage.Pixels[i] > 0) nonZeroPixels++;
                }

                double edgeDensity = (double)nonZeroPixels / edgeImage.Pixels.Length;
                double precision = Math.Clamp(edgeDensity * 100, 50, 100); // 50-100 range

                return Success(new 
                { 
                    Algorithm = _algorithmName,
                    EdgeDensity = edgeDensity,
                    NonZeroPixels = nonZeroPixels,
                    OutputKey = _outputKey
                }, precision);
            }
            catch (Exception ex)
            {
                return Failure($"Edge detection failed with {_algorithmName}: {ex.Message}", 0.0);
            }
        }
    }
}