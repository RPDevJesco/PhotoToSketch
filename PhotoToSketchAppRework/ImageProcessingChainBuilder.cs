using System.Drawing;
using EventChainsCore;
using EdgeDetectionLib.Core;
using EdgeDetectionLib.Algorithms;

namespace ImageProcessing.Events
{
    /// <summary>
    /// Builder class for constructing image processing event chains.
    /// Provides convenient methods for common processing workflows.
    /// </summary>
    public class ImageProcessingChainBuilder
    {
        private readonly Bitmap _sourceBitmap;

        public ImageProcessingChainBuilder(Bitmap sourceBitmap)
        {
            _sourceBitmap = sourceBitmap;
        }

        /// <summary>
        /// Creates a chain with automatic region processing (the default/recommended approach).
        /// </summary>
        public EventChain BuildAutomaticRegionChain(int blockSize = 32)
        {
            return EventChain.BestEffort()
                .AddEvent(new LoadImageEvent(_sourceBitmap))
                .AddEvent(new AutomaticRegionProcessingEvent(blockSize))
                .AddEvent(new PostProcessingEvent(
                    blurSigma: 1.0, 
                    invert: true,
                    inputKey: "region_processed_image"))
                .AddEvent(new ConvertToSketchEvent());
        }

        /// <summary>
        /// Creates a chain with a single edge detection algorithm.
        /// </summary>
        public EventChain BuildSingleAlgorithmChain(
            Func<GrayscaleImage, GrayscaleImage> processor,
            string algorithmName)
        {
            return EventChain.Strict()
                .AddEvent(new LoadImageEvent(_sourceBitmap))
                .AddEvent(new EdgeDetectionEvent(processor, algorithmName))
                .AddEvent(new PostProcessingEvent())
                .AddEvent(new ConvertToSketchEvent());
        }

        /// <summary>
        /// Creates a chain that combines two edge detection algorithms.
        /// </summary>
        public EventChain BuildDualAlgorithmChain(
            Func<GrayscaleImage, GrayscaleImage> first,
            string firstName,
            Func<GrayscaleImage, GrayscaleImage> second,
            string secondName)
        {
            return EventChain.Strict()
                .AddEvent(new LoadImageEvent(_sourceBitmap))
                .AddEvent(new EdgeDetectionEvent(first, firstName, "edge_1"))
                .AddEvent(new EdgeDetectionEvent(second, secondName, "edge_2"))
                .AddEvent(new EdgeCombinationEvent(
                    new List<string> { "edge_1", "edge_2" },
                    "edge_detected_image"))
                .AddEvent(new PostProcessingEvent())
                .AddEvent(new ConvertToSketchEvent());
        }

        /// <summary>
        /// Creates a chain that combines three edge detection algorithms.
        /// </summary>
        public EventChain BuildTripleAlgorithmChain(
            Func<GrayscaleImage, GrayscaleImage> first,
            string firstName,
            Func<GrayscaleImage, GrayscaleImage> second,
            string secondName,
            Func<GrayscaleImage, GrayscaleImage> third,
            string thirdName)
        {
            return EventChain.Strict()
                .AddEvent(new LoadImageEvent(_sourceBitmap))
                .AddEvent(new EdgeDetectionEvent(first, firstName, "edge_1"))
                .AddEvent(new EdgeDetectionEvent(second, secondName, "edge_2"))
                .AddEvent(new EdgeDetectionEvent(third, thirdName, "edge_3"))
                .AddEvent(new EdgeCombinationEvent(
                    new List<string> { "edge_1", "edge_2", "edge_3" },
                    "edge_detected_image"))
                .AddEvent(new PostProcessingEvent())
                .AddEvent(new ConvertToSketchEvent());
        }

        /// <summary>
        /// Creates a chain that combines four edge detection algorithms.
        /// </summary>
        public EventChain BuildQuadAlgorithmChain(
            Func<GrayscaleImage, GrayscaleImage> first,
            string firstName,
            Func<GrayscaleImage, GrayscaleImage> second,
            string secondName,
            Func<GrayscaleImage, GrayscaleImage> third,
            string thirdName,
            Func<GrayscaleImage, GrayscaleImage> fourth,
            string fourthName)
        {
            return EventChain.Strict()
                .AddEvent(new LoadImageEvent(_sourceBitmap))
                .AddEvent(new EdgeDetectionEvent(first, firstName, "edge_1"))
                .AddEvent(new EdgeDetectionEvent(second, secondName, "edge_2"))
                .AddEvent(new EdgeDetectionEvent(third, thirdName, "edge_3"))
                .AddEvent(new EdgeDetectionEvent(fourth, fourthName, "edge_4"))
                .AddEvent(new EdgeCombinationEvent(
                    new List<string> { "edge_1", "edge_2", "edge_3", "edge_4" },
                    "edge_detected_image"))
                .AddEvent(new PostProcessingEvent())
                .AddEvent(new ConvertToSketchEvent());
        }

        /// <summary>
        /// Executes the chain and returns the resulting sketch bitmap.
        /// </summary>
        public static Bitmap ExecuteAndGetSketch(EventChain chain)
        {
            var result = chain.ExecuteWithResults();
            
            if (!result.Success)
            {
                throw new InvalidOperationException(
                    $"Image processing failed: {result.EventResults.FirstOrDefault(r => !r.Success)?.ErrorMessage}");
            }

            var bitmap = result.Context.Get<Bitmap>("sketch_bitmap");
            return bitmap;
        }

