using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Reflection.Args;

namespace Rpg.Experimental.Activities
{
    public abstract class RpgAction : RpgObject
    {
        [JsonProperty] public string Classification { get; protected set; } = "Action";
        [JsonProperty] public string OwnerArchetype { get; protected set; }
        [JsonProperty] public bool IsPerformable { get; protected set; }

        [JsonProperty] public RpgMethod<RpgAction, bool>? CanPerformMethod { get; protected init; }
        [JsonProperty] public RpgMethod<RpgAction, bool>? CostMethod { get; protected init; }
        [JsonProperty] public RpgMethod<RpgAction, bool>? PerformMethod { get; protected init; }
        [JsonProperty] public RpgMethod<RpgAction, bool> OutcomeMethod { get; protected init; }
        [JsonProperty] public RpgArg[] ActionArgs { get; protected set; } = [];

        public bool CanPerformArgsComplete { get => ActionArgs.IsComplete(MethodNames.CanPerform); }
        public bool CostArgsComplete { get => ActionArgs.IsComplete(MethodNames.Cost); }
        public bool PerformComplete { get => ActionArgs.IsComplete(MethodNames.Perform); }
        public bool OutcomeComplete { get => ActionArgs.IsComplete(MethodNames.Outcome); }

        [JsonConstructor] protected RpgAction() { }

        public RpgAction(RpgObject owner)
            : this(owner, false)
        {
            CanPerformMethod = RpgMethodFactory.Create<RpgAction, bool>(this, MethodNames.CanPerform);
            CostMethod = RpgMethodFactory.Create<RpgAction, bool>(this, MethodNames.Cost);
            PerformMethod = RpgMethodFactory.Create<RpgAction, bool>(this, MethodNames.Perform);
            OutcomeMethod = RpgMethodFactory.Create<RpgAction, bool>(this, MethodNames.Outcome)!;
        }

        protected RpgAction(RpgObject owner, bool syncToOwner)
            : base(owner.Id, syncToOwner)
        {
            Name = GetType().Name;
            OwnerArchetype = GetOwnerArchetype()!;
        }

        public override void OnCreating(RpgGraph graph, RpgObject? owner)
        {
            var actionArgs = new List<RpgArg>(ActionArgs);

            OnCreatingProperties(graph, actionArgs, MethodNames.CanPerform, CanPerformMethod?.Args);
            OnCreatingProperties(graph, actionArgs, MethodNames.Cost, CostMethod?.Args);
            OnCreatingProperties(graph, actionArgs, MethodNames.Perform, PerformMethod?.Args);
            OnCreatingProperties(graph, actionArgs, MethodNames.Outcome, OutcomeMethod?.Args);

            ActionArgs = actionArgs.ToArray();

            base.OnCreating(graph, owner);
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            base.OnTimeEvent(graph);
            OnTimeEventArgs(graph);
            OnTimeEventPerformable(graph);
        }

        public override RpgObject? ResolvePropertyNameToObject(RpgGraph graph, string prop)
        {
            var obj = base.ResolvePropertyNameToObject(graph, prop);
            if (obj == null)
            {
                return prop switch
                {
                    ReservedArgs.Action => this,
                    _ => null
                };
            }

            return obj;
        }

        protected string? GetOwnerArchetype()
        {
            var actionType = GetType();
            while (actionType != null)
            {
                if (actionType.IsGenericType)
                {
                    var genericTypes = actionType.GetGenericArguments();
                    if (genericTypes.Length == 1)
                        return genericTypes[0].Name;
                }

                actionType = actionType.BaseType;
            }

            return null;
        }

        private void OnCreatingProperties(RpgGraph graph, List<RpgArg> res, string argGroup, RpgArg[]? args)
        {
            if (args == null) return;
            foreach (var arg in args)
            {
                var propData = graph.CreateVirtualProperty(Id, arg);
                var clonedArg = res.FirstOrDefault(x => x.Name == arg.Name);
                if (clonedArg == null)
                {
                    clonedArg = arg.Clone();
                    res.Add(clonedArg);
                }

                clonedArg.Groups = [.. clonedArg.Groups, argGroup];
            }
        }

        private void OnTimeEventArgs(RpgGraph graph)
        {
            foreach (var arg in ActionArgs)
            {
                var val = graph.GetPropertyData(Id, arg.Name)?.GetValue<object?>(graph);
                if (val != null)
                    arg.SetValue(val);
            }
        }

        private void OnTimeEventPerformable(RpgGraph graph)
        {
            if (CanPerformMethod == null)
                IsPerformable = true;

            else
            {
                var args = CanPerformMethod.Args
                    ?.CloneArgs()
                    .Fill(ActionArgs, graph);

                IsPerformable = !args.IsComplete()
                    ? false
                    : CanPerformMethod.Execute(this, args.ToDictionary(graph));
            }
        }
    }

    public abstract class RpgAction<TOwner> : RpgAction
        where TOwner : RpgObject
    {
        [JsonConstructor] protected RpgAction() 
            : base() { }

        public RpgAction(TOwner owner)
            : base(owner) { }
    }
}