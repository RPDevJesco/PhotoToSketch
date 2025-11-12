using System.Drawing;
using EventChainsCore;
using EdgeDetectionLib.Core;
using PhotoToSketchApp;

namespace ImageProcessing.Events
{
    /// <summary>
    /// Converts the final processed grayscale image back to a Bitmap sketch.
    /// </summary>
    public class ConvertToSketchEvent : BaseEvent
    {
        private readonly string _inputKey;

        public ConvertToSketchEvent(string inputKey = "final_processed_image")
        {
            _inputKey = inputKey;
        }

        public override string EventName => "ConvertToSketch";

        public override EventResult Execute(IEventContext context)
        {
            if (!context.TryGet<GrayscaleImage>(_inputKey, out var processedImage))
            {
                return Failure($"Processed image '{_inputKey}' not found in context", 0.0);
            }

            try
            {
                var sketchBitmap = ImageConversionHelper.GrayscaleImageToBitmap(processedImage);
                context.Set("sketch_bitmap", sketchBitmap);

                return Success(new 
                { 
                    Width = sketchBitmap.Width,
                    Height = sketchBitmap.Height,
                    PixelFormat = sketchBitmap.PixelFormat.ToString()
                }, 100.0);
            }
            catch (Exception ex)
            {
                return Failure($"Sketch conversion failed: {ex.Message}", 0.0);
            }
        }
    }
}