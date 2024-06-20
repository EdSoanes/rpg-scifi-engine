using Newtonsoft.Json;
using Rpg.ModObjects.Mods;

namespace Rpg.ModObjects.Time
{
    public class TurnBasedTimeEngine : ITimeEngine
    {
        public bool TimeHasBegun { get => Current.Type != nameof(BeforeTime);  }

        public TimePoint BeforeTime { get; private set; } = new(nameof(BeforeTime), int.MinValue);
        public TimePoint BeginningOfTime { get; private set; } = new(nameof(BeginningOfTime), int.MinValue);
        public TimePoint EndOfTime { get; private set; } = new(nameof(BeforeEncounter), int.MaxValue);

        public TimePoint BeforeEncounter = new(nameof(BeforeEncounter), -1);
        public TimePoint EncounterStart = new(nameof(EncounterStart), 0);
        public TimePoint Encounter = new(nameof(Encounter), 1);
        public TimePoint EncounterEnd = new(nameof(EncounterEnd), int.MaxValue - 1);


        [JsonProperty] public TimePoint Current { get; private set; }

        public TurnBasedTimeEngine()
        {
            Current = BeginningOfTime;
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

        public LifecycleExpiry CalculateExpiry(TimePoint startTime, TimePoint endTime)
        {
            if (startTime == BeginningOfTime && endTime == EndOfTime)
                return LifecycleExpiry.Active;

            if (endTime.Tick < Current.Tick)
            {
                return Current.Type == nameof(Encounter)
                    ? LifecycleExpiry.Expired
                    : LifecycleExpiry.Remove;
            }

            if (startTime.Tick > Current.Tick)
                return LifecycleExpiry.Pending;

            return LifecycleExpiry.Active;
        }

        public void Begin()
        {
            if (!TimeHasBegun)
            {
                TriggerEvent();

                Current = BeginningOfTime;
                TriggerEvent();
            }
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
