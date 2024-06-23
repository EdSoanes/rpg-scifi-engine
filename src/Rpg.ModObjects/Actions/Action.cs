using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;

namespace Rpg.ModObjects.Actions
{
    public abstract class Action
    {
        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string? OwnerId { get; private set; }
        [JsonProperty] public string? OwnerArchetype { get; private set; }

        [JsonProperty] private RpgMethod<Action, ModSet> OnCost { get; set; }
        [JsonProperty] private RpgMethod<Action, ModSet> OnAct { get; set; }
        [JsonProperty] private RpgMethod<Action, ModSet[]> OnOutcome { get; set; }

        [JsonConstructor] protected Action()
        {
            Id = this.NewId();
            Name = GetType().Name;

            OnCost = new RpgMethod<Action, ModSet>(this, nameof(OnCost));
            OnAct = new RpgMethod<Action, ModSet>(this, nameof(OnAct));
            OnOutcome = new RpgMethod<Action, ModSet[]>(this, nameof(OnOutcome));
        }

        public Action(RpgEntity owner)
            : this()
        {
            OwnerId = owner.Id;
            OwnerArchetype = owner.GetType().Name;
            Name = GetType().Name;
        }

        public abstract bool IsEnabled<TOwner>(TOwner owner, RpgEntity initiator)
            where TOwner : RpgEntity;

        public RpgArgSet CostArgs()
            => OnCost.Create();

        public ModSet Cost(RpgArgSet argSet)
            => OnCost.Execute(this, argSet);

        public RpgArgSet ActArgs() 
            => OnAct.Create();

        public ModSet Act(RpgArgSet args)
            => OnAct.Execute(this, args);

        public RpgArgSet OutcomeArgs() 
            => OnOutcome.Create();

        public ModSet[] Outcome(RpgArgSet args)
            => OnOutcome.Execute(this, args);
    }

    public abstract class Action<T> : Action
        where T : RpgEntity
    {
        [JsonConstructor] protected Action() { }

        public Action(T owner)
            : base(owner)
        { }
    }
}