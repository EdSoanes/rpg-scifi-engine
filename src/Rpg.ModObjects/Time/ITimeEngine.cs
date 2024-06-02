namespace Rpg.ModObjects.Time
{
    public interface ITimeEngine
    {
        Time Current { get; }

        event NotifyTimeEventHandler? OnTimeEvent;

        void TriggerEvent();
    }
}