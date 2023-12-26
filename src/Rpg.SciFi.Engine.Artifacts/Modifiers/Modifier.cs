using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection.Metadata;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class Modifier
    {
        [JsonProperty] private int? Resolution { get; set; }

        [JsonConstructor] protected Modifier() { }

        protected Modifier(string? name, Dice? dice, ModdableProperty? source, ModdableProperty target, string? diceCalc, ModifierType modifierType, ModifierAction modifierAction, bool isPermanent)
        {
            Name = name ?? source?.Source ?? target.Prop ?? GetType().Name;
            Dice = dice;
            Target = target;
            DiceCalc = diceCalc;
            ModifierType = modifierType;
            ModifierAction = modifierAction;
            IsPermanent = isPermanent;
        }

        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public Dice? Dice { get; protected set; }
        [JsonProperty] public ModdableProperty? Source { get; protected set; } = null;
        [JsonProperty] public ModdableProperty Target { get; protected set; }
        [JsonProperty] public string? DiceCalc { get; protected set; }
        [JsonProperty] public ModifierType ModifierType { get; protected set; }
        [JsonProperty] public ModifierAction ModifierAction { get; protected set; } = ModifierAction.Accumulate;
        [JsonProperty] public bool IsPermanent { get; protected set; } = false;
        [JsonProperty] public int RemoveOnTurn { get; set; }

        protected static Modifier _Create<TMod, TEntity, T1, TEntity2, T2>(TEntity? entity, string? name, Dice? dice, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod: Modifier
            where TEntity : Entity
            where TEntity2 : Entity
        {
            var mod = Activator.CreateInstance<TMod>();
            mod.Dice = dice;
            mod.Source = entity != null && sourceExpr != null
                ? ModdableProperty.Create(entity, sourceExpr, true)
                : null;

            mod.Target = ModdableProperty.Create(target, targetExpr);
            mod.DiceCalc = ReflectionEngine.GetDiceCalcFunction(diceCalcExpr);
            mod.Name = name ?? mod.Source?.Source ?? mod.Target.Prop ?? entity?.GetType().Name ?? target.Name;

            return mod;
        }

        protected static Modifier CreateByPath<TMod, T1>(Entity entity, string name, Dice dice, string targetPropPath, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Modifier
        {
            var mod = Activator.CreateInstance<TMod>();

            mod.Target = ModdableProperty.Create(entity, targetPropPath);
            mod.DiceCalc = ReflectionEngine.GetDiceCalcFunction(diceCalcExpr);
            mod.Dice = dice;
            mod.Name = name ?? mod.Target.Prop ?? entity.GetType().Name;

            return mod;
        }

        public void SetDice(Dice dice)
        {
            Dice = dice;
            Source = null;
        }

        public bool ShouldBeRemoved(int turn)
        {
            if (ModifierType == ModifierType.Transient)
            {
                if (RemoveOnTurn == RemoveTurn.WhenZero && Dice != null && Dice.Value.Roll() == 0)
                    return true;

                if (RemoveOnTurn == RemoveTurn.This)
                    return true;

                if (RemoveOnTurn == RemoveTurn.Encounter && turn == 0)
                    return true;

                return RemoveOnTurn <= turn;
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
    }
}
