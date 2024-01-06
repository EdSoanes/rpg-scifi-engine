using Newtonsoft.Json;
using System.Linq.Expressions;

namespace Rpg.Sys.Modifiers
{
    public abstract class Modifier
    {
        [JsonConstructor] protected Modifier() { }

        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public PropRef Source { get; protected set; }
        [JsonProperty] public PropRef Target { get; protected set; }
        [JsonProperty] public ModifierDiceCalc DiceCalc { get; protected set; } = new ModifierDiceCalc();
        [JsonProperty] public ModifierType ModifierType { get; protected set; }
        [JsonProperty] public ModifierAction ModifierAction { get; protected set; } = ModifierAction.Accumulate;
        [JsonProperty] public ModifierExpiry Expiry { get; protected set; } = ModifierExpiry.Active;
        [JsonProperty] public int StartTurn { get; protected set; } = int.MinValue;
        [JsonProperty] public int EndTurn { get; protected set; } = int.MaxValue;

        protected static TMod _Create<TMod, TEntity, T1, TEntity2, T2>(int startTurn, int duration, TEntity? entity, string? name, Dice? dice, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Modifier
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
        {
            var mod = _Create<TMod, TEntity, T1, TEntity2, T2>(entity, name, dice, sourceExpr, target, targetExpr, diceCalcExpr);
            mod.StartTurn = startTurn;
            mod.EndTurn = startTurn + duration;
            mod.ModifierType = ModifierType.Transient;
            return mod;
        }

        protected static TMod _Create<TMod, TEntity, T1, TEntity2, T2>(TEntity? entity, string? name, Dice? dice, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod: Modifier
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
        {
            var mod = Activator.CreateInstance<TMod>();
            mod.Source = entity != null && sourceExpr != null
                ? PropRef.FromPath(entity, sourceExpr, true)
                : PropRef.FromDice(dice);

            mod.Target = PropRef.FromPath(target, targetExpr);
            mod.DiceCalc.SetDiceCalc(diceCalcExpr);
            mod.Name = name ?? (mod.Source.PropType == Sys.PropType.Path ? mod.Source.Prop : null) ?? mod.Target.Prop ?? entity?.GetType().Name ?? target.Name;

            return mod;
        }

        protected static TMod _CreateByPath<TMod>(ModdableObject entity, string? name, Dice dice, string targetPropPath, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Modifier
        {
            var mod = Activator.CreateInstance<TMod>();

            mod.Source = PropRef.FromDice(dice);
            mod.Target = PropRef.FromPath(entity, targetPropPath);
            mod.DiceCalc.SetDiceCalc(diceCalcExpr);
            mod.Name = name ?? mod.Target.Prop ?? entity.GetType().Name;

            return mod;
        }

        public void SetDice(Dice dice)
        {
            Source = PropRef.FromDice(Source.RootId, Source.Id, dice);
            DiceCalc.Clear();
        }

        public void Expire(int turn)
        {
            EndTurn = turn;
        }

        public virtual void OnAdd(int turn) { }
        public virtual void OnUpdate(int turn) 
        {
            if (EndTurn < int.MaxValue)
            {
                if (turn < 1 || StartTurn > EndTurn)
                    Expiry = ModifierExpiry.Remove;
                else if (StartTurn > turn)
                    Expiry = ModifierExpiry.Pending;
                else if (EndTurn < turn)
                    Expiry = ModifierExpiry.Expired;
                else
                    Expiry = ModifierExpiry.Active;
            }
            else
                Expiry = ModifierExpiry.Active;
        }

        public bool CanBeCleared()
        {
            return Expiry != ModifierExpiry.Active;
        }

        public override string ToString()
        {
            var modType = ModifierType switch
            {
                ModifierType.Base => "b",
                ModifierType.State => "s",
                ModifierType.Transient => "t",
                ModifierType.Player => "p",
                _ => "_"
            };

            var actionType = ModifierAction switch
            {
                ModifierAction.Replace => "r",
                ModifierAction.Sum => "s",
                ModifierAction.Accumulate => "a",
                _ => "_"
            };

            var removeOnTurn = EndTurn.ToString();
            if (EndTurn == RemoveTurn.EndOfTurn)
                removeOnTurn = "t";
            else if (EndTurn == RemoveTurn.Encounter)
                removeOnTurn = "e";
            else if (EndTurn == RemoveTurn.WhenZero)
                removeOnTurn = "z";

            return $"({modType},{actionType},{removeOnTurn}) {Name} {Source.Prop} => {Target.Prop}";
        }
    }
}
