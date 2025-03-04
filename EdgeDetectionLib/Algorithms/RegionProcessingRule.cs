using System;
using System.Drawing;
using EdgeDetectionLib.Core;

namespace EdgeDetectionLib.Algorithms
{
    public class RegionProcessingRule
    {
        public Rectangle Region { get; }
        public Func<GrayscaleImage, GrayscaleImage> Processor { get; }

        public RegionProcessingRule(Rectangle region, Func<GrayscaleImage, GrayscaleImage> processor)
        {
            Region = region;
            Processor = processor;
        }
    }
}