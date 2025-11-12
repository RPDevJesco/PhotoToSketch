namespace EventChainsCore
{
    /// <summary>
    /// Base class for events that provides common functionality and naming.
    /// </summary>
    public abstract class BaseEvent : IChainableEvent
    {
        /// <summary>
        /// Gets the name of this event, used for result tracking and debugging.
        /// Defaults to the class name but can be overridden.
        /// </summary>
        public virtual string EventName
        {
            get { return GetType().Name; }
        }

        /// <summary>
        /// Executes the event logic and returns a result.
        /// </summary>
        public abstract EventResult Execute(IEventContext context);

        /// <summary>
        /// Helper method to create a success result for this event.
        /// </summary>
        protected EventResult Success(object data, double precisionScore)
        {
            return EventResult.CreateSuccess(EventName, data, precisionScore);
        }

        /// <summary>
        /// Helper method to create a failure result for this event.
        /// </summary>
        protected EventResult Failure(string message, double precisionScore)
        {
            return EventResult.CreateFailure(EventName, message, precisionScore);
        }

        /// <summary>
        /// Helper method to create a partial success result for this event.
        /// </summary>
        protected EventResult PartialSuccess(string message, double precisionScore, object data)
        {
            return EventResult.CreatePartialSuccess(EventName, message, precisionScore, data);
        }
    }
}