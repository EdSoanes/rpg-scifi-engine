using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;

namespace Rpg.ModObjects.Time
{
    public class TurnBasedTimeEngine : ITimeEngine
    {
        public Time BeginningOfTime { get; private set; } = new(TimeTypes.BeforeEncounter, int.MinValue);
        public Time EndOfTime { get; private set; } = new(TimeTypes.BeforeEncounter, int.MaxValue);

        public static Time BeforeEncounter = new(TimeTypes.BeforeEncounter, -1);
        public static Time EncounterStart = new(TimeTypes.EncounterStart, 0);
        public static Time Encounter = new(TimeTypes.Encounter, 1);
        public static Time EncounterEnd = new(TimeTypes.EncounterEnd, int.MaxValue - 1);

        [JsonProperty] public Time Current { get; private set; }

        public TurnBasedTimeEngine()
        {
            Current = new Time(TimeTypes.BeforeEncounter, TimeTypes.AsTurn(TimeTypes.BeforeEncounter));
        }

        public event NotifyTimeEventHandler? OnTimeEvent;

        public void TriggerEvent()
            => OnTimeEvent?.Invoke(this, new NotifyTimeEventEventArgs(Current));

        public Time CalculateStartTime(Time delay)
        {
            if (delay.Type == TimeTypes.EncounterStart)
                return EncounterStart;

            if (delay.Type == TimeTypes.EncounterEnd)
                return EncounterEnd;

            if (delay.Type == TimeTypes.Encounter)
                return new Time(TimeTypes.Encounter, Current.Tick + delay.Tick);

            return BeforeEncounter;
        }

        public Time CalculateEndTime(Time startTime, Time duration)
        {
            if (duration == EndOfTime)
                return EndOfTime;

            return new Time(TimeTypes.Encounter, startTime.Tick + duration.Tick - 1);
        }

        public ModExpiry CalculateExpiry(Time startTime, Time endTime)
        {
            if (startTime == BeginningOfTime && endTime == EndOfTime)
                return ModExpiry.Active;

            if (endTime.Tick < Current.Tick)
            {
                return Current.Type == TimeTypes.Encounter
                    ? ModExpiry.Expired
                    : ModExpiry.Remove;
            }

            if (startTime.Tick > Current.Tick)
                return ModExpiry.Pending;

            return ModExpiry.Active;
        }

        public void NewEncounter()
        {
            if (Current.Type == TimeTypes.Encounter)
                EndEncounter();

            if (Current.Type != TimeTypes.EncounterStart)
            {
                Current = new Time(TimeTypes.EncounterStart, TimeTypes.AsTurn(TimeTypes.EncounterStart));
                TriggerEvent();
            }

            NewTurn();
        }

        public void EndEncounter()
        {
            Current = new Time(TimeTypes.EncounterEnd, TimeTypes.AsTurn(TimeTypes.EncounterEnd));
            TriggerEvent();
        }

        public void NewTurn()
        {
            Current = new Time(TimeTypes.Encounter, Current.Tick + 1);
            TriggerEvent();
        }

        public void PrevTurn()
        {
            if (Current.Tick > 1)
            {
                Current = new Time(TimeTypes.Encounter, Current.Tick - 1);
                TriggerEvent();
            }
        }

        public void SetTurn(int turn)
        {
            if (Current.Type == TimeTypes.Encounter && turn > 0 && turn < TimeTypes.AsTurn(TimeTypes.EncounterEnd))
            {
                Current = new Time(TimeTypes.Encounter, turn);
                TriggerEvent();
            }
        }
    }
}
