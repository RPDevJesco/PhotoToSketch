namespace EdgeDetectionLib.Algorithms
{
    using EdgeDetectionLib.Core;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Morphological Reconstruction processor that uses morphological operations
    /// (erosion, dilation, opening, closing) for edge detection and enhancement.
    /// Particularly useful for detecting edges while preserving connectivity.
    /// </summary>
    public static class MorphologicalReconstructionProcessor
    {
        /// <summary>
        /// Applies morphological edge detection using gradient (dilation - erosion).
        /// </summary>
        public static GrayscaleImage Apply(GrayscaleImage image)
        {
            return ApplyMorphologicalGradient(image, structuringElementSize: 3);
        }

        /// <summary>
        /// Morphological gradient: detects edges by computing dilation - erosion.
        /// </summary>
        public static GrayscaleImage ApplyMorphologicalGradient(GrayscaleImage image, int structuringElementSize = 3)
        {
            var dilated = Dilate(image, structuringElementSize);
            var eroded = Erode(image, structuringElementSize);

            byte[] gradient = new byte[image.Width * image.Height];

            for (int i = 0; i < gradient.Length; i++)
            {
                gradient[i] = (byte)Math.Clamp(dilated.Pixels[i] - eroded.Pixels[i], 0, 255);
            }

            return new GrayscaleImage(image.Width, image.Height, gradient);
        }

        /// <summary>
        /// Internal gradient: original - erosion (detects inner edges).
        /// </summary>
        public static GrayscaleImage ApplyInternalGradient(GrayscaleImage image, int structuringElementSize = 3)
        {
            var eroded = Erode(image, structuringElementSize);
            byte[] gradient = new byte[image.Width * image.Height];

            for (int i = 0; i < gradient.Length; i++)
            {
                gradient[i] = (byte)Math.Clamp(image.Pixels[i] - eroded.Pixels[i], 0, 255);
            }

            return new GrayscaleImage(image.Width, image.Height, gradient);
        }

        /// <summary>
        /// External gradient: dilation - original (detects outer edges).
        /// </summary>
        public static GrayscaleImage ApplyExternalGradient(GrayscaleImage image, int structuringElementSize = 3)
        {
            var dilated = Dilate(image, structuringElementSize);
            byte[] gradient = new byte[image.Width * image.Height];

            for (int i = 0; i < gradient.Length; i++)
            {
                gradient[i] = (byte)Math.Clamp(dilated.Pixels[i] - image.Pixels[i], 0, 255);
            }

            return new GrayscaleImage(image.Width, image.Height, gradient);
        }

        /// <summary>
        /// Top-hat transform: original - opening (detects bright features on dark background).
        /// </summary>
        public static GrayscaleImage ApplyTopHat(GrayscaleImage image, int structuringElementSize = 5)
        {
            var opened = Open(image, structuringElementSize);
            byte[] topHat = new byte[image.Width * image.Height];

            for (int i = 0; i < topHat.Length; i++)
            {
                topHat[i] = (byte)Math.Clamp(image.Pixels[i] - opened.Pixels[i], 0, 255);
            }

            return new GrayscaleImage(image.Width, image.Height, topHat);
        }

        /// <summary>
        /// Black-hat transform: closing - original (detects dark features on bright background).
        /// </summary>
        public static GrayscaleImage ApplyBlackHat(GrayscaleImage image, int structuringElementSize = 5)
        {
            var closed = Close(image, structuringElementSize);
            byte[] blackHat = new byte[image.Width * image.Height];

            for (int i = 0; i < blackHat.Length; i++)
            {
                blackHat[i] = (byte)Math.Clamp(closed.Pixels[i] - image.Pixels[i], 0, 255);
            }

            return new GrayscaleImage(image.Width, image.Height, blackHat);
        }

        /// <summary>
        /// Morphological reconstruction by dilation: reconstructs image from marker using mask.
        /// </summary>
        public static GrayscaleImage ReconstructByDilation(GrayscaleImage marker, GrayscaleImage mask)
        {
            int width = marker.Width;
            int height = marker.Height;
            byte[] result = new byte[marker.Pixels.Length];
            Array.Copy(marker.Pixels, result, marker.Pixels.Length);

            bool changed = true;
            int iterations = 0;
            int maxIterations = 1000; // Prevent infinite loops

            while (changed && iterations < maxIterations)
            {
                changed = false;
                iterations++;

                byte[] temp = new byte[result.Length];
                Array.Copy(result, temp, result.Length);

                // Dilate
                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        int idx = y * width + x;
                        byte maxVal = temp[idx];

                        // 8-connected neighborhood
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int neighborIdx = (y + dy) * width + (x + dx);
                                if (temp[neighborIdx] > maxVal)
                                    maxVal = temp[neighborIdx];
                            }
                        }

                        // Point-wise minimum with mask
                        byte newVal = Math.Min(maxVal, mask.Pixels[idx]);

                        if (newVal != result[idx])
                        {
                            result[idx] = newVal;
                            changed = true;
                        }
                    }
                }
            }

            return new GrayscaleImage(width, height, result);
        }

        /// <summary>
        /// Morphological reconstruction by erosion: dual of reconstruction by dilation.
        /// </summary>
        public static GrayscaleImage ReconstructByErosion(GrayscaleImage marker, GrayscaleImage mask)
        {
            // Invert, reconstruct by dilation, invert back
            var invertedMarker = InvertImage(marker);
            var invertedMask = InvertImage(mask);
            var reconstructed = ReconstructByDilation(invertedMarker, invertedMask);
            return InvertImage(reconstructed);
        }

        // Basic morphological operations

        private static GrayscaleImage Dilate(GrayscaleImage image, int size)
        {
            int width = image.Width;
            int height = image.Height;
            byte[] result = new byte[width * height];
            int radius = size / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte maxVal = 0;

                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            int nx = Math.Clamp(x + dx, 0, width - 1);
                            int ny = Math.Clamp(y + dy, 0, height - 1);
                            byte val = image.Pixels[ny * width + nx];
                            
                            if (val > maxVal)
                                maxVal = val;
                        }
                    }

                    result[y * width + x] = maxVal;
                }
            }

            return new GrayscaleImage(width, height, result);
        }

        private static GrayscaleImage Erode(GrayscaleImage image, int size)
        {
            int width = image.Width;
            int height = image.Height;
            byte[] result = new byte[width * height];
            int radius = size / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte minVal = 255;

                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            int nx = Math.Clamp(x + dx, 0, width - 1);
                            int ny = Math.Clamp(y + dy, 0, height - 1);
                            byte val = image.Pixels[ny * width + nx];
                            
                            if (val < minVal)
                                minVal = val;
                        }
                    }

                    result[y * width + x] = minVal;
                }
            }

            return new GrayscaleImage(width, height, result);
        }

        private static GrayscaleImage Open(GrayscaleImage image, int size)
        {
            // Opening = erosion followed by dilation
            var eroded = Erode(image, size);
            return Dilate(eroded, size);
        }

        private static GrayscaleImage Close(GrayscaleImage image, int size)
        {
            // Closing = dilation followed by erosion
            var dilated = Dilate(image, size);
            return Erode(dilated, size);
        }

        private static GrayscaleImage InvertImage(GrayscaleImage image)
        {
            byte[] inverted = new byte[image.Width * image.Height];
            
            for (int i = 0; i < inverted.Length; i++)
            {
                inverted[i] = (byte)(255 - image.Pixels[i]);
            }

            return new GrayscaleImage(image.Width, image.Height, inverted);
        }
    }
}