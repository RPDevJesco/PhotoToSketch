using EventChainsCore;
using System.Diagnostics;

namespace ImageProcessing.Middleware
{
    /// <summary>
    /// Collection of useful middleware for image processing chains.
    /// </summary>
    public static class ImageProcessingMiddleware
    {
        /// <summary>
        /// Logs detailed information about each event execution.
        /// </summary>
        public static Func<EventChain.ExecuteDelegate, EventChain.ExecuteDelegate> Logging(
            Action<string> logger)
        {
            return next => (evt, ctx) =>
            {
                var eventName = evt.GetType().Name;
                logger($"[START] {eventName}");
                
                var result = next(evt, ctx);
                
                var status = result.Success ? "SUCCESS" : "FAILURE";
                logger($"[{status}] {eventName} - Precision: {result.PrecisionScore:F2}");
                
                if (!result.Success)
                {
                    logger($"[ERROR] {eventName} - {result.ErrorMessage}");
                }
                
                return result;
            };
        }

        /// <summary>
        /// Times each event execution and stores metrics in context.
        /// </summary>
        public static Func<EventChain.ExecuteDelegate, EventChain.ExecuteDelegate> Timing()
        {
            return next => (evt, ctx) =>
            {
                var stopwatch = Stopwatch.StartNew();
                var result = next(evt, ctx);
                stopwatch.Stop();

                var eventName = evt.GetType().Name;
                ctx.Append("timing_metrics", new TimingMetric
                {
                    EventName = eventName,
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Success = result.Success
                });

                return result;
            };
        }

        /// <summary>
        /// Validates that required context keys exist before event execution.
        /// </summary>
        public static Func<EventChain.ExecuteDelegate, EventChain.ExecuteDelegate> RequiredKeys(
            params string[] requiredKeys)
        {
            return next => (evt, ctx) =>
            {
                foreach (var key in requiredKeys)
                {
                    if (!ctx.ContainsKey(key))
                    {
                        return EventResult.CreateFailure(
                            evt.GetType().Name,
                            $"Required context key '{key}' is missing");
                    }
                }

                return next(evt, ctx);
            };
        }

        /// <summary>
        /// Implements retry logic for failed events.
        /// </summary>
        public static Func<EventChain.ExecuteDelegate, EventChain.ExecuteDelegate> Retry(
            int maxAttempts = 3,
            int delayMs = 100)
        {
            return next => (evt, ctx) =>
            {
                EventResult? result = null;
                
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    result = next(evt, ctx);
                    
                    if (result.Success)
                        return result;

                    if (attempt < maxAttempts)
                    {
                        Thread.Sleep(delayMs);
                    }
                }

                return result!;
            };
        }

        /// <summary>
        /// Caches the results of events based on a cache key function.
        /// Useful for avoiding redundant processing.
        /// </summary>
        public static Func<EventChain.ExecuteDelegate, EventChain.ExecuteDelegate> Caching(
            Func<IChainableEvent, IEventContext, string> getCacheKey,
            Dictionary<string, EventResult> cache)
        {
            return next => (evt, ctx) =>
            {
                var cacheKey = getCacheKey(evt, ctx);
                
                if (cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return cachedResult;
                }

                var result = next(evt, ctx);
                
                if (result.Success)
                {
                    cache[cacheKey] = result;
                }

                return result;
            };
        }

        /// <summary>
        /// Validates precision scores meet a minimum threshold.
        /// </summary>
        public static Func<EventChain.ExecuteDelegate, EventChain.ExecuteDelegate> MinimumPrecision(
            double minimumScore)
        {
            return next => (evt, ctx) =>
            {
                var result = next(evt, ctx);
                
                if (result.Success && result.PrecisionScore < minimumScore)
                {
                    return EventResult.CreatePartialSuccess(
                        result.EventName,
                        $"Precision score {result.PrecisionScore:F2} below minimum {minimumScore}",
                        result.PrecisionScore,
                        result.Data);
                }

                return result;
            };
        }

        /// <summary>
        /// Tracks memory usage before and after each event.
        /// </summary>
        public static Func<EventChain.ExecuteDelegate, EventChain.ExecuteDelegate> MemoryTracking()
        {
            return next => (evt, ctx) =>
            {
                var beforeMemory = GC.GetTotalMemory(false);
                var result = next(evt, ctx);
                var afterMemory = GC.GetTotalMemory(false);

                var memoryDelta = afterMemory - beforeMemory;
                ctx.Append("memory_metrics", new MemoryMetric
                {
                    EventName = evt.GetType().Name,
                    MemoryDeltaBytes = memoryDelta,
                    TotalMemoryAfter = afterMemory
                });

                return result;
            };
        }

        /// <summary>
        /// Provides progress updates during chain execution.
        /// </summary>
        public static Func<EventChain.ExecuteDelegate, EventChain.ExecuteDelegate> ProgressReporting(
            int totalEvents,
            Action<int, int, string> onProgress)
        {
            int currentEvent = 0;

            return next => (evt, ctx) =>
            {
                currentEvent++;
                var eventName = evt.GetType().Name;
                onProgress(currentEvent, totalEvents, eventName);

                return next(evt, ctx);
            };
        }
    }

    public class TimingMetric
    {
        public string EventName { get; set; } = "";
        public long ElapsedMs { get; set; }
        public bool Success { get; set; }
    }

    public class MemoryMetric
    {
        public string EventName { get; set; } = "";
        public long MemoryDeltaBytes { get; set; }
        public long TotalMemoryAfter { get; set; }
    }
}