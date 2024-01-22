using Newtonsoft.Json;
using System.Linq.Expressions;

namespace Rpg.Sys.Modifiers
{
    //TODO: Improved modifier lifecycle. Try to encode Start/End turns, Permanent, Conditional, etc into a more intuitive model.

    public abstract class Modifier
    {
        [JsonConstructor] protected Modifier() { }

        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public PropRef? Source { get; protected set; }
        [JsonProperty] public Dice? SourceDice { get; protected set; }
        [JsonProperty] public PropRef Target { get; protected set; }
        [JsonProperty] public ModifierDiceCalc DiceCalc { get; protected set; } = new ModifierDiceCalc();
        [JsonProperty] public ModifierType ModifierType { get; protected set; }
        [JsonProperty] public ModifierAction ModifierAction { get; protected set; } = ModifierAction.Accumulate;

        [JsonProperty] public ModifierDuration Duration { get; protected set; } = new ModifierDuration();

        protected static TMod _Create<TMod, TEntity, T1, TEntity2, T2>(int startTurn, int duration, TEntity? entity, string? name, Dice? dice, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Modifier
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
        {
            var mod = _Create<TMod, TEntity, T1, TEntity2, T2>(entity, name, dice, sourceExpr, target, targetExpr, diceCalcExpr);
            
            mod.Duration.SetOnTurnPeriod(startTurn, startTurn + duration);
            mod.ModifierType = ModifierType.Transient;

            return mod;
        }

        protected static TMod _Create<TMod, TEntity, T1, TEntity2, T2>(TEntity? entity, string? name, Dice? dice, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod: Modifier
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
        {
            var mod = Activator.CreateInstance<TMod>();
            if (entity != null && sourceExpr != null)
                mod.Source = PropRef.Create(entity, sourceExpr);
            else
                mod.SourceDice = dice;

            mod.Target = PropRef.Create(target, targetExpr);
            mod.DiceCalc.SetDiceCalc(diceCalcExpr);
            mod.Name = name ?? mod.Source?.Prop ?? mod.Target.Prop ?? entity?.GetType().Name ?? target.Name;

            return mod;
        }

        protected static TMod _CreateByPath<TMod>(ModdableObject entity, string? name, Dice dice, string targetPropPath, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Modifier
        {
            var mod = Activator.CreateInstance<TMod>();

            mod.SourceDice = dice;
            mod.Target = PropRef.Create(entity, targetPropPath);
            mod.DiceCalc.SetDiceCalc(diceCalcExpr);
            mod.Name = name ?? mod.Target.Prop ?? entity.GetType().Name;

            return mod;
        }

        public void SetDice(Dice dice)
        {
            SourceDice = dice;
            DiceCalc.Clear();
        }

        public virtual void OnAdd(int turn) { }
        public virtual void OnUpdate(int turn) { }

        public override string ToString()
        {
            var modType = ModifierType switch
            {
                ModifierType.Base => "b",
                ModifierType.State => "s",
                ModifierType.Transient => "t",
                ModifierType.BaseOverride => "p",
                _ => "_"
            };

            var actionType = ModifierAction switch
            {
                ModifierAction.Replace => "r",
                ModifierAction.Sum => "s",
                ModifierAction.Accumulate => "a",
                _ => "_"
            };

            var removeOnTurn = Duration.EndTurn.ToString();
            if (Duration.EndTurn == RemoveTurn.EndOfTurn)
                removeOnTurn = "t";
            else if (Duration.EndTurn == RemoveTurn.Encounter)
                removeOnTurn = "e";
            else if (Duration.EndTurn == RemoveTurn.WhenZero)
                removeOnTurn = "z";

            return $"({modType},{actionType},{removeOnTurn}) {Name} {Source.Prop} => {Target.Prop}";
        }
    }
}
