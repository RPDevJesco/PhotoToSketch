using System.Drawing;
using EventChainsCore;
using EdgeDetectionLib.Algorithms;
using ImageProcessing.Events;
using ImageProcessing.Middleware;

namespace ImageProcessing.Examples
{
    /// <summary>
    /// Demonstrates various ways to use the EventChain-based image processing system.
    /// </summary>
    public class ImageProcessingExamples
    {
        /// <summary>
        /// Example 1: Simple single-algorithm processing using the builder.
        /// </summary>
        public void Example1_SimpleProcessing(Bitmap sourceBitmap)
        {
            var builder = new ImageProcessingChainBuilder(sourceBitmap);
            
            // Process with a single algorithm
            var sketch = builder.ProcessWithSobel();
            
            sketch.Save("output_sobel.png");
            Console.WriteLine("Saved Sobel-processed sketch");
        }

        /// <summary>
        /// Example 2: Automatic region processing (recommended default).
        /// </summary>
        public void Example2_AutomaticRegions(Bitmap sourceBitmap)
        {
            var builder = new ImageProcessingChainBuilder(sourceBitmap);
            
            // Process with automatic region detection
            var sketch = builder.ProcessWithAutomaticRegions(blockSize: 32);
            
            sketch.Save("output_automatic.png");
            Console.WriteLine("Saved automatically-processed sketch");
        }

        /// <summary>
        /// Example 3: Manual chain construction with middleware.
        /// </summary>
        public void Example3_ManualChainWithMiddleware(Bitmap sourceBitmap)
        {
            var chain = EventChain.Strict()
                .UseMiddleware(ImageProcessingMiddleware.Timing())
                .UseMiddleware(ImageProcessingMiddleware.Logging(Console.WriteLine))
                .AddEvent(new LoadImageEvent(sourceBitmap))
                .AddEvent(new EdgeDetectionEvent(SobelProcessor.Apply, "Sobel"))
                .AddEvent(new PostProcessingEvent())
                .AddEvent(new ConvertToSketchEvent());

            var result = chain.ExecuteWithResults();

            if (result.Success)
            {
                var sketch = result.Context.Get<Bitmap>("sketch_bitmap");
                sketch.Save("output_with_middleware.png");

                // Get timing metrics
                var timings = result.Context.Get<List<TimingMetric>>("timing_metrics");
                Console.WriteLine("\nTiming Metrics:");
                foreach (var timing in timings)
                {
                    Console.WriteLine($"  {timing.EventName}: {timing.ElapsedMs}ms");
                }
            }
        }

        /// <summary>
        /// Example 4: Combining multiple algorithms with custom fault tolerance.
        /// </summary>
        public void Example4_MultipleAlgorithmsWithFaultTolerance(Bitmap sourceBitmap)
        {
            var chain = EventChain.Lenient() // Continue even if one algorithm fails
                .AddEvent(new LoadImageEvent(sourceBitmap))
                .AddEvent(new EdgeDetectionEvent(SobelProcessor.Apply, "Sobel", "edge_1"))
                .AddEvent(new EdgeDetectionEvent(DoGProcessor.Apply, "DoG", "edge_2"))
                .AddEvent(new EdgeDetectionEvent(PhaseCongruencyProcessor.Apply, "PhaseCongruency", "edge_3"))
                .AddEvent(new EdgeCombinationEvent(
                    new List<string> { "edge_1", "edge_2", "edge_3" },
                    "combined_edges",
                    EdgeCombinationEvent.CombinationMethod.WeightedAverage))
                .AddEvent(new PostProcessingEvent(
                    blurSigma: 1.0,
                    invert: true,
                    inputKey: "combined_edges"))
                .AddEvent(new ConvertToSketchEvent());

            var result = chain.ExecuteWithResults();

            Console.WriteLine($"Chain completed: {result.Success}");
            Console.WriteLine($"Events executed: {result.TotalCount}");
            Console.WriteLine($"Successes: {result.SuccessCount}");
            Console.WriteLine($"Failures: {result.FailureCount}");
            Console.WriteLine($"Total precision: {result.TotalPrecisionScore:F2}");
            Console.WriteLine($"Grade: {result.GetGrade()}");

            if (result.Context.ContainsKey("sketch_bitmap"))
            {
                var sketch = result.Context.Get<Bitmap>("sketch_bitmap");
                sketch.Save("output_multi_algorithm.png");
            }
        }

