using System.Drawing;
using EventChainsCore;
using EdgeDetectionLib.Core;
using PhotoToSketchApp;

namespace ImageProcessing.Events
{
    /// <summary>
    /// Loads a bitmap image and converts it to grayscale, storing it in the context.
    /// </summary>
    public class LoadImageEvent : BaseEvent
    {
        private readonly Bitmap _bitmap;

        public LoadImageEvent(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public override EventResult Execute(IEventContext context)
        {
            try
            {
                var grayscaleImage = ImageConversionHelper.BitmapToGrayscaleImage(_bitmap);
                
                context.Set("original_bitmap", _bitmap);
                context.Set("grayscale_image", grayscaleImage);
                context.Set("image_width", grayscaleImage.Width);
                context.Set("image_height", grayscaleImage.Height);

                return Success(new 
                { 
                    Width = grayscaleImage.Width, 
                    Height = grayscaleImage.Height,
                    PixelCount = grayscaleImage.Pixels.Length
                }, 100.0);
            }
            catch (Exception ex)
            {
                return Failure($"Failed to load image: {ex.Message}", 0.0);
            }
        }
    }
}