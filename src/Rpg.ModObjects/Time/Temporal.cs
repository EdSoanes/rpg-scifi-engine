using Newtonsoft.Json;

namespace Rpg.ModObjects.Time
{
    public class Temporal
    {
        [JsonProperty] public PointInTime Now { get; private set; } = new PointInTime(PointInTimeType.BeforeTime);

        public Temporal()
        { }

        public event NotifyTemporalEventHandler? OnTemporalEvent;

        public void TriggerEvent()
            => TriggerEvent(Now);

        public void TriggerEvent(PointInTime pointInTime)
        {
            Now = pointInTime;
            OnTemporalEvent?.Invoke(this, new TemporalEventArgs(Now));
        }

        public void TriggerEvent(PointInTimeType type, int count = 0)
        {
            Now = new PointInTime(type, count);
            OnTemporalEvent?.Invoke(this, new TemporalEventArgs(Now));
        }

        public void Transition(PointInTimeType type, int count = 0)
            => Transition(new PointInTime(type, type == PointInTimeType.Turn ? Math.Max(count, 1) : count));

        public void Transition(PointInTime to)
        {
            if (to == Now)
                return;

            if (!Now.IsEncounterTime && !to.IsEncounterTime && to < Now)
                throw new InvalidOperationException($"Cannot transition from '{Now}' to '{to}'");

            if (Now.IsEncounterTime && to.Type < PointInTimeType.TimePassing)
                throw new InvalidOperationException($"Cannot transition from '{Now}' to '{to}'");

            if (Now.Type == PointInTimeType.BeforeTime)
            {
                TriggerEvent(PointInTimeType.BeforeTime);
                TriggerEvent(PointInTimeType.TimeBegins);
                Transition(to);
                return;
            }

            if (Now.Type == PointInTimeType.TimeBegins)
            {
                TriggerEvent(PointInTimeType.TimePassing);
                Transition(to);
                return;
            }

            if (!to.IsEncounterTime && to.Type != PointInTimeType.EncounterEnds)
            {
                TriggerEvent(to);
                if (to.Type != PointInTimeType.TimeEnds && to.Type != PointInTimeType.TimePassing)
                    TriggerEvent(PointInTimeType.TimePassing);
                return;
            }

            if (to.Type == PointInTimeType.EncounterBegins)
            {
                if (Now.Type == PointInTimeType.Turn)
                    TriggerEvent(PointInTimeType.EncounterEnds);

                if (Now.Type == PointInTimeType.EncounterEnds)
                    TriggerEvent(PointInTimeType.TimePassing);

                TriggerEvent(PointInTimeType.EncounterBegins);
                TriggerEvent(PointInTimeType.Turn, 1);
                return;
            }

            if (to.Type == PointInTimeType.Turn)
            {
                if (Now.Type == PointInTimeType.EncounterEnds)
                    TriggerEvent(PointInTimeType.TimePassing);

                if (Now.Type == PointInTimeType.TimePassing)
                    TriggerEvent(PointInTimeType.EncounterBegins);

                if (Now.Type == PointInTimeType.EncounterBegins || Now.Type == PointInTimeType.Turn)
                    TriggerEvent(PointInTimeType.Turn, to.Count);

                return;
            }

            if (to.Type == PointInTimeType.EncounterEnds)
            {
                if (Now.Type == PointInTimeType.TimePassing)
                    TriggerEvent(PointInTimeType.EncounterBegins);

                if (Now.Type == PointInTimeType.EncounterBegins || Now.Type == PointInTimeType.Turn)
                    TriggerEvent(PointInTimeType.EncounterEnds);

                if (Now.Type == PointInTimeType.EncounterEnds)
                    TriggerEvent(PointInTimeType.TimePassing);

                return;
            }
        }
    }
}
