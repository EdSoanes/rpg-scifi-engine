using Newtonsoft.Json;

namespace Rpg.ModObjects.Time
{
    public class TurnBasedTimeEngine : ITimeEngine
    {
        [JsonProperty] public Time Current { get; private set; }

        public TurnBasedTimeEngine()
        {
            Current = new Time(TimeTypes.BeforeEncounter, TimeTypes.AsTurn(TimeTypes.BeforeEncounter));
        }

        public event NotifyTimeEventHandler? OnTimeEvent;

        public void TriggerEvent()
            => OnTimeEvent?.Invoke(this, new NotifyTimeEventEventArgs(Current));

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
