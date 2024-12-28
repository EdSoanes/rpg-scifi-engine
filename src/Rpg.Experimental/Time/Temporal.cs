using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Rpg.Experimental.Time
{
    public class Temporal
    {
        [JsonProperty] public TimePoint Now { get; private set; } = new TimePoint(TimePointType.BeforeTime);

        public Temporal()
        { }

        public event NotifyTemporalEventHandler? OnTemporalEvent;

        public void BeginTime()
            => Transition(TimePointType.TimeBegins);

        public void Refresh()
            => TriggerEvent(Now);

        public void BeginEncounter()
            => Transition(TimePointType.EncounterBegins);

        public void ToTurn(int turn)
            => Transition(new TimePoint(TimePointType.Turn, turn));

        public void EndEncounter()
            => Transition(TimePointType.EncounterEnds);

        public void TimePasses(int count = 0)
            => Transition(new TimePoint(TimePointType.TimePasses, count));

        private void TriggerEvent(TimePoint pointInTime)
        {
            Now = pointInTime;
            OnTemporalEvent?.Invoke(this, new TemporalEventArgs(Now));
        }

        private void TriggerEvent(TimePointType type, int count = 0)
        {
            Now = new TimePoint(type, count);
            OnTemporalEvent?.Invoke(this, new TemporalEventArgs(Now));
        }

        //public void Transition(TimePointType type, int count = 0)
        //    => Transition(new TimePoint(type, type == TimePointType.Turn ? Math.Max(count, 1) : count));

        private void Transition(TimePoint to)
        {
            if (to == Now)
                return;

            if (!Now.IsEncounterTime && !to.IsEncounterTime && to < Now)
                throw new InvalidOperationException($"Cannot transition from '{Now}' to '{to}'");

            if (Now.IsEncounterTime && to.Type < TimePointType.Waiting)
                throw new InvalidOperationException($"Cannot transition from '{Now}' to '{to}'");

            if (Now.Type == TimePointType.BeforeTime)
                TriggerEvent(TimePointType.TimeBegins);

            if (Now.Type == TimePointType.TimeBegins && to.Type == TimePointType.TimeBegins)
            {
                TriggerEvent(TimePointType.Waiting);
                return;
            }

            if (!to.IsEncounterTime && to.Type != TimePointType.EncounterEnds)
            {
                if (to.Type == TimePointType.TimePasses)
                {
                    for (int i = 0; i <= to.Count; i++)
                        TriggerEvent(new TimePoint(TimePointType.TimePasses, i));
                }
                else
                {
                    TriggerEvent(to);
                }

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
