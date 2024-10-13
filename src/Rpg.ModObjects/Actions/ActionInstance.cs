using Rpg.ModObjects.Reflection.Args;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Actions
{
    public class ActionInstance
    {
        private RpgGraph? Graph { get; set; }

        private RpgEntity? _owner;
        [JsonIgnore] public RpgEntity? Owner 
        { 
            get
            {
                if (_owner == null && Graph != null)
                    _owner = Graph.GetObject<RpgEntity>(OwnerId);

                return _owner;
            }
        }

        private Action? _action;
        [JsonIgnore] public Action? Action
        {
            get
            {
                if (_action == null && Graph != null)
                    _action = Owner?.GetAction(ActionName);

                return _action;
            }
        }

        [JsonProperty] public string OwnerId { get; protected set; }
        [JsonProperty] public string ActionName { get; protected set; }
        [JsonProperty] public int ActionNo { get; protected set; }

        [JsonProperty] public Dictionary<string, object?> CanActArgs { get; protected set; } = new();
        [JsonProperty] public Dictionary<string, object?> CostArgs { get; protected set; } = new();
        [JsonProperty] public Dictionary<string, object?> ActArgs { get; protected set; } = new();
        [JsonProperty] public Dictionary<string, object?> OutcomeArgs { get; protected set; } = new();
        [JsonProperty] public Dictionary<string, RpgArg> Args { get; protected set; } = new();

        public ActionInstance(RpgEntity owner, Action action, int actionNo)
        {
            ActionName = action.Name;
            ActionNo = actionNo;
            OwnerId = owner.Id;
        }

        public void OnBeforeTime(RpgGraph graph)
        {
            Graph = graph;

            FillArgs(CanActArgs, Action!.OnCanAct.Args);
            FillArgs(CostArgs, Action!.OnCost.Args);
            FillArgs(ActArgs, Action!.OnAct.Args);
            FillArgs(OutcomeArgs, Action!.OnOutcome.Args);

            Merge(Args, Action!.OnCanAct.Args);
            Merge(Args, Action!.OnCost.Args);
            Merge(Args, Action!.OnAct.Args);
            Merge(Args, Action!.OnOutcome.Args);
        }

        private void Merge(Dictionary<string, RpgArg> args, IEnumerable<RpgArg> other)
        {
            foreach (var otherArg in other)
            {
                if (!args.ContainsKey(otherArg.Name))
                    args.Add(otherArg.Name, otherArg);
            }
        }

        private void FillArgs(Dictionary<string, object?> args, IEnumerable<RpgArg> other)
        {
            foreach (var otherArg in other)
            {
                if (!args.ContainsKey(otherArg.Name))
                    args.Add(otherArg.Name, null);
            }
        }

        public object? SetCanAct(string arg, object? value)
        {
            var canActArg = Action!.OnCanAct.Args.FirstOrDefault(x => x.Name == arg);
            if (canActArg != null)
            {

                var val = canActArg.FromInput(Graph!, value);
                if (val != null)
                    CanActArgs[arg] = val;

                return val;
            }

            return null;
        }

        public object? SetCost(string arg, object? value)
        {
            var costArg = Action!.OnCost.Args.FirstOrDefault(x => x.Name == arg);
            if (costArg != null)
            {
                var val = costArg.FromInput(Graph!, value);
                if (val != null)
                    CostArgs[arg] = val;

                return val;
            }

            return null;
        }

        public object? SetAct(string arg, object? value)
        {
            var actArg = Action!.OnAct.Args.FirstOrDefault(x => x.Name == arg);
            if (actArg != null)
            {
                var val = actArg.FromInput(Graph!, value);
                if (val != null)
                    ActArgs[arg] = val;

                return val;
            }

            return null;
        }

        public object? SetOutcome(string arg, object? value)
        {
            var outcomeArg = Action!.OnOutcome.Args.First(x => x.Name == arg);
            if (outcomeArg != null)
            {
                var val = outcomeArg.FromInput(Graph!, value);
                if (val != null)
                    OutcomeArgs[arg] = val;

                return val;
            }

            return null;
        }
    }
}
