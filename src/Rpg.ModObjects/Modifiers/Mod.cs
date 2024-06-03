using Newtonsoft.Json;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class Mod : PropRef, ITimeLifecycle
    {
        [JsonProperty] public string Id { get; protected set; }
        [JsonProperty] public string? SyncedToId { get; internal set; }
        [JsonProperty] public string? SyncedToType { get; internal set; }
        [JsonProperty] public string Name { get; protected set; }

        [JsonProperty] public PropRef? SourcePropRef { get; protected set; }
        [JsonProperty] public Dice? SourceValue { get; protected set; }
        [JsonProperty] public ModSourceValueFunction SourceValueFunc { get; protected set; } = new ModSourceValueFunction();

        [JsonProperty] public ModBehavior Behavior { get; protected set; }
        [JsonProperty] public ITimeLifecycle Lifecycle { get; protected set; }
        [JsonProperty] public bool IsBaseInitMod { get => Behavior.Type == ModType.Initial; }
        [JsonProperty] public bool IsBaseOverrideMod { get => Behavior.Type == ModType.Override; }
        [JsonProperty] public bool IsBaseMod { get => IsBaseInitMod || IsBaseOverrideMod || Behavior.Type == ModType.Base; }

        public ModExpiry Expiry { get => Lifecycle.Expiry; }

        [JsonConstructor] protected Mod() { }

        internal Mod(string name, ModTemplate template)
        {
            Id = this.NewId();
            Name = name;
            EntityId = template.TargetPropRef.EntityId;
            Prop = template.TargetPropRef.Prop;
            SourcePropRef = template.SourcePropRef;
            SourceValue = template.SourceValue;
            SourceValueFunc = template.SourceValueFunc; 
        }

        internal Mod(string entityId, ModBehavior behavior, Mod template)
        {
            Id = this.NewId();
            Name = template.Name;
            EntityId = entityId;
            Prop = template.Prop;
            Behavior = behavior;
            Lifecycle = template.Lifecycle;
            SourcePropRef = template.SourcePropRef;
            SourceValue = template.SourceValue;
            SourceValueFunc = template.SourceValueFunc;
        }

        internal Mod(string syncedToId, string syncedToType, string name, ModTemplate template)
            : this(name, template)
        {
            SyncedToId = syncedToId;
            SyncedToType = syncedToType;
        }

        public void SetExpired()
            => Lifecycle.SetExpired();

        public ModExpiry StartLifecycle<T>(RpgGraph graph, Time.Time time, T obj)
            where T : class
                => Lifecycle.StartLifecycle<T>(graph, time, obj);

        public ModExpiry UpdateLifecycle<T>(RpgGraph graph, Time.Time time, T obj)
            where T : class
                => Lifecycle.UpdateLifecycle<T>(graph, time, obj);

        public void OnUpdating(RpgGraph graph, Time.Time time)
        {
            var propStore = graph.GetEntity(EntityId)!.PropStore;
            Behavior.OnUpdating(graph, propStore, this);
            //if (Behavior.Scope != ModScope.Standard && Behavior.Expiry == ModExpiry.Active)
            //{
            //    var entities = graph.GetEntities(EntityId, Behavior.Scope);
            //    foreach (var entity in entities)
            //    {
            //        var existing = entity.PropStore.GetMods(Prop, m => m.Prop == Prop && m.Name == Name);
            //        if (!existing.Any())
            //        {
            //            var newMod = Clone<Permanent>(entity.Id);
            //            entity.AddMods(newMod);
            //            graph.OnPropUpdated(newMod);
            //        }
            //    }
            //}
        }

        public void OnRemoving(RpgGraph graph, Mod? mod = null)
        {
            var propStore = graph.GetEntity(EntityId)!.PropStore;
            Behavior.OnRemoving(graph, this);
            //if (mod != null && mod.Behavior.Scope != ModScope.Standard)
            //{
            //    var entities = graph.GetEntities(mod.EntityId, mod.Behavior.Scope);
            //    var mods = entities
            //        .SelectMany(x =>
            //            x.PropStore.GetMods(mod.Prop, m => m.Name == mod.Name))
            //        .ToArray();

            //    graph.RemoveMods(mods);
            //}
        }

        public Mod Clone<T>(string targetId, ModScope scope = ModScope.Standard)
            where T : ModBehavior
        {
            var behavior = Behavior.Clone<T>(scope);
            var mod = new Mod(targetId, behavior, this);

            //TODO: SourceValueFunc cloning.
            //mod.SourceValueFunc

            return mod;
        }

        public override string ToString()
        {
            var src = $"{SourcePropRef}{SourceValue}";
            src = SourceValueFunc.IsCalc
                ? $"{SourceValueFunc.FuncName}({src})"
                : src;

            var mod = $"({Behavior.Type}) {EntityId}.{Prop} <= {src}";
            return mod;
        }

        public void SetSource(Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        {
            SourceValue = value;
            SourcePropRef = null;
            SourceValueFunc.Set(valueFunc);
        }

        //internal static Mod Create(ModBehavior behavior, string targetId, string prop, Dice value)
        //{
        //    var targetPropRef = new PropRef(targetId, prop);
        //    var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, value);
        //    return mod;
        //}

        //internal static Mod Create<TTarget, TTargetVal>(ModBehavior behavior, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        //    where TTarget : RpgObject
        //{
        //    var targetPropRef = CreatePropRef(target, targetExpr);

        //    var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, value);
        //    mod.SourceValueFunc.Set(valueFunc);

        //    return mod;
        //}

        //internal static Mod Create<TTarget, TTargetVal, TSource, TSourceVal>(ModBehavior behavior, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        //    where TSource : RpgObject
        //    where TTarget : RpgObject
        //{
        //    var targetPropRef = CreatePropRef(target, targetExpr);
        //    var sourcePropRef = CreatePropRef(source, sourceExpr);

        //    var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, sourcePropRef);
        //    mod.SourceValueFunc.Set(valueFunc);
        //    return mod;
        //}

        //internal static Mod Create<TTarget, TSource, TSourceVal>(ModBehavior behavior, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        //    where TSource : RpgObject
        //    where TTarget : RpgObject
        //{
        //    var targetPropRef = CreatePropRef(target, targetProp);
        //    var sourcePropRef = CreatePropRef(source, sourceExpr);

        //    var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, sourcePropRef);
        //    mod.SourceValueFunc.Set(valueFunc);
        //    return mod;
        //}

        //internal static Mod Create<TTarget>(ModBehavior behavior, TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        //    where TTarget : RpgObject
        //{
        //    var targetPropRef = CreatePropRef(target, targetProp);

        //    var mod = new Mod(targetPropRef, targetPropRef.Prop, behavior, value);
        //    mod.SourceValueFunc.Set(valueFunc);
        //    return mod;
        //}

        //internal static Mod Create<TTarget, TTargetVal>(ModBehavior behavior, string name, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        //    where TTarget : RpgObject
        //{
        //    var targetPropRef = CreatePropRef(target, targetExpr);

        //    var mod = new Mod(targetPropRef, name, behavior, value);
        //    mod.SourceValueFunc.Set(valueFunc);
        //    return mod;
        //}

        //internal static Mod Create<TTarget, TTargetVal, TSource, TSourceVal>(ModBehavior behavior, string name, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        //    where TSource : RpgObject
        //    where TTarget : RpgObject
        //{
        //    var targetPropRef = CreatePropRef(target, targetExpr);
        //    var sourcePropRef = CreatePropRef(source, sourceExpr);

        //    var mod = new Mod(targetPropRef, name, behavior, sourcePropRef);
        //    mod.SourceValueFunc.Set(valueFunc);
        //    return mod;
        //}

        //internal static Mod Create<TTarget, TSource, TSourceVal>(ModBehavior behavior, string name, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        //    where TSource : RpgObject
        //    where TTarget : RpgObject
        //{
        //    var targetPropRef = CreatePropRef(target, targetProp);
        //    var sourcePropRef = CreatePropRef(source, sourceExpr);

        //    var mod = new Mod(targetPropRef, name, behavior, sourcePropRef);
        //    mod.SourceValueFunc.Set(valueFunc);
        //    return mod;
        //}

        //internal static Mod Create<TTarget>(ModBehavior behavior, string name, TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        //    where TTarget : RpgObject
        //{
        //    var targetPropRef = CreatePropRef(target, targetProp);

        //    var mod = new Mod(targetPropRef, name, behavior, value);
        //    mod.SourceValueFunc.Set(valueFunc);
        //    return mod;
        //}
    }

    //public static class ModExtensions
    //{
    //    public static TTarget AddMod<TTarget>(this TTarget entity, ModBehavior behavior, string prop, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
    //        where TTarget : RpgObject
    //    {
    //        var mod = Mod.Create(behavior, prop, entity, prop, value, valueFunc);
    //        entity.AddMods(mod);
    //        return entity;
    //    }

    //    public static TTarget AddMod<TTarget>(this TTarget entity, ModBehavior behavior, string name, string prop, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
    //        where TTarget : RpgObject
    //    {
    //        var mod = Mod.Create(behavior, name, entity, prop, value, valueFunc);
    //        entity.AddMods(mod);
    //        return entity;
    //    }


    //    public static TTarget AddMod<TTarget, TTargetValue>(this TTarget entity, ModBehavior behavior, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
    //        where TTarget : RpgObject
    //    {
    //        var mod = Mod.Create(behavior, entity, targetExpr, dice, valueFunc);
    //        entity.AddMods(mod);
    //        return entity;
    //    }

    //    public static TTarget AddMod<TTarget, TTargetValue>(this TTarget entity, ModBehavior behavior, string name, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
    //        where TTarget : RpgObject
    //    {
    //        var mod = Mod.Create<TTarget, TTargetValue>(behavior, name, entity, targetExpr, dice, valueFunc);
    //        entity.AddMods(mod);
    //        return entity;
    //    }


    //    public static TTarget AddMod<TTarget, TTargetValue, TSourceValue>(this TTarget entity, ModBehavior behavior, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
    //        where TTarget : RpgObject
    //    {
    //        var mod = Mod.Create(behavior, entity, targetExpr, entity, sourceExpr, valueFunc);
    //        entity.AddMods(mod);
    //        return entity;
    //    }

    //    public static TTarget AddMod<TTarget, TTargetValue, TSourceValue>(this TTarget entity, ModBehavior behavior, string name, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
    //        where TTarget : RpgObject
    //    {
    //        var mod = Mod.Create(behavior, name, entity, targetExpr, entity, sourceExpr, valueFunc);
    //        entity.AddMods(mod);
    //        return entity;
    //    }
    //}
}