        /// <summary>
        /// Example 5: Progress reporting during processing.
        /// </summary>
        public void Example5_ProgressReporting(Bitmap sourceBitmap)
        {
            var totalEvents = 5; // LoadImage, EdgeDetection, PostProcessing, Convert, Extra

            var chain = EventChain.Strict()
                .UseMiddleware(ImageProcessingMiddleware.ProgressReporting(
                    totalEvents,
                    (current, total, eventName) =>
                    {
                        var percentage = (current * 100) / total;
                        Console.WriteLine($"[{percentage}%] Processing: {eventName}");
                    }))
                .AddEvent(new LoadImageEvent(sourceBitmap))
                .AddEvent(new EdgeDetectionEvent(SobelProcessor.Apply, "Sobel"))
                .AddEvent(new PostProcessingEvent())
                .AddEvent(new ConvertToSketchEvent());

            var result = chain.ExecuteWithResults();

            if (result.Success)
            {
                Console.WriteLine($"\nProcessing complete in {result.ExecutionTimeMs}ms");
                var sketch = result.Context.Get<Bitmap>("sketch_bitmap");
                sketch.Save("output_progress.png");
            }
        }

        /// <summary>
        /// Example 6: Using BestEffort mode for graduated precision.
        /// </summary>
        public void Example6_BestEffortGraduatedPrecision(Bitmap sourceBitmap)
        {
            var chain = EventChain.BestEffort() // Try all events, collect all results
                .UseMiddleware(ImageProcessingMiddleware.Timing())
                .UseMiddleware(ImageProcessingMiddleware.MemoryTracking())
                .AddEvent(new LoadImageEvent(sourceBitmap))
                .AddEvent(new AutomaticRegionProcessingEvent(blockSize: 32))
                .AddEvent(new PostProcessingEvent(
                    blurSigma: 1.0,
                    invert: true,
                    inputKey: "region_processed_image"))
                .AddEvent(new ConvertToSketchEvent());

            var result = chain.ExecuteWithResults();

            // BestEffort always succeeds if any events run
            Console.WriteLine($"Overall Success: {result.Success}");
            Console.WriteLine($"Total Precision Score: {result.TotalPrecisionScore:F2}");
            Console.WriteLine($"Quality Grade: {result.GetGrade()}");
            Console.WriteLine($"Execution Time: {result.ExecutionTimeMs}ms");

            // Detailed event results
            Console.WriteLine("\nEvent Details:");
            foreach (var eventResult in result.EventResults)
            {
                Console.WriteLine($"  {eventResult.EventName}:");
                Console.WriteLine($"    Success: {eventResult.Success}");
                Console.WriteLine($"    Precision: {eventResult.PrecisionScore:F2}");
                if (!eventResult.Success)
                {
                    Console.WriteLine($"    Error: {eventResult.ErrorMessage}");
                }
            }

            // Memory metrics
            if (result.Context.TryGet<List<MemoryMetric>>("memory_metrics", out var memoryMetrics))
            {
                Console.WriteLine("\nMemory Usage:");
                foreach (var metric in memoryMetrics)
                {
                    var deltaMB = metric.MemoryDeltaBytes / (1024.0 * 1024.0);
                    Console.WriteLine($"  {metric.EventName}: {deltaMB:F2} MB delta");
                }
            }

            if (result.Context.ContainsKey("sketch_bitmap"))
            {
                var sketch = result.Context.Get<Bitmap>("sketch_bitmap");
                sketch.Save("output_best_effort.png");
            }
        }

        /// <summary>
        /// Example 7: Custom continuation logic with retry.
        /// </summary>
        public void Example7_CustomContinuationWithRetry(Bitmap sourceBitmap)
        {
            // Custom logic: continue if precision score is above 50, otherwise fail
            var chain = EventChain.Custom((eventResult, context) =>
            {
                if (!eventResult.Success)
                {
                    return eventResult.PrecisionScore > 50.0; // Continue if partial success
                }
                return true; // Always continue on success
            })
                .UseMiddleware(ImageProcessingMiddleware.Retry(maxAttempts: 3, delayMs: 100))
                .UseMiddleware(ImageProcessingMiddleware.Logging(Console.WriteLine))
                .AddEvent(new LoadImageEvent(sourceBitmap))
                .AddEvent(new EdgeDetectionEvent(SobelProcessor.Apply, "Sobel"))
                .AddEvent(new PostProcessingEvent())
                .AddEvent(new ConvertToSketchEvent());

            var result = chain.ExecuteWithResults();

            // Set custom success criteria in context
            result.Context.Set("CustomSuccessCriteria", result.TotalPrecisionScore > 70.0);

            Console.WriteLine($"Custom Success: {result.Success}");
            Console.WriteLine($"Precision: {result.TotalPrecisionScore:F2}");
        }

