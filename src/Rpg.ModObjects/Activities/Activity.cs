using Newtonsoft.Json;
using Rpg.ModObjects.Reflection.Args;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Activities
{
    public class Activity : RpgObject
    {
        private RpgEntity? Owner { get => Graph?.GetObject<RpgEntity>(OwnerId); }

        [JsonProperty] public int ActivityNo { get; protected set; }
        internal int NextActionNo { get => Actions.Length; }

        public Action[] Actions { get => Graph?.GetObjects<Action>().Where(x => x.OwnerId == Id).ToArray() ?? []; }

        public bool CanAutoComplete { get => Actions.All(x => x.Status == ActionStatus.CanAutoComplete); }
        [JsonProperty] public int CurrentActionNo
        {
            get
            {
                for (int i = 0; i < Actions.Length; i++)
                    if (!Actions[i].IsComplete) return i;
                
                return Actions.Length - 1;
            }
        }

        [JsonConstructor] protected Activity() { }

        public Activity(RpgEntity initiator, string name, int activityNo)
            : base(initiator.Id)
        {
            SetLifespan(new Lifespan(0, 1));
            Name = $"{name}/{activityNo}";
            ActivityNo = activityNo;
        }

        public Action? CurrentAction()
        {
            return CurrentActionNo >= Actions.Count() - 1 && CurrentActionNo < Actions.Count()
                ? Actions[CurrentActionNo]
                : null;
        }

        #region Action Execution

        public RpgArg[] CostArgs()
            => CurrentAction()?.CostArgs() ?? [];

        public bool Cost(RpgArg[] args)
            => CurrentAction()?.Cost(args) ?? false;

        public RpgArg[] PerformArgs()
            => CurrentAction()?.PerformArgs() ?? [];

        public bool Perform(RpgArg[] args)
            => CurrentAction()?.Perform(args) ?? false;

        public RpgArg[] OutcomeArgs()
            => CurrentAction()?.OutcomeArgs() ?? [];

        public bool Outcome(RpgArg[] args)
            => CurrentAction()?.Outcome(args) ?? false;

        public ActionRef[] Complete()
            => CurrentAction()?.Complete() ?? [];

        public ActionRef[] AutoComplete(params (string, object?)[]? args)
            => CurrentAction()?.AutoComplete(args) ?? [];
        
        public void Reset()
            => CurrentAction()?.Reset();

        #endregion Action Execution

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            foreach (var action in Actions)
                action.OnCreating(graph, this);

            Lifespan = graph.Time.Now.IsEncounterTime
                ? new Lifespan(graph.Time.Now.Count, 1)
                : new Lifespan(graph.Time.Now, PointInTimeType.TimePasses);
        }

        public override void OnRestoring(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnRestoring(graph, entity);
            foreach (var action in Actions)
                action.OnRestoring(graph);
        }
    }
}
