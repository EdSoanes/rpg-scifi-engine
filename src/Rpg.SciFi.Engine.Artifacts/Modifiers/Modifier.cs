using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class Modifier
    {
        [JsonConstructor] protected Modifier() { }

        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public PropRef Source { get; protected set; }
        [JsonProperty] public PropRef Target { get; protected set; }
        [JsonProperty] public ModifierDiceCalc DiceCalc { get; protected set; } = new ModifierDiceCalc();
        [JsonProperty] public ModifierType ModifierType { get; protected set; }
        [JsonProperty] public ModifierAction ModifierAction { get; protected set; } = ModifierAction.Accumulate;
        [JsonProperty] public bool IsPermanent { get; protected set; } = false;
        [JsonProperty] public int RemoveOnTurn { get; set; }

        protected static Modifier _Create<TMod, TEntity, T1, TEntity2, T2>(TEntity? entity, string? name, Dice? dice, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
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
            mod.Name = name ?? mod.Source.Prop ?? mod.Target.Prop ?? entity?.GetType().Name ?? target.Name;

            return mod;
        }

        protected static Modifier CreateByPath<TMod, T1>(ModdableObject entity, string name, Dice dice, string targetPropPath, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Modifier
        {
            var mod = Activator.CreateInstance<TMod>();

            mod.Source = PropRef.FromPath(entity, dice);
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

        public bool ShouldBeRemoved(int turn)
        {
            if (ModifierType == ModifierType.Transient)
            {
                if (RemoveOnTurn == RemoveTurn.WhenZero && Source.PropType == PropType.Dice && new Dice(Source.Prop).Roll() == 0)
                    return true;

                if (RemoveOnTurn == RemoveTurn.This)
                    return true;

                if (RemoveOnTurn == RemoveTurn.Encounter && turn == 0)
                    return true;

                return RemoveOnTurn > 0 && (RemoveOnTurn <= turn || turn == 0);
            }

            if (ModifierType == ModifierType.State)
            {
                //Do state stuff...
            }

            return false;
        }

        public bool CanBeCleared()
        {
            return !IsPermanent;
        }

        public override string ToString()
        {
            var modType = ModifierType switch
            {
                ModifierType.Base => "b",
                ModifierType.Custom => "c",
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

            var removeOnTurn = RemoveOnTurn.ToString();
            if (RemoveOnTurn == RemoveTurn.This)
                removeOnTurn = "t";
            else if (RemoveOnTurn == RemoveTurn.Encounter)
                removeOnTurn = "e";
            else if (RemoveOnTurn == RemoveTurn.WhenZero)
                removeOnTurn = "z";

            return $"({modType},{actionType},{removeOnTurn}) {Name} {Source.Prop} => {Target.Prop}";
        }
    }
}
