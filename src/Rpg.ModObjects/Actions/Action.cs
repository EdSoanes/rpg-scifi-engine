using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Time.Templates;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Actions
{
    public abstract class Action
    {
        protected RpgGraph Graph { get; private set; }

        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string OwnerId { get; private set; }
        [JsonProperty] public string OwnerArchetype { get; private set; }

        [JsonProperty] internal RpgMethod<Action, bool> OnCanAct { get; private set; }
        [JsonProperty] internal RpgMethod<Action, ModSet> OnCost { get; private set; }
        [JsonProperty] internal RpgMethod<Action, ModSet[]> OnAct { get; private set; }
        [JsonProperty] internal RpgMethod<Action, ModSet[]> OnOutcome { get; private set; }

        [JsonConstructor] protected Action()
        {
            var type = GetType();

            Id = this.NewId();
            Name = type.Name;
            OwnerArchetype = type.OwnerArchetype() ?? nameof(RpgEntity);

            OnCanAct = new RpgMethod<Action, bool>(this, nameof(OnCanAct));
            OnCost = new RpgMethod<Action, ModSet>(this, nameof(OnCost));
            OnAct = new RpgMethod<Action, ModSet[]>(this, nameof(OnAct));
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

        internal string ActResultProp(int actionNo)
            => $"{GetType().Name}_ActResult_{actionNo}";

        internal string OutcomeResultProp(int actionNo)
            => $"{GetType().Name}_OutcomeResult_{actionNo}";

        internal Dice ActResult(RpgEntity initiator, int actionNo)
            => Graph.CalculatePropValue(initiator, ActResultProp(actionNo)) ?? Dice.Zero;

        internal Dice OutcomeResult(RpgEntity initiator, int actionNo)
            => Graph.CalculatePropValue(initiator, OutcomeResultProp(actionNo)) ?? Dice.Zero;

        protected void ActResult<TInitiator>(int actionNo, ModSet modSet, TInitiator initiator, string name, Dice dice)
            where TInitiator : RpgEntity
        {
            modSet.Add(new TurnMod().SetName(name), initiator, ActResultProp(actionNo), dice);
        }

        protected void ActResult<TInitiator, TSourceValue>(int actionNo, ModSet modSet, TInitiator initiator, Expression<Func<TInitiator, TSourceValue>> sourceExpr)
            where TInitiator : RpgEntity
        {
            modSet.Add(new TurnMod(), initiator, ActResultProp(actionNo), sourceExpr);
        }

        protected void ActResult<TInitiator>(int actionNo, ModSet modSet, TInitiator initiator, string sourceProp)
            where TInitiator : RpgEntity
        {
            modSet.Add(new TurnMod(), initiator, ActResultProp(actionNo), sourceProp);
        }

        protected void DiceRollMod<TInitiator, TSource, TSourceValue>(int actionNo, ModSet modSet, TInitiator initiator, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr)
            where TInitiator : RpgEntity
            where TSource : RpgObject
        {
            modSet.Add(new TurnMod(), initiator, ActResultProp(actionNo), source, sourceExpr);
        }

        protected void OutcomeMod<TInitiator>(int actionNo, ModSet modSet, TInitiator initiator, string name, Dice dice)
            where TInitiator : RpgEntity
        {
            modSet.Add(new TurnMod().SetName(name), initiator, OutcomeResultProp(actionNo), dice);
        }

        protected void OutcomeMod<TInitiator, TSourceValue>(int actionNo, ModSet modSet, TInitiator initiator, Expression<Func<TInitiator, TSourceValue>> sourceExpr)
            where TInitiator : RpgEntity
        {
            modSet.Add(new TurnMod(), initiator, OutcomeResultProp(actionNo), sourceExpr);
        }

        protected void OutcomeMod<TInitiator, TSource, TSourceValue>(int actionNo, ModSet modSet, TInitiator initiator, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr)
            where TInitiator : RpgEntity
            where TSource : RpgObject
        {
            modSet.Add(new TurnMod(), initiator, OutcomeResultProp(actionNo), source, sourceExpr);
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

        public ModSet[] Act(RpgArgSet args)
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