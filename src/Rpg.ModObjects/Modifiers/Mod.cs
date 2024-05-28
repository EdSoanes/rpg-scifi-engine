using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

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

        public void SetSource(Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            => Source = new ModSource<ModObject, Dice>(value, diceCalcExpr);

        internal static Mod Create<TBehavior, TTarget, TTargetVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var behavior = (TBehavior)Activator.CreateInstance(typeof(TBehavior))!;
            var modSource = new ModSource<TTarget, Dice>(value, valueFunc);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TBehavior, TTarget, TTargetVal, TSource, TSourceVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TSource : ModObject
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var behavior = (TBehavior)Activator.CreateInstance(typeof(TBehavior))!;
            var modSource = new ModSource<TSource, TSourceVal>(source, sourceExpr, valueFunc);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TBehavior, TTarget, TSource, TSourceVal>(TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TSource : ModObject
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var behavior = (TBehavior)Activator.CreateInstance(typeof(TBehavior))!;
            var modSource = new ModSource<TSource, TSourceVal>(source, sourceExpr, valueFunc);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TBehavior, TTarget>(TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var behavior = (TBehavior)Activator.CreateInstance(typeof(TBehavior))!;
            var modSource = new ModSource<TTarget, Dice>(value, valueFunc);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TBehavior, TTarget, TTargetVal>(string name, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var behavior = (TBehavior)Activator.CreateInstance(typeof(TBehavior))!;
            var modSource = new ModSource<TTarget, Dice>(value, valueFunc);

            var mod = new Mod(targetPropRef, name, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TBehavior, TTarget, TTargetVal, TSource, TSourceVal>(string name, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TSource : ModObject
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var behavior = (TBehavior)Activator.CreateInstance(typeof(TBehavior))!;
            var modSource = new ModSource<TSource, TSourceVal>(source, sourceExpr, valueFunc);

            var mod = new Mod(targetPropRef, name, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TBehavior, TTarget, TSource, TSourceVal>(string name, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TSource : ModObject
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var behavior = (TBehavior)Activator.CreateInstance(typeof(TBehavior))!;
            var modSource = new ModSource<TSource, TSourceVal>(source, sourceExpr, valueFunc);

            var mod = new Mod(targetPropRef, name, behavior, modSource);
            return mod;
        }

        internal static Mod Create<TBehavior, TTarget>(string name, TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TTarget : ModObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var behavior = (TBehavior)Activator.CreateInstance(typeof(TBehavior))!;
            var modSource = new ModSource<TTarget, Dice>(value, valueFunc);

            var mod = new Mod(targetPropRef, name, behavior, modSource);
            return mod;
        }

        public virtual void OnAdd(int turn) { }
        public virtual void OnUpdate(int turn) { }

        public override string ToString()
        {
            var mod = $"{EntityId}.{Prop} <= {Source} ({Behavior.Type})";
            return mod;
        }
    }

    public static class ModExtensions
    {
        public static TTarget AddMod<TBehavior, TTarget>(this TTarget entity, string prop, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TTarget : ModObject
        {
            var mod = Mod.Create<TBehavior, TTarget>(prop, entity, prop, value, valueFunc);
            entity.AddMod(mod);
            return entity;
        }

        public static TTarget AddMod<TBehavior, TTarget>(this TTarget entity, string name, string prop, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TTarget : ModObject
        {
            var mod = Mod.Create<TBehavior, TTarget>(name, entity, prop, value, valueFunc);
            entity.AddMod(mod);
            return entity;
        }


        public static TTarget AddMod<TBehavior, TTarget, TTargetValue>(this TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TTarget : ModObject
        {
            var mod = Mod.Create<TBehavior, TTarget, TTargetValue>(entity, targetExpr, dice, valueFunc);
            entity.AddMod(mod);
            return entity;
        }

        public static TTarget AddMod<TBehavior, TTarget, TTargetValue>(this TTarget entity, string name, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TTarget : ModObject
        {
            var mod = Mod.Create<TBehavior, TTarget, TTargetValue>(name, entity, targetExpr, dice, valueFunc);
            entity.AddMod(mod);
            return entity;
        }


        public static TTarget AddMod<TBehavior, TTarget, TTargetValue, TSourceValue>(this TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TTarget : ModObject
        {
            var mod = Mod.Create<TBehavior, TTarget, TTargetValue, TTarget, TSourceValue>(entity, targetExpr, entity, sourceExpr, valueFunc);
            entity.AddMod(mod);
            return entity;
        }

        public static TTarget AddMod<TBehavior, TTarget, TTargetValue, TSourceValue>(this TTarget entity, string name, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TBehavior : ModBehavior
            where TTarget : ModObject
        {
            var mod = Mod.Create<TBehavior, TTarget, TTargetValue, TTarget, TSourceValue>(name, entity, targetExpr, entity, sourceExpr, valueFunc);
            entity.AddMod(mod);
            return entity;
        }
    }
}
