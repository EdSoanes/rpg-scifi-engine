using Newtonsoft.Json;
using Rpg.ModObjects.Mods;

namespace Rpg.ModObjects.Time
{
    public partial class TimePoints
    {
        public static TimePoint BeforeEncounter = new TimePoint(nameof(BeforeEncounter), -1);
        public static TimePoint BeginningOfEncounter = new TimePoint(nameof(BeginningOfEncounter), 0);
        public static TimePoint Encounter(int turn)
        {
            if (turn < 1)
                turn = 1;

            if (turn >= EndOfEncounter.Tick)
                turn = EndOfEncounter.Tick - 1;

            return new TimePoint(nameof(Encounter), turn);
        }

        public static TimePoint EndOfEncounter = new TimePoint(nameof(EndOfEncounter), int.MaxValue - 1);
    }

    public class TurnBasedTimeEngine : ITimeEngine
    {
        [JsonProperty] public TimePoint Current { get; private set; }

        public TurnBasedTimeEngine()
        {
            Current = TimePoints.BeforeTime;
        }

        public event NotifyTimeEventHandler? OnTimeEvent;

        public void TriggerEvent()
            => OnTimeEvent?.Invoke(this, new NotifyTimeEventEventArgs(Current));

        public ModTemplate Create(string timeType = "turn")
        {
            ModTemplate template = timeType switch
            {
                "encounter" => new EncounterMod(),
                _ => new TurnMod()
            };

            return template;
        }

        public TimePoint CalculateStartTime(TimePoint delay)
        {
            if (delay == TimePoints.Empty || delay.Type == nameof(TimePoints.Encounter))
                return TimePoints.Encounter(Current.Tick + delay.Tick);

            return delay;
        }

        public TimePoint CalculateEndTime(TimePoint startTime, TimePoint duration)
        {
            if (duration.Type != nameof(TimePoints.Encounter))
                return duration;

            return TimePoints.Encounter(startTime.Tick + duration.Tick - 1);
        }

        public ModExpiry CalculateExpiry(TimePoint startTime, TimePoint endTime)
        {
            if (startTime == TimePoints.BeginningOfTime && endTime == TimePoints.EndOfTime)
                return ModExpiry.Active;

            if (endTime.Tick < Current.Tick)
            {
                return Current.Type == nameof(TimePoints.Encounter)
                    ? ModExpiry.Expired
                    : ModExpiry.Remove;
            }

            if (startTime.Tick > Current.Tick)
                return ModExpiry.Pending;

            return ModExpiry.Active;
        }

        public void NewEncounter()
        {
            if (Current.Type == nameof(TimePoints.Encounter))
                EndEncounter();

            if (Current != TimePoints.BeginningOfEncounter)
            {
                Current = TimePoints.BeginningOfEncounter;
                TriggerEvent();
            }

            NewTurn();
        }

        public void EndEncounter()
        {
            Current = TimePoints.EndOfEncounter;
            TriggerEvent();
        }

        public void NewTurn()
        {
            Current = TimePoints.Encounter(Current.Tick + 1);
            TriggerEvent();
        }

        public void PrevTurn()
        {
            if (Current.Tick > 1)
            {
                Current = TimePoints.Encounter(Current.Tick - 1);
                TriggerEvent();
            }
        }

        public void SetTurn(int turn)
        {
            if (Current.Type == nameof(TimePoints.Encounter) && turn > 0 && turn < TimePoints.EndOfEncounter.Tick)
            {
                Current = TimePoints.Encounter(turn);
                TriggerEvent();
            }
        }
    }
}
