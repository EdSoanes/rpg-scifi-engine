//using Rpg.ModObjects.Mods.Templates;

//namespace Rpg.ModObjects.Time
//{
//    public interface ITimeEngine
//    {
//        TimePoint Current { get; }

//        ModTemplate Create(string type = "turn");
//        void Begin();
//        void SetTime(TimePoint timePoint);
//        bool IncreaseTick();
//        bool DecreaseTick();
//        bool SetTick(int tick);

//        TimePoint CalculateStartTime(TimePoint delay);
//        TimePoint CalculateEndTime(TimePoint startTime, TimePoint duration);
//        LifecycleExpiry CalculateExpiry(TimePoint startTime, TimePoint endType);

//        event NotifyTimeEventHandler? OnTimeEvent;

//        void TriggerEvent();
//    }
//}