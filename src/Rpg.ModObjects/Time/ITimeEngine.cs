namespace Rpg.ModObjects.Time
{
    public interface ITimeEngine
    {
        Time Current { get; }

        Time CalculateStartTime(Time delay);
        Time CalculateEndTime(Time startTime, Time duration);

        event NotifyTimeEventHandler? OnTimeEvent;

        void TriggerEvent();
    }
}