        /// <summary>
        /// Example 8: Comparing multiple processing methods.
        /// </summary>
        public void Example8_CompareMultipleMethods(Bitmap sourceBitmap)
        {
            var builder = new ImageProcessingChainBuilder(sourceBitmap);

            var methods = new[]
            {
                ("Sobel", (Func<Bitmap>)builder.ProcessWithSobel),
                ("Prewitt", (Func<Bitmap>)builder.ProcessWithPrewitt),
                ("DoG", (Func<Bitmap>)builder.ProcessWithDoG),
                ("PhaseCongruency", (Func<Bitmap>)builder.ProcessWithPhaseCongruency),
                ("Automatic", (Func<Bitmap>)(() => builder.ProcessWithAutomaticRegions()))
            };

            Console.WriteLine("Comparing processing methods...\n");

            foreach (var (name, method) in methods)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    var result = method();
                    stopwatch.Stop();
                    
                    result.Save($"output_compare_{name.ToLower()}.png");
                    Console.WriteLine($"{name,-20} - Success in {stopwatch.ElapsedMilliseconds}ms");
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    Console.WriteLine($"{name,-20} - Failed: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Example 9: Conditional processing based on image characteristics.
        /// </summary>
        public void Example9_ConditionalProcessing(Bitmap sourceBitmap)
        {
            var chain = EventChain.Strict()
                .AddEvent(new LoadImageEvent(sourceBitmap))
                
                // Conditionally use different algorithms based on image size
                .AddEvent(new ConditionalEvent(
                    ctx => ctx.Get<int>("image_width") * ctx.Get<int>("image_height") < 1000000,
                    new EdgeDetectionEvent(PhaseCongruencyProcessor.Apply, "PhaseCongruency_Small"),
                    "small image"))
                
                .AddEvent(new ConditionalEvent(
                    ctx => ctx.Get<int>("image_width") * ctx.Get<int>("image_height") >= 1000000,
                    new AutomaticRegionProcessingEvent(64),
                    "large image"))
                
                .AddEvent(new PostProcessingEvent(
                    inputKey: "edge_detected_image",
                    blurSigma: 1.0,
                    invert: true))
                
                .AddEvent(new ConvertToSketchEvent());

            var result = chain.ExecuteWithResults();

            Console.WriteLine($"Conditional processing complete: {result.Success}");
            Console.WriteLine($"Image size: {result.Context.Get<int>("image_width")} x {result.Context.Get<int>("image_height")}");
            
            if (result.Success)
            {
                var sketch = result.Context.Get<Bitmap>("sketch_bitmap");
                sketch.Save("output_conditional.png");
            }
        }

        /// <summary>
        /// Example 10: Sub-chains for complex workflows.
        /// </summary>
        public void Example10_SubChains(Bitmap sourceBitmap)
        {
            // Create a sub-chain for edge detection
            var edgeDetectionChain = EventChain.Strict()
                .AddEvent(new EdgeDetectionEvent(SobelProcessor.Apply, "Sobel", "edge_1"))
                .AddEvent(new EdgeDetectionEvent(DoGProcessor.Apply, "DoG", "edge_2"))
                .AddEvent(new EdgeCombinationEvent(
                    new List<string> { "edge_1", "edge_2" },
                    "edge_detected_image"));

            // Main chain that uses the sub-chain
            var mainChain = EventChain.Strict()
                .UseMiddleware(ImageProcessingMiddleware.Logging(Console.WriteLine))
                .AddEvent(new LoadImageEvent(sourceBitmap))
                .AddEvent(new SubChainEvent(edgeDetectionChain, "EdgeDetection_SubChain"))
                .AddEvent(new PostProcessingEvent())
                .AddEvent(new ConvertToSketchEvent());

            var result = mainChain.ExecuteWithResults();

            Console.WriteLine($"\nMain chain with sub-chain complete: {result.Success}");
            Console.WriteLine($"Total events (including sub-chain): {result.TotalCount}");
            Console.WriteLine($"Total precision: {result.TotalPrecisionScore:F2}");

            if (result.Success)
            {
                var sketch = result.Context.Get<Bitmap>("sketch_bitmap");
                sketch.Save("output_subchain.png");
            }
        }
    }
}