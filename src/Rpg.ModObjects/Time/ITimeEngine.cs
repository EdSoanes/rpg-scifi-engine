using Rpg.ModObjects.Mods;

namespace Rpg.ModObjects.Time
{
    public interface ITimeEngine
    {
        TimePoint Current { get; }

        ModTemplate Create(string? type = null);
        TimePoint CalculateStartTime(TimePoint delay);
        TimePoint CalculateEndTime(TimePoint startTime, TimePoint duration);

        event NotifyTimeEventHandler? OnTimeEvent;

        void TriggerEvent();
    }
}