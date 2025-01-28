using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Reflection.Args;

namespace Rpg.Experimental
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
        [JsonProperty] public RpgArg[] Args { get; protected set; } = [];

        public bool CanPerformArgsComplete { get => Args.IsComplete(ActionMethodNames.CanPerform); }
        public bool CostArgsComplete { get => Args.IsComplete(ActionMethodNames.Cost); }
        public bool PerformComplete { get => Args.IsComplete(ActionMethodNames.Perform); }
        public bool OutcomeComplete { get => Args.IsComplete(ActionMethodNames.Outcome); }

        [JsonConstructor] protected RpgAction() { }

        public RpgAction(RpgObject owner)
            : this(owner, false)
        {
            CanPerformMethod = RpgMethodFactory.Create<RpgAction, bool>(this, ActionMethodNames.CanPerform);
            CostMethod = RpgMethodFactory.Create<RpgAction, bool>(this, ActionMethodNames.Cost);
            PerformMethod = RpgMethodFactory.Create<RpgAction, bool>(this, ActionMethodNames.Perform);
            OutcomeMethod = RpgMethodFactory.Create<RpgAction, bool>(this, ActionMethodNames.Outcome)!;
        }

        protected RpgAction(RpgObject owner, bool syncToOwner)
            : base(owner.Id, syncToOwner)
        {
            Name = GetType().Name;
            OwnerArchetype = GetOwnerArchetype()!;
        }

        public override void OnCreating(RpgGraph graph, RpgObject? owner)
        {
            base.OnCreating(graph, owner);
            Args = RpgArg.CreateArgs(graph, CanPerformMethod, CostMethod, PerformMethod, OutcomeMethod);
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            base.OnTimeEvent(graph);

            RpgArg.SetValues(graph, Args, this);
            RpgArg.SetValues(graph, CanPerformMethod?.Args, Args);
            RpgArg.SetValues(graph, CostMethod?.Args, Args);
            RpgArg.SetValues(graph, PerformMethod?.Args, Args);
            RpgArg.SetValues(graph, OutcomeMethod?.Args, Args);

            OnTimeEventPerformable(graph);
        }

        public override RpgObject? ResolvePropertyNameToObject(RpgGraph graph, string prop)
        {
            var obj = base.ResolvePropertyNameToObject(graph, prop);
            if (obj == null)
            {
                return prop switch
                {
                    ActionReservedArgs.Action => this,
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

        private void OnTimeEventPerformable(RpgGraph graph)
        {
            if (CanPerformMethod == null)
                IsPerformable = true;

            else
            {
                var args = CanPerformMethod.Args
                    ?.CloneArgs()
                    .Fill(Args, graph);

                IsPerformable = !args.IsComplete()
                    ? false
                    : CanPerformMethod.Execute(this, RpgArg.CreateDictionary(graph, args));
            }
        }
    }

    public abstract class RpgAction<TOwner> : RpgAction
        where TOwner : RpgObject
    {
        [JsonConstructor]
        protected RpgAction()
            : base() { }

        public RpgAction(TOwner owner)
            : base(owner) { }
    }
}