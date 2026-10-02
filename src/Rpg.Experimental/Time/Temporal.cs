using Newtonsoft.Json;

namespace Rpg.Experimental.Time
{
    /// <summary>
    /// Time has two modes.
    ///
    /// Turn tracking (an encounter) is precise. It is counted in turns and only ends when told to.
    ///
    /// Outside turn tracking time is loose. It moves forward through time events: the built in "time passes"
    /// and any named events the game system defines (e.g. sunrise). A time event can also happen during
    /// turn tracking, which does not end it.
    /// </summary>
    public class Temporal
    {
        /// <summary>
        /// The name of the built in time event. It means the story has moved on and nothing in particular is
        /// happening. Its purpose is to give effects a fuzzy duration: they last for as long as something is
        /// going on, then end. Every named time event also counts as time passing.
        /// 
        /// Counting turns is not time passing in this sense. Starting turn tracking, a turn tick and ending
        /// turn tracking do not end something that lasts "until time passes".
        /// </summary>
        public const string TimePassesEvent = "TimePasses";

        [JsonProperty] public TimePoint Now { get; private set; } = new TimePoint(TimePointType.BeforeTime);

        /// <summary>
        /// The number of time events that have happened
        /// </summary>
        [JsonProperty] public int EventSequence { get; private set; }

        /// <summary>
        /// The name of the most recent time event
        /// </summary>
        [JsonProperty] public string? LastEvent { get; private set; }

        /// <summary>
        /// The name of the time event that is being delivered right now, otherwise null. Rules can use this
        /// in OnTimeEvent() to react to an event.
        /// </summary>
        [JsonIgnore] public string? CurrentEvent { get; private set; }

        [JsonIgnore] public bool IsTurnTracking { get => Now.IsEncounterTime; }
        [JsonIgnore] public int Turn { get => Now.Type == TimePointType.Turn ? Now.Count : 0; }

        public Temporal()
        { }

        public event NotifyTemporalEventHandler? OnTemporalEvent;

        public bool IsEvent(string eventName)
            => CurrentEvent == eventName;

        public void BeginTime()
            => Transition(TimePointType.TimeBegins);

        public void Refresh()
            => TriggerEvent(Now);

        public void BeginEncounter()
            => Transition(TimePointType.EncounterBegins);

        /// <summary>
        /// Start tracking turns at the given turn number. Does nothing if turns are already being tracked.
        /// </summary>
        public void BeginTurnTracking(int turn = 1)
        {
            if (!IsTurnTracking)
                Transition(new TimePoint(TimePointType.Turn, turn));
        }

        /// <summary>
        /// Move to a turn. Moving forward several turns passes through each turn in order.
        /// </summary>
        public void ToTurn(int turn)
            => Transition(new TimePoint(TimePointType.Turn, turn));

        public void NextTurn()
            => ToTurn(Turn + 1);

        public void EndEncounter()
            => Transition(TimePointType.EncounterEnds);

        /// <summary>
        /// The built in time event. Outside turn tracking time moves on. During turn tracking the event is
        /// delivered without ending turn tracking.
        /// </summary>
        public void TimePasses(int count = 0)
        {
            if (IsTurnTracking)
            {
                for (int i = 0; i <= count; i++)
                    DeliverEvent(TimePassesEvent);

                return;
            }

            Transition(new TimePoint(TimePointType.TimePasses, count));
        }

        /// <summary>
        /// A named time event, e.g. "Sunrise". It also counts as time passing.
        /// </summary>
        public void RaiseEvent(string eventName)
        {
            if (eventName == TimePassesEvent)
            {
                TimePasses();
                return;
            }

            if (IsTurnTracking)
            {
                DeliverEvent(eventName);
                return;
            }

            if (Now.Type == TimePointType.BeforeTime)
                BeginTime();

            //Lifespans that start or end at the event are resolved first, then time passes
            DeliverEvent(eventName);

            //One event, so it only counts once
            var sequence = EventSequence;
            Transition(new TimePoint(TimePointType.TimePasses, 0));
            EventSequence = sequence;
            LastEvent = eventName;
        }

        /// <summary>
        /// Change the number of the current turn without any time passing. The graph is responsible for
        /// shifting everything that is measured in turns.
        /// </summary>
        internal void RenumberTurn(int turn)
        {
            if (Now.Type == TimePointType.Turn)
                Now = new TimePoint(TimePointType.Turn, turn);
        }

        private void DeliverEvent(string eventName)
        {
            EventSequence++;
            LastEvent = eventName;
            CurrentEvent = eventName;
            try
            {
                OnTemporalEvent?.Invoke(this, new TemporalEventArgs(Now, eventName));
            }
            finally
            {
                CurrentEvent = null;
            }
        }

        private void TriggerEvent(TimePoint pointInTime)
        {
            Now = pointInTime;

            if (Now.Type == TimePointType.TimePasses)
            {
                EventSequence++;
                LastEvent = TimePassesEvent;
            }

            OnTemporalEvent?.Invoke(this, new TemporalEventArgs(Now));
        }

        private void TriggerEvent(TimePointType type, int count = 0)
            => TriggerEvent(new TimePoint(type, count));

        private void Transition(TimePoint to)
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
            }

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

                if (Now.Type == TimePointType.TimeBegins || Now.Type == TimePointType.Waiting)
                    TriggerEvent(TimePointType.EncounterBegins);

                if (Now.Type == TimePointType.EncounterBegins)
                {
                    //Turn tracking can start at any turn number
                    TriggerEvent(TimePointType.Turn, to.Count);
                }
                else if (Now.Type == TimePointType.Turn)
                {
                    if (to.Count > Now.Count)
                    {
                        //Moving forward several turns gives the same result as moving one turn at a time
                        for (int turn = Now.Count + 1; turn <= to.Count; turn++)
                            TriggerEvent(TimePointType.Turn, turn);
                    }
                    else
                    {
                        TriggerEvent(TimePointType.Turn, to.Count);
                    }
                }

                return;
            }

            if (to.Type == TimePointType.EncounterEnds)
            {
                if (Now.Type == TimePointType.TimeBegins || Now.Type == TimePointType.Waiting)
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
