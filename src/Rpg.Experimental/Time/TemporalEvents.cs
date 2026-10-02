namespace Rpg.Experimental.Time
{
    public class TemporalEventArgs : EventArgs
    {
        public TimePoint Time { get; private set; }

        /// <summary>
        /// The name of the time event being delivered (e.g. "Sunrise"), or null when time simply moved
        /// </summary>
        public string? Event { get; private set; }

        public TemporalEventArgs(TimePoint time, string? eventName = null)
        {
            Time = time;
            Event = eventName;
        }
    }

    public delegate void NotifyTemporalEventHandler(object? sender, TemporalEventArgs e);
}
