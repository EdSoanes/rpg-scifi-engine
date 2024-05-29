using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class Mod : PropRef
    {
        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public Guid? ModSetId { get; internal set; }
        [JsonProperty] public string Name { get; protected set; }
        //[JsonProperty] public ModSource Source { get; protected set; }
        [JsonProperty] public PropRef? SourcePropRef { get; protected set; }
        [JsonProperty] public Dice? SourceValue { get; protected set; }
        [JsonProperty] public ModSourceValueFunction SourceValueFunc { get; protected set; }

        [JsonProperty] public ModBehavior Behavior { get; protected set; }
        [JsonProperty] public bool IsBaseInitMod { get => Behavior.Type == ModType.Initial; }
        [JsonProperty] public bool IsBaseOverrideMod { get => Behavior.Type == ModType.Override; }
        [JsonProperty] public bool IsBaseMod { get => IsBaseInitMod || IsBaseOverrideMod || Behavior.Type == ModType.Base; }

        [JsonConstructor] protected Mod() { }

        private Mod(PropRef targetPropRef, string name, ModBehavior behavior, PropRef sourcePropRef)
        {
            EntityId = targetPropRef.EntityId;
            Prop = targetPropRef.Prop;
            Name = name;
            Behavior = behavior;
            SourcePropRef = sourcePropRef;
        }

        private Mod(PropRef targetPropRef, string name, ModBehavior behavior, Dice value)
        {
            EntityId = targetPropRef.EntityId;
            Prop = targetPropRef.Prop;
            Name = name;
            Behavior = behavior;
            SourceValue = value;
        }

        public virtual void OnAdd(int turn) 
            => Behavior.OnAdd(turn);

        public virtual void OnUpdate(int turn) { }

        public override string ToString()
        {
            var src = $"{SourcePropRef}{SourceValue}";
            src = SourceValueFunc.IsCalc
                ? $"{SourceValueFunc.FuncName} <= {src}"
                : src;

            var mod = $"{EntityId}.{Prop} <= {src} ({Behavior.Type})";
            return mod;
        }

        public void SetSource(Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        {
            SourceValue = value;
            SourcePropRef = null;
            SourceValueFunc.Set(valueFunc);
        }

        internal static Mod Create(ModBehavior behavior, Guid targetId, string prop, Dice value)
        {
            var targetPropRef = new PropRef(targetId, prop);
            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, value);
            return mod;
        }

        internal static Mod Create<TTarget, TTargetVal>(ModBehavior behavior, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, value);
            mod.SourceValueFunc.Set(valueFunc);

            return mod;
        }

        internal static Mod Create<TTarget, TTargetVal, TSource, TSourceVal>(ModBehavior behavior, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var sourcePropRef = CreatePropRef(source, sourceExpr);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, sourcePropRef);
            mod.SourceValueFunc.Set(valueFunc);
            return mod;
        }

        internal static Mod Create<TTarget, TSource, TSourceVal>(ModBehavior behavior, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var sourcePropRef = CreatePropRef(source, sourceExpr);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, sourcePropRef);
            mod.SourceValueFunc.Set(valueFunc);
            return mod;
        }

        internal static Mod Create<TTarget>(ModBehavior behavior, TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);

            var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, value);
            mod.SourceValueFunc.Set(valueFunc);
            return mod;
        }

        internal static Mod Create<TTarget, TTargetVal>(ModBehavior behavior, string name, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);

            var mod = new Mod(targetPropRef, name, behavior, value);
            mod.SourceValueFunc.Set(valueFunc);
            return mod;
        }

        internal static Mod Create<TTarget, TTargetVal, TSource, TSourceVal>(ModBehavior behavior, string name, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            var targetPropRef = CreatePropRef(target, targetExpr);
            var sourcePropRef = CreatePropRef(source, sourceExpr);

            var mod = new Mod(targetPropRef, name, behavior, sourcePropRef);
            mod.SourceValueFunc.Set(valueFunc);
            return mod;
        }

        internal static Mod Create<TTarget, TSource, TSourceVal>(ModBehavior behavior, string name, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);
            var sourcePropRef = CreatePropRef(source, sourceExpr);

            var mod = new Mod(targetPropRef, name, behavior, sourcePropRef);
            mod.SourceValueFunc.Set(valueFunc);
            return mod;
        }

        internal static Mod Create<TTarget>(ModBehavior behavior, string name, TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var targetPropRef = CreatePropRef(target, targetProp);

            var mod = new Mod(targetPropRef, name, behavior, value);
            mod.SourceValueFunc.Set(valueFunc);
            return mod;
        }
    }

    public static class ModExtensions
    {
        public static TTarget AddMod<TTarget>(this TTarget entity, ModBehavior behavior, string prop, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var mod = Mod.Create(behavior, prop, entity, prop, value, valueFunc);
            entity.AddMod(mod);
            return entity;
        }

        public static TTarget AddMod<TTarget>(this TTarget entity, ModBehavior behavior, string name, string prop, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var mod = Mod.Create(behavior, name, entity, prop, value, valueFunc);
            entity.AddMod(mod);
            return entity;
        }


        public static TTarget AddMod<TTarget, TTargetValue>(this TTarget entity, ModBehavior behavior, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var mod = Mod.Create(behavior, entity, targetExpr, dice, valueFunc);
            entity.AddMod(mod);
            return entity;
        }

        public static TTarget AddMod<TTarget, TTargetValue>(this TTarget entity, ModBehavior behavior, string name, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var mod = Mod.Create<TTarget, TTargetValue>(behavior, name, entity, targetExpr, dice, valueFunc);
            entity.AddMod(mod);
            return entity;
        }


        public static TTarget AddMod<TTarget, TTargetValue, TSourceValue>(this TTarget entity, ModBehavior behavior, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var mod = Mod.Create(behavior, entity, targetExpr, entity, sourceExpr, valueFunc);
            entity.AddMod(mod);
            return entity;
        }

        public static TTarget AddMod<TTarget, TTargetValue, TSourceValue>(this TTarget entity, ModBehavior behavior, string name, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var mod = Mod.Create(behavior, name, entity, targetExpr, entity, sourceExpr, valueFunc);
            entity.AddMod(mod);
            return entity;
        }
    }
}
