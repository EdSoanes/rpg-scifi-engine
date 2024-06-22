using Newtonsoft.Json;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Mods
{
    public class ModSet : ILifecycle
    {
        protected RpgGraph? Graph { get; private set; }

        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string? OwnerId { get; private set; }
        [JsonProperty] public string? OwnerArchetype { get; private set; }

        [JsonProperty] public string? InitiatorId { get; private set; }
        [JsonProperty] public string? InitiatorArchetype { get; private set; }

        [JsonProperty] public string? RecipientId { get; private set; }
        [JsonProperty] public string? RecipientArchetype { get; private set; }

        [JsonProperty] public string Name { get; set; }

        [JsonIgnore] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] public ILifecycle Lifecycle { get; protected set; }

        public LifecycleExpiry Expiry { get => Lifecycle.Expiry; protected set { } }

        [JsonConstructor] protected ModSet() { }

        public ModSet(ILifecycle lifecycle, string? name = null)
        {
            Id = this.NewId();
            Lifecycle = lifecycle;
            Name = name ?? GetType().Name;
        }

        public ModSet(string name, ILifecycle lifecycle, params Mod[] mods)
            : this(lifecycle, name)
        {
            AddMods(mods);
        }

        public ModSet AddOwner(RpgObject? owner)
        {
            OwnerId = owner?.Id;
            OwnerArchetype = owner?.GetType().Name;

            return this;
        }

        public ModSet AddRecipient(RpgObject? recipient)
        {
            RecipientId = recipient?.Id;
            RecipientArchetype = recipient?.GetType().Name;

            return this;
        }

        public ModSet AddInitiator(RpgObject? initiator)
        {
            InitiatorId = initiator?.Id;
            InitiatorArchetype = initiator?.GetType().Name;

            return this;
        }

        public ModSet AddMods(params Mod[] mods)
        {
            foreach (var mod in mods)
            {
                mod.SyncedToId = Id;
                Mods.Add(mod);
            }

            return this;
        }

        public Mod[] GetMods(string prop, Func<Mod, bool>? filterFunc = null)
            => Mods.Where(x => x.Prop == prop && (filterFunc?.Invoke(x) ?? true)).ToArray();

        public Dice GetValue(string entityId, string prop)
        {
            var mods = Mods.Where(x => x.EntityId == entityId && x.Prop == prop).ToArray();
            var val = Graph!.CalculateModsValue(mods.ToArray());

            return val;
        }
           

        //public Modification[] SubSets(RpgGraph graph)
        //{
        //    var subSets = new List<Modification>();
        //    foreach (var modGroup in Mods.GroupBy(x => $"{x.EntityId}.{x.Prop}"))
        //    {
        //        var mods = modGroup.ToArray();
        //        var dice = graph.CalculateModsValue(mods);

        //        var subSet = new Modification(modGroup.Key, Lifecycle, mods);
        //        subSets.Add(subSet);
        //    }

        //    return subSets.ToArray();
        //}

        public virtual void SetExpired(TimePoint currentTime)
            => Lifecycle.SetExpired(currentTime);

        public void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        {
            Graph = graph;
            OwnerId ??= entity?.Id;

            if (!Mods.Any())
                Mods = graph.GetMods().Where(x => x.SyncedToId == Id).ToList();

            Lifecycle.OnBeginningOfTime(graph, entity);
        }

        public LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
            => Lifecycle.OnStartLifecycle(graph, currentTime);

        public LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
            => Lifecycle.OnUpdateLifecycle(graph, currentTime);
    }

    public static class ModSetExtensions
    {
        public static T AddMod<T, TEntity>(this T modSet, ModTemplate template, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetProp, dice, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }

        public static T AddMod<T, TEntity, TTargetValue, TSourceValue>(this T modSet, ModTemplate template, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetExpr, entity, sourceExpr, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }

        public static T AddMod<T, TEntity, TSourceValue>(this T modSet, ModTemplate template, TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetProp, entity, sourceExpr, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }

        public static T AddMod<T, TTarget, TSource, TSourceValue>(this T modSet, ModTemplate template, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = template
                .SetProps(target, targetProp, source, sourceExpr, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }

        public static T AddMod<T, TEntity, TTargetValue>(this T modSet, ModTemplate template, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetExpr, dice, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }

        public static T AddMod<T, TEntity, TTarget, TTargetValue, TSourceValue>(this T modSet, ModTemplate template, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetExpr, entity, sourceExpr, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }

        public static T AddMod<T, TTarget, TTargetValue, TSource, TSourceValue>(this T modSet, ModTemplate template, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = template
                .SetProps(target, targetExpr, source, sourceExpr, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }
    }

    public class Modification<T> : ModSet
        where T : RpgObject
    {
        public Modification(ILifecycle lifecycle, string? name = null)
            : base(lifecycle, name)
        { }

        //public Modification<T> AddMod<TEntity>(ModTemplate template, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        //    where TEntity : RpgObject
        //{
        //    var mod = template
        //        .SetProps(entity, targetProp, dice, valueCalc)
        //        .Create();

        //    AddMods(mod);

        //    return this;
        //}

        //public Modification<T> AddMod<TEntity, TTargetValue, TSourceValue>(ModTemplate template, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        //    where TEntity : RpgObject
        //{
        //    var mod = template
        //        .SetProps(entity, targetExpr, entity, sourceExpr, valueCalc)
        //        .Create();

        //    AddMods(mod);

        //    return this;
        //}

        //public Modification<T> AddMod<TEntity, TSourceValue>(ModTemplate template, TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        //    where TEntity : RpgObject
        //{
        //    var mod = template
        //        .SetProps(entity, targetProp, entity, sourceExpr, valueCalc)
        //        .Create();

        //    AddMods(mod);

        //    return this;
        //}

        //public Modification<T> AddMod<TTarget, TSource, TSourceValue>(ModTemplate template, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        //    where TTarget : RpgObject
        //    where TSource : RpgObject
        //{
        //    var mod = template
        //        .SetProps(target, targetProp, source, sourceExpr, valueCalc)
        //        .Create();

        //    AddMods(mod);

        //    return this;
        //}

        //public Modification<T> AddMod<TEntity, TTargetValue>(ModTemplate template, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        //    where TEntity : RpgObject
        //{
        //    var mod = template
        //        .SetProps(entity, targetExpr, dice, valueCalc)
        //        .Create();

        //    AddMods(mod);

        //    return this;
        //}

        //public Modification<T> AddMod<TEntity, TTarget, TTargetValue, TSourceValue>(ModTemplate template, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        //    where TEntity : RpgObject
        //{
        //    var mod = template
        //        .SetProps(entity, targetExpr, entity, sourceExpr, valueCalc)
        //        .Create();

        //    AddMods(mod);

        //    return this;
        //}

        //public Modification<T> AddMod<TTarget, TTargetValue, TSource, TSourceValue>(ModTemplate template, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        //    where TTarget : RpgObject
        //    where TSource : RpgObject
        //{
        //    var mod = template
        //        .SetProps(target, targetExpr, source, sourceExpr, valueCalc)
        //        .Create();

        //    AddMods(mod);

        //    return this;
        //}
    }
}