        // Convenience methods for common processing scenarios

        public Bitmap ProcessWithSobel() => 
            ExecuteAndGetSketch(BuildSingleAlgorithmChain(SobelProcessor.Apply, "Sobel"));

        public Bitmap ProcessWithPrewitt() => 
            ExecuteAndGetSketch(BuildSingleAlgorithmChain(PrewittProcessor.Apply, "Prewitt"));

        public Bitmap ProcessWithRoberts() => 
            ExecuteAndGetSketch(BuildSingleAlgorithmChain(RobertsProcessor.Apply, "Roberts"));

        public Bitmap ProcessWithScharr() => 
            ExecuteAndGetSketch(BuildSingleAlgorithmChain(ScharrProcessor.Apply, "Scharr"));

        public Bitmap ProcessWithKirsch() => 
            ExecuteAndGetSketch(BuildSingleAlgorithmChain(KirschProcessor.Apply, "Kirsch"));

        public Bitmap ProcessWithLoG() => 
            ExecuteAndGetSketch(BuildSingleAlgorithmChain(LoGProcessor.Apply, "LoG"));

        public Bitmap ProcessWithDoG() => 
            ExecuteAndGetSketch(BuildSingleAlgorithmChain(DoGProcessor.Apply, "DoG"));

        public Bitmap ProcessWithPhaseCongruency() => 
            ExecuteAndGetSketch(BuildSingleAlgorithmChain(PhaseCongruencyProcessor.Apply, "PhaseCongruency"));

        public Bitmap ProcessWithEdgeFlow() => 
            ExecuteAndGetSketch(BuildSingleAlgorithmChain(EdgeFlowProcessor.Apply, "EdgeFlow"));

        public Bitmap ProcessWithLindebergScaleSpace() => 
            ExecuteAndGetSketch(BuildSingleAlgorithmChain(LindebergScaleSpaceProcessor.Apply, "LindebergScaleSpace"));

        public Bitmap ProcessWithGaussianBlur() => 
            ExecuteAndGetSketch(BuildSingleAlgorithmChain(img => GaussianBlurProcessor.Apply(img, 1.0), "GaussianBlur"));

        public Bitmap ProcessWithGaborFilter() => 
            ExecuteAndGetSketch(BuildSingleAlgorithmChain(
                img => GaborFilterProcessor.Apply(img, Math.PI / 4, 4.0, 2.0, 0.5), "GaborFilter"));

        // Dual algorithm combinations

        public Bitmap ProcessWithSobelAndPrewitt() => 
            ExecuteAndGetSketch(BuildDualAlgorithmChain(
                SobelProcessor.Apply, "Sobel",
                PrewittProcessor.Apply, "Prewitt"));

        public Bitmap ProcessWithSobelAndLoG() => 
            ExecuteAndGetSketch(BuildDualAlgorithmChain(
                SobelProcessor.Apply, "Sobel",
                LoGProcessor.Apply, "LoG"));

        public Bitmap ProcessWithDoGAndPhaseCongruency() => 
            ExecuteAndGetSketch(BuildDualAlgorithmChain(
                DoGProcessor.Apply, "DoG",
                PhaseCongruencyProcessor.Apply, "PhaseCongruency"));

        public Bitmap ProcessWithPhaseCongruencyAndEdgeFlow() => 
            ExecuteAndGetSketch(BuildDualAlgorithmChain(
                PhaseCongruencyProcessor.Apply, "PhaseCongruency",
                EdgeFlowProcessor.Apply, "EdgeFlow"));

        // Triple algorithm combinations

        public Bitmap ProcessWithSobelLoGAndPhaseCongruency() => 
            ExecuteAndGetSketch(BuildTripleAlgorithmChain(
                SobelProcessor.Apply, "Sobel",
                LoGProcessor.Apply, "LoG",
                PhaseCongruencyProcessor.Apply, "PhaseCongruency"));

        public Bitmap ProcessWithDoGPhaseCongruencyAndEdgeFlow() => 
            ExecuteAndGetSketch(BuildTripleAlgorithmChain(
                DoGProcessor.Apply, "DoG",
                PhaseCongruencyProcessor.Apply, "PhaseCongruency",
                EdgeFlowProcessor.Apply, "EdgeFlow"));

        // Quad algorithm combinations

        public Bitmap ProcessWithDoGPhaseCongruencyEdgeFlowAndGabor() => 
            ExecuteAndGetSketch(BuildQuadAlgorithmChain(
                DoGProcessor.Apply, "DoG",
                PhaseCongruencyProcessor.Apply, "PhaseCongruency",
                EdgeFlowProcessor.Apply, "EdgeFlow",
                img => GaborFilterProcessor.Apply(img, Math.PI / 4, 4.0, 2.0, 0.5), "GaborFilter"));

        // Automatic region processing (recommended default)

        public Bitmap ProcessWithAutomaticRegions(int blockSize = 32) => 
            ExecuteAndGetSketch(BuildAutomaticRegionChain(blockSize));
    }
}