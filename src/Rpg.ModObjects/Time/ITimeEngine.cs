using Rpg.ModObjects.Mods;

namespace Rpg.ModObjects.Time
{
    public interface ITimeEngine
    {
        TimePoint Current { get; }
        bool TimeHasBegun { get; }
        TimePoint BeforeTime { get; }
        TimePoint BeginningOfTime { get; }
        TimePoint EndOfTime { get; }
        ModTemplate Create(string? type = null);
        TimePoint CalculateStartTime(TimePoint delay);
        TimePoint CalculateEndTime(TimePoint startTime, TimePoint duration);

        event NotifyTimeEventHandler? OnTimeEvent;

        void TriggerEvent();
    }
}