using Newtonsoft.Json;
using Rpg.ModObjects.Mods;

namespace Rpg.ModObjects.Time
{
    public class TurnBasedTimeEngine : ITimeEngine
    {
        public TimePoint BeginningOfTime { get; private set; } = new(nameof(BeforeEncounter), int.MinValue);
        public TimePoint EndOfTime { get; private set; } = new(nameof(BeforeEncounter), int.MaxValue);

        public static TimePoint BeforeEncounter = new(nameof(BeforeEncounter), -1);
        public static TimePoint EncounterStart = new(nameof(EncounterStart), 0);
        public static TimePoint Encounter = new(nameof(Encounter), 1);
        public static TimePoint EncounterEnd = new(nameof(EncounterEnd), int.MaxValue - 1);

        [JsonProperty] public TimePoint Current { get; private set; }

        public TurnBasedTimeEngine()
        {
            Current = BeforeEncounter;
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
            if (delay.Type == nameof(EncounterStart))
                return EncounterStart;

            if (delay.Type == nameof(EncounterEnd))
                return EncounterEnd;

            if (delay.Type == nameof(Encounter))
                return new TimePoint(nameof(Encounter), Current.Tick + delay.Tick);

            return BeforeEncounter;
        }

        public TimePoint CalculateEndTime(TimePoint startTime, TimePoint duration)
        {
            if (duration == EndOfTime)
                return EndOfTime;

            return new TimePoint(nameof(Encounter), startTime.Tick + duration.Tick - 1);
        }

        public ModExpiry CalculateExpiry(TimePoint startTime, TimePoint endTime)
        {
            if (startTime == BeginningOfTime && endTime == EndOfTime)
                return ModExpiry.Active;

            if (endTime.Tick < Current.Tick)
            {
                return Current.Type == nameof(Encounter)
                    ? ModExpiry.Expired
                    : ModExpiry.Remove;
            }

            if (startTime.Tick > Current.Tick)
                return ModExpiry.Pending;

            return ModExpiry.Active;
        }

        public void NewEncounter()
        {
            if (Current.Type == Encounter.Type)
                EndEncounter();

            if (Current.Type != EncounterStart.Type)
            {
                Current = EncounterStart;
                TriggerEvent();
            }

            NewTurn();
        }

        public void EndEncounter()
        {
            Current = EncounterEnd;
            TriggerEvent();
        }

        public void NewTurn()
        {
            Current = new TimePoint(nameof(Encounter), Current.Tick + 1);
            TriggerEvent();
        }

        public void PrevTurn()
        {
            if (Current.Tick > 1)
            {
                Current = new TimePoint(nameof(Encounter), Current.Tick - 1);
                TriggerEvent();
            }
        }

        public void SetTurn(int turn)
        {
            if (Current.Type == nameof(Encounter) && turn > 0 && turn < EncounterEnd.Tick)
            {
                Current = new TimePoint(nameof(Encounter), turn);
                TriggerEvent();
            }
        }
    }
}
