using Newtonsoft.Json;

namespace Rpg.Experimental.Time
{
    public class Temporal
    {
        [JsonProperty] public TimePoint Now { get; private set; } = new TimePoint(TimePointType.BeforeTime);

        public Temporal()
        { }

        public event NotifyTemporalEventHandler? OnTemporalEvent;

        public void TriggerEvent()
            => TriggerEvent(Now);

        public void TriggerEvent(TimePoint pointInTime)
        {
            Now = pointInTime;
            OnTemporalEvent?.Invoke(this, new TemporalEventArgs(Now));
        }

        public void TriggerEvent(TimePointType type, int count = 0)
        {
            Now = new TimePoint(type, count);
            OnTemporalEvent?.Invoke(this, new TemporalEventArgs(Now));
        }

        public void Transition(TimePointType type, int count = 0)
            => Transition(new TimePoint(type, type == TimePointType.Turn ? Math.Max(count, 1) : count));

        public void Transition(TimePoint to)
        {
            if (to == Now)
                return;

            if (!Now.IsEncounterTime && !to.IsEncounterTime && to < Now)
                throw new InvalidOperationException($"Cannot transition from '{Now}' to '{to}'");

            if (Now.IsEncounterTime && to.Type < TimePointType.Waiting)
                throw new InvalidOperationException($"Cannot transition from '{Now}' to '{to}'");

            if (Now.Type == TimePointType.BeforeTime)
            {
                TriggerEvent(TimePointType.BeforeTime);
                TriggerEvent(TimePointType.TimeBegins);
                Transition(to);
                return;
            }

            if (Now.Type == TimePointType.TimeBegins)
            {
                TriggerEvent(TimePointType.Waiting);
                Transition(to);
                return;
            }

            if (!to.IsEncounterTime && to.Type != TimePointType.EncounterEnds)
            {
                TriggerEvent(to);
                if (to.Type != TimePointType.TimeEnds && to.Type != TimePointType.Waiting)
                    TriggerEvent(TimePointType.Waiting);
                return;
            }

            if (to.Type == TimePointType.EncounterBegins)
            {
                if (Now.Type == TimePointType.Turn)
                    TriggerEvent(TimePointType.EncounterEnds);

                if (Now.Type == TimePointType.EncounterEnds)
                    TriggerEvent(TimePointType.Waiting);

                TriggerEvent(TimePointType.EncounterBegins);
                TriggerEvent(TimePointType.Turn, 1);
                return;
            }

            if (to.Type == TimePointType.Turn)
            {
                if (Now.Type == TimePointType.EncounterEnds)
                    TriggerEvent(TimePointType.Waiting);

                if (Now.Type == TimePointType.Waiting)
                    TriggerEvent(TimePointType.EncounterBegins);

                if (Now.Type == TimePointType.EncounterBegins || Now.Type == TimePointType.Turn)
                    TriggerEvent(TimePointType.Turn, to.Count);

                return;
            }

            if (to.Type == TimePointType.EncounterEnds)
            {
                if (Now.Type == TimePointType.Waiting)
                    TriggerEvent(TimePointType.EncounterBegins);

                if (Now.Type == TimePointType.EncounterBegins || Now.Type == TimePointType.Turn)
                    TriggerEvent(TimePointType.EncounterEnds);

                if (Now.Type == TimePointType.EncounterEnds)
                    TriggerEvent(TimePointType.Waiting);

                return;
            }
        }
    }

}
