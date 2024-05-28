using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.ModObjects.Modifiers
{
    //TODO: Improved modifier lifecycle. Try to encode Start/End turns, Permanent, Conditional, etc into a more intuitive model.

    public class Mod : ModPropRef
    {
        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public Guid? ModSetId { get; internal set; }
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public ModSource Source { get; protected set; }
        //[JsonProperty] public ModType ModifierType { get; protected set; }
        //[JsonProperty] public ModAction ModifierAction { get; protected set; } = ModAction.Accumulate;
        //[JsonProperty] public ModDuration Duration { get; protected set; } = new ModDuration();

        [JsonProperty] public ModBehavior Behavior { get; protected set; }
        [JsonProperty] public bool IsBaseInitMod { get => Behavior.Type == ModType.Initial; }
        [JsonProperty] public bool IsBaseOverrideMod { get => Behavior.Type == ModType.Override; }
        [JsonProperty] public bool IsBaseMod { get => IsBaseInitMod || IsBaseOverrideMod || Behavior.Type == ModType.Base; }

        [JsonConstructor] protected Mod() { }

        private Mod(ModPropRef targetPropRef, string name, ModBehavior behavior, ModSource source)
        {
            EntityId = targetPropRef.EntityId;
            Prop = targetPropRef.Prop;
            Name = name;
            Behavior = behavior;
            Source = source;
        }


        public virtual void OnAdd(int turn) 
            => Behavior.OnAdd(turn);

        public virtual void OnUpdate(int turn) { }

        public override string ToString()
        {
            var mod = $"{EntityId}.{Prop} <= {Source} ({Behavior.Type})";
            return mod;
        }

        public void SetSource(Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            => Source = new ModSource<ModObject, Dice>(value, diceCalcExpr);

        internal static Mod Create(ModBehavior behavior, Guid targetId, string prop, Dice value)
        {
            var targetPropRef = new ModPropRef(targetId, prop);
            var modSource = new ModSource(value);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TTarget, TTargetVal>(ModBehavior behavior, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var modSource = new ModSource<TTarget, Dice>(value, valueFunc);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TTarget, TTargetVal, TSource, TSourceVal>(ModBehavior behavior, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : ModObject
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var modSource = new ModSource<TSource, TSourceVal>(source, sourceExpr, valueFunc);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TTarget, TSource, TSourceVal>(ModBehavior behavior, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : ModObject
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var modSource = new ModSource<TSource, TSourceVal>(source, sourceExpr, valueFunc);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TTarget>(ModBehavior behavior, TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var modSource = new ModSource<TTarget, Dice>(value, valueFunc);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TTarget, TTargetVal>(ModBehavior behavior, string name, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var modSource = new ModSource<TTarget, Dice>(value, valueFunc);

            var mod = new Mod(targetPropRef, name, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TTarget, TTargetVal, TSource, TSourceVal>(ModBehavior behavior, string name, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : ModObject
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var modSource = new ModSource<TSource, TSourceVal>(source, sourceExpr, valueFunc);

            var mod = new Mod(targetPropRef, name, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TTarget, TSource, TSourceVal>(ModBehavior behavior, string name, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : ModObject
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var modSource = new ModSource<TSource, TSourceVal>(source, sourceExpr, valueFunc);

            var mod = new Mod(targetPropRef, name, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TTarget>(ModBehavior behavior, string name, TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var modSource = new ModSource<TTarget, Dice>(value, valueFunc);

            var mod = new Mod(targetPropRef, name, behavior, modSource);
            return mod;
        }
    }

    public static class ModExtensions
    {
        public static TTarget AddMod<TTarget>(this TTarget entity, ModBehavior behavior, string prop, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create(behavior, prop, entity, prop, value, valueFunc);
            entity.AddMod(mod);
            return entity;
        }

        public static TTarget AddMod<TTarget>(this TTarget entity, ModBehavior behavior, string name, string prop, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create(behavior, name, entity, prop, value, valueFunc);
            entity.AddMod(mod);
            return entity;
        }


        public static TTarget AddMod<TTarget, TTargetValue>(this TTarget entity, ModBehavior behavior, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create(behavior, entity, targetExpr, dice, valueFunc);
            entity.AddMod(mod);
            return entity;
        }

        public static TTarget AddMod<TTarget, TTargetValue>(this TTarget entity, ModBehavior behavior, string name, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create<TTarget, TTargetValue>(behavior, name, entity, targetExpr, dice, valueFunc);
            entity.AddMod(mod);
            return entity;
        }


        public static TTarget AddMod<TTarget, TTargetValue, TSourceValue>(this TTarget entity, ModBehavior behavior, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create(behavior, entity, targetExpr, entity, sourceExpr, valueFunc);
            entity.AddMod(mod);
            return entity;
        }

        public static TTarget AddMod<TTarget, TTargetValue, TSourceValue>(this TTarget entity, ModBehavior behavior, string name, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create(behavior, name, entity, targetExpr, entity, sourceExpr, valueFunc);
            entity.AddMod(mod);
            return entity;
        }
    }
}
