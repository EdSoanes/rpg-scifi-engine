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
        [JsonProperty] public string OwnerArchetype { get; private set; }

        [JsonProperty] internal RpgMethod<Action, ModSet> OnCost { get; private set; }
        [JsonProperty] internal RpgMethod<Action, ModSet> OnAct { get; private set; }
        [JsonProperty] internal RpgMethod<Action, ModSet[]> OnOutcome { get; private set; }

        [JsonConstructor] protected Action()
        {
            var type = GetType();

            Id = this.NewId();
            Name = type.Name;
            OwnerArchetype = type.BaseType!.IsGenericType ? type.BaseType!.GenericTypeArguments[0].Name : nameof(RpgEntity);
            OnCost = new RpgMethod<Action, ModSet>(this, nameof(OnCost));
            OnAct = new RpgMethod<Action, ModSet>(this, nameof(OnAct));
            OnOutcome = new RpgMethod<Action, ModSet[]>(this, nameof(OnOutcome));
        }

        public Action(RpgEntity owner)
            : this()
        {
            OwnerId = owner.Id;
            Name = GetType().Name;
        }

        public abstract bool IsEnabled<TOwner>(TOwner owner, RpgEntity initiator)
            where TOwner : RpgEntity;

        public RpgArgSet CostArgs()
            => OnCost.CreateArgSet();

        public ModSet Cost(RpgArgSet argSet)
            => OnCost.Execute(this, argSet);

        public RpgArgSet ActArgs() 
            => OnAct.CreateArgSet();

        public ModSet Act(RpgArgSet args)
            => OnAct.Execute(this, args);

        public RpgArgSet OutcomeArgs() 
            => OnOutcome.CreateArgSet();

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