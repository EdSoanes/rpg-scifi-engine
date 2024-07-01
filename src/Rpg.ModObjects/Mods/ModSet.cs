using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Mods
{
    public class ModSet
    {
        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string? OwnerId { get; private set; }
        [JsonProperty] public string? OwnerArchetype { get; private set; }

        [JsonProperty] public string Name { get; set; }

        [JsonIgnore] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] public ILifecycle Lifecycle { get; protected set; }

        public LifecycleExpiry Expiry { get => Lifecycle.Expiry; protected set { } }

        [JsonConstructor] protected ModSet() { }

        protected ModSet(string ownerId, string ownerArchetype, string? name = null) 
        {
            Id = this.NewId();
            Name = name ?? GetType().Name;
            OwnerId = ownerId;
            OwnerArchetype = ownerArchetype;
        }

        public ModSet(ILifecycle lifecycle, string? name = null)
        {
            Id = this.NewId();
            Lifecycle = lifecycle;
            Name = name ?? GetType().Name;
        }

        public ModSet SetOwner(RpgObject? owner)
        {
            OwnerId = owner?.Id;
            OwnerArchetype = owner?.GetType().Name;

            return this;
        }

        public ModSet AddMods(params Mod[] mods)
        {
            foreach (var mod in mods)
            {
                mod.OwnerId = Id;
                Mods.Add(mod);
            }

            return this;
        }

        public virtual void SetExpired(TimePoint currentTime) 
            => Lifecycle.SetExpired(currentTime);

        public void OnAdding(RpgObject? entity = null)
        {
            OwnerId ??= entity?.Id;
            OwnerArchetype ??= entity?.Archetype;
        }
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
