using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;

namespace Rpg.ModObjects.Actions
{
    public abstract class Action
    {
        protected RpgGraph Graph { get; private set; }

        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string OwnerId { get; private set; }
        [JsonProperty] public string OwnerArchetype { get; private set; }

        [JsonProperty] public RpgMethod<Action, bool> OnCanAct { get; private set; }
        [JsonProperty] public RpgMethod<Action, ModSet> OnCost { get; private set; }
        [JsonProperty] public RpgMethod<Action, ActionModSet> OnAct { get; private set; }
        [JsonProperty] public RpgMethod<Action, ModSet[]> OnOutcome { get; private set; }

        [JsonConstructor] protected Action()
        {
            var type = GetType();

            Id = this.NewId();
            Name = type.Name;
            OwnerArchetype = type.OwnerArchetype() ?? nameof(RpgEntity);

            OnCanAct = new RpgMethod<Action, bool>(this, nameof(OnCanAct));
            OnCost = new RpgMethod<Action, ModSet>(this, nameof(OnCost));
            OnAct = new RpgMethod<Action, ActionModSet>(this, nameof(OnAct));
            OnOutcome = new RpgMethod<Action, ModSet[]>(this, nameof(OnOutcome));
        }

        public Action(RpgEntity owner)
            : this()
        {
            OwnerId = owner.Id;
            Name = GetType().Name;
        }

        public void OnBeforeTime(RpgGraph graph)
        {
            Graph = graph;
        }

        public RpgArgSet CanActArgs()
            => OnCanAct.CreateArgSet();

        public RpgArgSet CostArgs()
            => OnCost.CreateArgSet();

        public bool CanAct(RpgArgSet argSet)
            => OnCanAct.Execute(this, argSet);

        public ModSet Cost(RpgArgSet argSet)
            => OnCost.Execute(this, argSet);

        public RpgArgSet ActArgs() 
            => OnAct.CreateArgSet();

        public ActionModSet Act(RpgArgSet args)
            => OnAct.Execute(this, args);

        public RpgArgSet OutcomeArgs() 
            => OnOutcome.CreateArgSet();

        public ModSet[] Outcome(RpgArgSet args)
            => OnOutcome.Execute(this, args);
    }

    public abstract class Action<TOwner> : Action
        where TOwner : RpgEntity
    {
        [JsonConstructor] protected Action() { }

        public Action(TOwner owner)
            : base(owner)
        { }
    }
}