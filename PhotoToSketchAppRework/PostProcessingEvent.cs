using EventChainsCore;
using EdgeDetectionLib.Core;
using EdgeDetectionLib.Algorithms;

namespace ImageProcessing.Events
{
    /// <summary>
    /// Applies post-processing operations like Gaussian blur and color inversion.
    /// </summary>
    public class PostProcessingEvent : BaseEvent
    {
        private readonly double _blurSigma;
        private readonly bool _invert;
        private readonly string _inputKey;
        private readonly string _outputKey;

        public PostProcessingEvent(
            double blurSigma = 1.0,
            bool invert = true,
            string inputKey = "edge_detected_image",
            string outputKey = "final_processed_image")
        {
            _blurSigma = blurSigma;
            _invert = invert;
            _inputKey = inputKey;
            _outputKey = outputKey;
        }

        public override string EventName => "PostProcessing";

        public override EventResult Execute(IEventContext context)
        {
            if (!context.TryGet<GrayscaleImage>(_inputKey, out var inputImage))
            {
                return Failure($"Input image '{_inputKey}' not found in context", 0.0);
            }

            try
            {
                var processed = inputImage;

                // Apply Gaussian blur if sigma > 0
                if (_blurSigma > 0)
                {
                    processed = GaussianBlurProcessor.Apply(processed, _blurSigma);
                }

                // Apply inversion if requested
                if (_invert)
                {
                    processed = InvertImage(processed);
                }

                context.Set(_outputKey, processed);

                return Success(new 
                { 
                    BlurApplied = _blurSigma > 0,
                    BlurSigma = _blurSigma,
                    InversionApplied = _invert,
                    OutputKey = _outputKey
                }, 100.0);
            }
            catch (Exception ex)
            {
                return Failure($"Post-processing failed: {ex.Message}", 0.0);
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