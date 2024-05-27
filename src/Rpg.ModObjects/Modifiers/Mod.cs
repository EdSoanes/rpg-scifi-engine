using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;

namespace Rpg.ModObjects.Modifiers
{
    //TODO: Improved modifier lifecycle. Try to encode Start/End turns, Permanent, Conditional, etc into a more intuitive model.

    public abstract class Mod : ModPropRef
    {
        [JsonConstructor] protected Mod() { }

        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public ModSource Source { get; protected set; }
        [JsonProperty] public ModType ModifierType { get; protected set; }
        [JsonProperty] public ModAction ModifierAction { get; protected set; } = ModAction.Accumulate;
        [JsonProperty] public ModDuration Duration { get; protected set; } = new ModDuration();

        [JsonProperty] public bool IsBaseInitMod { get => ModifierType == ModType.BaseInit; }
        [JsonProperty] public bool IsBaseOverrideMod { get => ModifierType == ModType.BaseOverride; }
        [JsonProperty] public bool IsBaseMod { get => IsBaseInitMod || IsBaseOverrideMod || ModifierType == ModType.Base; }

        public void SetSource(Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            => Source = new ModSource<ModObject, Dice>(value, diceCalcExpr);

        internal static TMod Create<TMod, TTarget, TTargetVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Mod
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var mod = (TMod)Activator.CreateInstance(typeof(TMod), targetPropRef)!;
            mod.Source = new ModSource<TTarget, Dice>(value, diceCalcExpr);

            return mod;
        }

        internal static TMod Create<TMod, TTarget, TTargetVal, TSource, TSourceVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Mod
            where TSource : ModObject
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var mod = (TMod)Activator.CreateInstance(typeof(TMod), targetPropRef)!;
            mod.Source = new ModSource<TSource, TSourceVal>(source, sourceExpr, diceCalcExpr);

            return mod;
        }

        internal static TMod Create<TMod, TTarget, TSource, TSourceVal>(TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Mod
            where TSource : ModObject
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var mod = (TMod)Activator.CreateInstance(typeof(TMod), targetPropRef)!;
            mod.Source = new ModSource<TSource, TSourceVal>(source, sourceExpr, diceCalcExpr);

            return mod;
        }

        internal static TMod Create<TMod, TTarget>(TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Mod
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var mod = (TMod)Activator.CreateInstance(typeof(TMod), targetPropRef)!;
            mod.Source = new ModSource<TTarget, Dice>(value, diceCalcExpr);

            return mod;
        }

        internal static TMod Create<TMod, TTarget, TTargetVal>(string name, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Mod
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var mod = (TMod)Activator.CreateInstance(typeof(TMod), name, targetPropRef)!;
            mod.Source = new ModSource<TTarget, Dice>(value, diceCalcExpr);

            return mod;
        }

        internal static TMod Create<TMod, TTarget, TTargetVal, TSource, TSourceVal>(string name, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Mod
            where TSource : ModObject
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var mod = (TMod)Activator.CreateInstance(typeof(TMod), name, targetPropRef)!;
            mod.Source = new ModSource<TSource, TSourceVal>(source, sourceExpr, diceCalcExpr);

            return mod;
        }

        internal static TMod Create<TMod, TTarget, TSource, TSourceVal>(string name, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Mod
            where TSource : ModObject
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var mod = (TMod)Activator.CreateInstance(typeof(TMod), name, targetPropRef)!;
            mod.Source = new ModSource<TSource, TSourceVal>(source, sourceExpr, diceCalcExpr);

            return mod;
        }

        internal static TMod Create<TMod, TTarget>(string name, TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Mod
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var mod = (TMod)Activator.CreateInstance(typeof(TMod), name, targetPropRef)!;
            mod.Source = new ModSource<TTarget, Dice>(value, diceCalcExpr);

            return mod;
        }

        public virtual void OnAdd(int turn) { }
        public virtual void OnUpdate(int turn) { }

        public override string ToString()
        {
            var mod = $"{EntityId}.{Prop} <= {Source} ({ModifierType}, {Duration})";
            return mod;
        }
    }
}
