using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Json;
using Rpg.Experimental.System;
using Rpg.Experimental.Time;

namespace Rpg.Experimental
{
    public class RpgCharacterSheet : RpgGraph
    {
        public const int DefaultMaxTurnHistory = 5;

        private bool _restoringSnapshot;
        private int _maxTurnHistory = DefaultMaxTurnHistory;

        [JsonProperty] public RpgObject Actor { get; private set; }

        /// <summary>
        /// The state of the sheet at the start of each turn since turn tracking began, keyed by turn number.
        /// This is what makes it possible to go back to an earlier turn.
        /// </summary>
        [JsonProperty] internal Dictionary<int, RpgTurnSnapshot> TurnSnapshots { get; private set; } = new();

        /// <summary>
        /// How many turns can be gone back to, counting the current one. Each turn kept is a whole copy of
        /// the sheet, so this bounds how large a saved sheet gets in a long fight. Older turns are dropped
        /// as new ones start. Zero keeps no history. It is a setting of the sheet: it is saved with it and
        /// going back a turn does not change it.
        /// </summary>
        [JsonProperty] public int MaxTurnHistory
        {
            get => _maxTurnHistory;
            set
            {
                _maxTurnHistory = Math.Max(0, value);
                TrimTurnHistory();
            }
        }

        public RpgCharacterSheet(RpgObject context, RpgSystem? metaGraph = null)
            : base(context, metaGraph)
        {
            Actor = context;
            Time.Refresh();
        }

        public RpgCharacterSheet(RpgObject context, RpgObject actor, RpgSystem? rpgSystem = null)
            : base(context, rpgSystem)
        {
            Actor = actor;

            //The graph was built before the actor was known. Refresh so anything that depends on the actor
            //(e.g. whether actions can be performed) is evaluated
            Time.Refresh();
        }

        public RpgCharacterSheet(RpgCharacterSheetState characterSheetState, RpgSystem rpgSystem)
            : base(characterSheetState, rpgSystem)
        {
            Actor = (RpgObject)characterSheetState.Objects.First(x => x.Id == characterSheetState.ActorId);
            TurnSnapshots = characterSheetState.TurnSnapshots ?? new();
            MaxTurnHistory = characterSheetState.MaxTurnHistory;
            Time.Refresh();
        }

        /// <summary>
        /// The whole sheet as compressed text, for storing on a device or handing to another one
        /// </summary>
        public string Save()
            => RpgJson.SerializeSnapshot(GetState());

        /// <summary>
        /// Restore a sheet from the text given by Save()
        /// </summary>
        public static RpgCharacterSheet Load(string saved, RpgSystem rpgSystem)
            => new RpgCharacterSheet(RpgJson.DeserializeSnapshot<RpgCharacterSheetState>(saved), rpgSystem);

        public RpgCharacterSheetState GetState()
            => GetState(true);

        private RpgCharacterSheetState GetState(bool includeTurnSnapshots)
        {
            var characterSheetState = new RpgCharacterSheetState
            {
                Objects = Objects.Values.ToList(),
                ObjectData = ObjectData.Values.ToList(),
                ContextId = Context.Id,
                ActorId = Actor.Id,
                Time = Time,
                RollMode = RollMode,
                MaxTurnHistory = MaxTurnHistory,
                TurnSnapshots = includeTurnSnapshots ? TurnSnapshots : new()
            };

            return characterSheetState;
        }

        /// <summary>
        /// The turns that RewindToTurn() can go back to
        /// </summary>
        public int[] GetRewindableTurns()
            => TurnSnapshots.Keys.Order().ToArray();

        /// <summary>
        /// Go back to the start of a turn. Everything is as it was then: values, states, completed actions,
        /// where items were, changes made by hand. The later turns are discarded, so replaying a turn means
        /// doing it again.
        ///
        /// The objects of the sheet are replaced, so any object obtained before the rewind must be fetched
        /// again (e.g. with GetObject()).
        /// </summary>
        /// <returns>False if the turn cannot be gone back to. Nothing is changed.</returns>
        public bool RewindToTurn(int turn)
        {
            if (!TurnSnapshots.TryGetValue(turn, out var snapshot))
                return false;

            _restoringSnapshot = true;
            try
            {
                var state = RpgJson.DeserializeSnapshot<RpgCharacterSheetState>(snapshot.Data);

                //Who rolls is a setting of the sheet. It is not part of what happened in a turn.
                state.RollMode = RollMode;

                RestoreState(state);
                Actor = (RpgObject)state.Objects.First(x => x.Id == state.ActorId);

                //The turns may have been renumbered since the snapshot was taken
                if (snapshot.TurnOffset != 0)
                    base.RenumberTurn(Time.Turn + snapshot.TurnOffset);

                foreach (var laterTurn in TurnSnapshots.Keys.Where(x => x > turn).ToArray())
                    TurnSnapshots.Remove(laterTurn);
            }
            finally
            {
                _restoringSnapshot = false;
            }

            Time.Refresh();
            return true;
        }

        public override void RenumberTurn(int turn)
        {
            if (!Time.IsTurnTracking)
                return;

            var offset = turn - Time.Turn;
            if (offset == 0)
                return;

            //No new turn has started, so no new snapshot
            var restoring = _restoringSnapshot;
            _restoringSnapshot = true;
            try
            {
                base.RenumberTurn(turn);
            }
            finally
            {
                _restoringSnapshot = restoring;
            }

            TurnSnapshots = TurnSnapshots.ToDictionary(
                x => x.Key + offset,
                x => new RpgTurnSnapshot { Data = x.Value.Data, TurnOffset = x.Value.TurnOffset + offset });
        }

        protected override void OnAfterTemporalEvent(TemporalEventArgs e)
        {
            base.OnAfterTemporalEvent(e);

            //Still being constructed
            if (Actor == null || _restoringSnapshot)
                return;

            if (!Time.IsTurnTracking)
            {
                //Ending turn tracking settles what happened. There is nothing to go back to.
                if (TurnSnapshots.Any() && (Time.Now.Type == TimePointType.Waiting || Time.Now.Type == TimePointType.EncounterEnds))
                    TurnSnapshots.Clear();

                return;
            }

            if (Time.Now.Type == TimePointType.Turn && !TurnSnapshots.ContainsKey(Time.Turn))
                TakeTurnSnapshot();
        }

        internal override void OnTurnStateChanged()
        {
            if (Actor != null && !_restoringSnapshot && Time.Now.Type == TimePointType.Turn)
                TakeTurnSnapshot();
        }

        private void TakeTurnSnapshot()
        {
            if (MaxTurnHistory <= 0)
                return;

            TurnSnapshots[Time.Turn] = new RpgTurnSnapshot
            {
                Data = RpgJson.SerializeSnapshot(GetState(false))
            };

            TrimTurnHistory();
        }

        /// <summary>
        /// Drop the oldest turns beyond the number that are kept
        /// </summary>
        private void TrimTurnHistory()
        {
            if (TurnSnapshots == null)
                return;

            foreach (var turn in TurnSnapshots.Keys.OrderDescending().Skip(MaxTurnHistory).ToArray())
                TurnSnapshots.Remove(turn);
        }
    }
}
