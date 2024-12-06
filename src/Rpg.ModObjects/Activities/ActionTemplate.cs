using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Reflection.Args;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Activities
{
    public abstract class ActionTemplate
    {
        protected RpgGraph? Graph { get; set; }

        [JsonProperty] public string Id { get; protected set; }
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public string Classification { get; protected set; } = "Action";
        [JsonProperty] public string OwnerId { get; protected set; }
        [JsonProperty] public string OwnerArchetype { get; protected set; }
        [JsonProperty] public RpgArg[] ActionArgs { get; protected set; } = [];

        [JsonProperty] public RpgMethod<ActionTemplate, bool>? CanPerformMethod { get; private init; }
        [JsonProperty] public RpgMethod<ActionTemplate, bool>? CostMethod { get; private init; }
        [JsonProperty] public RpgMethod<ActionTemplate, bool>? PerformMethod { get; private init; }
        [JsonProperty] public RpgMethod<ActionTemplate, bool> OutcomeMethod { get; private init; }

        public bool IsPerformable
        {
            get
            {
                if (CanPerformMethod == null)
                    return true;

                var args = CanPerformMethod.Args
                    ?.CloneArgs()
                    .Fill(ActionArgs, Graph);

                if (!args.IsComplete())
                    return false;

                return CanPerformMethod.Execute(this, args.ToDictionary(Graph!));
            }
        }

        [JsonConstructor] protected ActionTemplate() { }

        public ActionTemplate(RpgEntity owner)
        {
            var type = GetType();
            Id = this.NewId();
            Name = type.Name;
            OwnerId = owner.Id;
            OwnerArchetype = GetOwnerArchetype()!;
            CanPerformMethod = RpgMethodFactory.Create<ActionTemplate, bool>(this, "CanPerform");
            CostMethod = RpgMethodFactory.Create<ActionTemplate, bool>(this, "Cost");
            PerformMethod = RpgMethodFactory.Create<ActionTemplate, bool>(this, "Perform");
            OutcomeMethod = RpgMethodFactory.Create<ActionTemplate, bool>(this, "Outcome")!;
        }

        //public Action CreateAction(RpgEntity owner, RpgEntity initiator, Activity activity)
        //{
        //    if (!owner.Archetypes.Contains(OwnerArchetype))
        //        throw new InvalidOperationException($"Cannot create Action of type {this.GetType().Name} for owner {owner.Archetype}");

        //    return new Action(this, owner, initiator, activity);
        //}

        public virtual void OnCreating(RpgGraph graph, RpgEntity owner)
        {
            Graph = graph;
            InitActionArgs();
        }

        public virtual void OnRestoring(RpgGraph graph)
            => Graph = graph;

        protected PerformResult CreatePerformResult()
            => new PerformResult();

        protected OutcomeResult CreateOutcomeResult(string modSetName)
            => new OutcomeResult
            {
                ModSet = new ModSet(OwnerId, modSetName)
            };

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

        protected ActionTemplate SetArg(string argName, Dice value)
        {
            var arg = ActionArgs.FirstOrDefault(x => x.Name == argName);
            arg?.SetValue(value);

            return this;
        }

        private void InitActionArgs()
        {
            var res = new List<RpgArg>(ActionArgs);

            InitActionArgs(res, "CanPerform", CanPerformMethod?.Args);
            InitActionArgs(res, "Cost", CostMethod?.Args);
            InitActionArgs(res, "Perform", PerformMethod?.Args);
            InitActionArgs(res, "Outcome", OutcomeMethod?.Args);

            var ownerArg = res.FirstOrDefault(x => x.Name == "owner" && x is RpgObjectArg);
            ownerArg?.SetValue(Graph!.GetObject<RpgEntity>(OwnerId));

            var initiatorArg = res.FirstOrDefault(x => x.Name == "initiator" && x is RpgObjectArg);
            initiatorArg?.SetValue(Graph!.GetInitiator<RpgEntity>());

            ActionArgs = res.ToArray();
        }

        private void InitActionArgs(List<RpgArg> res, string argGroup, RpgArg[]? args)
        {
            if (args != null)
            {
                foreach (var arg in args)
                {
                    var existing = res.FirstOrDefault(x => x.Name == arg.Name);

                    if (existing == null)
                    {
                        var clonedArg = arg.Clone();
                        clonedArg.Groups =  [argGroup];

                        res.Add(clonedArg);
                    }
                    else
                    {
                        existing.Groups = [.. existing.Groups, argGroup];
                    }
                }
            }
        }
    }

    public abstract class ActionTemplate<TOwner> : ActionTemplate
        where TOwner : RpgEntity
    {
        [JsonConstructor] protected ActionTemplate() 
            : base() { }

        public ActionTemplate(TOwner owner)
            : base(owner) { }

    }
}