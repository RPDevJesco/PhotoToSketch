namespace EventChainsCore
{
    /// <summary>
    /// A conditional event that only executes if a context condition is met.
    /// Useful for branching logic in chains.
    /// </summary>
    public class ConditionalEvent : BaseEvent
    {
        private readonly Func<IEventContext, bool> _condition;
        private readonly IChainableEvent _innerEvent;
        private readonly string _conditionDescription;

        public ConditionalEvent(
            Func<IEventContext, bool> condition,
            IChainableEvent innerEvent,
            string conditionDescription = "condition")
        {
            _condition = condition;
            _innerEvent = innerEvent;
            _conditionDescription = conditionDescription;
        }
        
        public override EventResult Execute(IEventContext context)
        {
            if (_condition(context))
            {
                return _innerEvent.Execute(context);
            }

            // Condition not met - skip this event
            return Success(new { Skipped = true, Reason = $"Condition '{_conditionDescription}' not met" }, 100.0);
        }
    }
}
