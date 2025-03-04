using EdgeDetectionLib.Core;
using EdgeDetectionLib.Algorithms;

namespace PhotoToSketchApp
{
    public class ImageSketchProcessor
    {
        private readonly AutomaticRegionProcessor _regionProcessor = new();

        /// <summary>
        /// Default entry point - can call any preferred method (manual or auto-region).
        /// </summary>
        public Bitmap ProcessImage(Bitmap original) => ProcessWithAutomaticRegions(original);

        // ----------------- Automatic Region Processing -----------------

        public Bitmap ProcessWithAutomaticRegions(Bitmap original)
        {
            return _regionProcessor.Process(original);
        }

        // ----------------- Single Algorithm Processing -----------------

        public Bitmap ProcessWithSobel(Bitmap original) => ProcessSingle(original, SobelProcessor.Apply);
        public Bitmap ProcessWithPrewitt(Bitmap original) => ProcessSingle(original, PrewittProcessor.Apply);
        public Bitmap ProcessWithRoberts(Bitmap original) => ProcessSingle(original, RobertsProcessor.Apply);
        public Bitmap ProcessWithScharr(Bitmap original) => ProcessSingle(original, ScharrProcessor.Apply);
        public Bitmap ProcessWithKirsch(Bitmap original) => ProcessSingle(original, KirschProcessor.Apply);
        public Bitmap ProcessWithLoG(Bitmap original) => ProcessSingle(original, LoGProcessor.Apply);
        public Bitmap ProcessWithDoG(Bitmap original) => ProcessSingle(original, DoGProcessor.Apply);
        public Bitmap ProcessWithGaussianBlur(Bitmap original) => ProcessSingle(original, img => GaussianBlurProcessor.Apply(img, 1.0));
        public Bitmap ProcessWithGaborFilter(Bitmap original) => ProcessSingle(original, img => GaborFilterProcessor.Apply(img, Math.PI / 4, 4.0, 2.0, 0.5));
        public Bitmap ProcessWithPhaseCongruency(Bitmap original) => ProcessSingle(original, PhaseCongruencyProcessor.Apply);
        public Bitmap ProcessWithEdgeFlow(Bitmap original) => ProcessSingle(original, EdgeFlowProcessor.Apply);
        public Bitmap ProcessWithLindebergScaleSpace(Bitmap original) => ProcessSingle(original, LindebergScaleSpaceProcessor.Apply);

        // ----------------- Combined Processing (Pairs) -----------------

        public Bitmap ProcessWithSobelAndPrewitt(Bitmap original) => CombinedProcess(original, SobelProcessor.Apply, PrewittProcessor.Apply);
        public Bitmap ProcessWithSobelAndLoG(Bitmap original) => CombinedProcess(original, SobelProcessor.Apply, LoGProcessor.Apply);
        public Bitmap ProcessWithPrewittAndLoG(Bitmap original) => CombinedProcess(original, PrewittProcessor.Apply, LoGProcessor.Apply);
        public Bitmap ProcessWithDoGAndPhaseCongruency(Bitmap original) => CombinedProcess(original, DoGProcessor.Apply, PhaseCongruencyProcessor.Apply);
        public Bitmap ProcessWithPhaseCongruencyAndEdgeFlow(Bitmap original) => CombinedProcess(original, PhaseCongruencyProcessor.Apply, EdgeFlowProcessor.Apply);
        public Bitmap ProcessWithLindebergScaleSpaceAndPhaseCongruency(Bitmap original) => CombinedProcess(original, LindebergScaleSpaceProcessor.Apply, PhaseCongruencyProcessor.Apply);
        public Bitmap ProcessWithKirschAndScharr(Bitmap original) => CombinedProcess(original, KirschProcessor.Apply, ScharrProcessor.Apply);
        public Bitmap ProcessWithGaborFilterAndPhaseCongruency(Bitmap original) => CombinedProcess(original, img => GaborFilterProcessor.Apply(img, Math.PI / 4, 4.0, 2.0, 0.5), PhaseCongruencyProcessor.Apply);

        // ----------------- Combined Processing (Triples & Quads) -----------------

        public Bitmap ProcessWithSobelLoGAndPhaseCongruency(Bitmap original) =>
            CombinedProcess(original, SobelProcessor.Apply, LoGProcessor.Apply, PhaseCongruencyProcessor.Apply);

        public Bitmap ProcessWithDoGPhaseCongruencyAndEdgeFlow(Bitmap original) =>
            CombinedProcess(original, DoGProcessor.Apply, PhaseCongruencyProcessor.Apply, EdgeFlowProcessor.Apply);

        public Bitmap ProcessWithPrewittLoGAndPhaseCongruency(Bitmap original) =>
            CombinedProcess(original, PrewittProcessor.Apply, LoGProcessor.Apply, PhaseCongruencyProcessor.Apply);

        public Bitmap ProcessWithSobelKirschAndScharr(Bitmap original) =>
            CombinedProcess(original, SobelProcessor.Apply, KirschProcessor.Apply, ScharrProcessor.Apply);

        public Bitmap ProcessWithDoGPhaseCongruencyEdgeFlowAndGabor(Bitmap original) =>
            CombinedProcess(original,
                DoGProcessor.Apply,
                PhaseCongruencyProcessor.Apply,
                EdgeFlowProcessor.Apply,
                img => GaborFilterProcessor.Apply(img, Math.PI / 4, 4.0, 2.0, 0.5));

        public Bitmap ProcessWithSobelPrewittLoGAndPhaseCongruency(Bitmap original) =>
            CombinedProcess(original,
                SobelProcessor.Apply,
                PrewittProcessor.Apply,
                LoGProcessor.Apply,
                PhaseCongruencyProcessor.Apply);

        public Bitmap ProcessWithLindebergPhaseCongruencyDoGAndEdgeFlow(Bitmap original) =>
            CombinedProcess(original,
                LindebergScaleSpaceProcessor.Apply,
                PhaseCongruencyProcessor.Apply,
                DoGProcessor.Apply,
                EdgeFlowProcessor.Apply);

        // ----------------- Core Processing Helpers -----------------

        private Bitmap ProcessSingle(Bitmap original, Func<GrayscaleImage, GrayscaleImage> processor)
        {
            var grayscale = ImageConversionHelper.BitmapToGrayscaleImage(original);
            var edges = processor(grayscale);
            return FinalizeSketch(edges);
        }

        private Bitmap CombinedProcess(Bitmap original, params Func<GrayscaleImage, GrayscaleImage>[] processors)
        {
            var grayscale = ImageConversionHelper.BitmapToGrayscaleImage(original);

            GrayscaleImage combined = processors[0](grayscale);

            for (int i = 1; i < processors.Length; i++)
            {
                var nextEdges = processors[i](grayscale);
                combined = CombineEdges(combined, nextEdges);
            }

            return FinalizeSketch(combined);
        }

        private Bitmap FinalizeSketch(GrayscaleImage edges)
        {
            var blurred = GaussianBlurProcessor.Apply(edges, 1.0);
            var inverted = InvertImage(blurred);
            return ImageConversionHelper.GrayscaleImageToBitmap(inverted);
        }

        private GrayscaleImage CombineEdges(GrayscaleImage img1, GrayscaleImage img2)
        {
            byte[] combinedPixels = new byte[img1.Width * img1.Height];
            for (int i = 0; i < combinedPixels.Length; i++)
            {
                combinedPixels[i] = (byte)Math.Clamp((img1.Pixels[i] + img2.Pixels[i]) / 2, 0, 255);
            }
            return new GrayscaleImage(img1.Width, img1.Height, combinedPixels);
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