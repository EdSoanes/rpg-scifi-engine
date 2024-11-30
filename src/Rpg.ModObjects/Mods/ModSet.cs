using Newtonsoft.Json;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Mods
{
    public class ModSet : RpgLifecycleObject
    {
        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string? OwnerId { get; protected set; }
        [JsonProperty] public string Name { get; set; }

        [JsonProperty] public bool IsApplied { get; protected set; } = true;
        [JsonProperty] public bool IsDisabled { get; protected set; }
        public bool IsActive { get => IsApplied && !IsDisabled; }

        [JsonIgnore] public List<Mod> Mods { get; protected set; } = new List<Mod>();
        [JsonProperty] private List<string> _modIds { get; init; } = new();

        [JsonConstructor] protected ModSet() { }

        public ModSet(string ownerId, string name)
        {
            Id = this.NewId();
            Name = name ?? this.GetType().Name;
            OwnerId = ownerId;
        }

        public ModSet ExtractFor(string entityId)
        {
            var mods = Mods
                .Where(x => x.EntityId == entityId)
                .Select(x => new Synced(entityId).Set(x.Target, x))
                .ToList();

            Mods = Mods.Where(x => x.EntityId != entityId).ToList();

            var res = new ModSet(entityId, Name);
            res.Mods = mods;
            return res;
        }

        public override void SetExpired()
        {
            base.SetExpired();
            foreach (var mod in Mods.Where(x => x is Synced && !x.IsExpired))
            {
                mod.SetExpired();
                Graph.OnPropUpdated(mod.Target);
            }
        }

        public void Remove()
            => Graph?.RemoveModSet(Id);

        public virtual void Reset()
        {
            Graph.RemoveMods(Mods.ToArray());
            Mods.Clear();
            _modIds.Clear();
        }

        public ModSetDescription Describe()
        {
            var res = new ModSetDescription(Name);
            if (Graph != null)
            {
                var entity = Graph.GetObject(OwnerId);
                if (entity != null)
                {
                    var isAdded = entity.ModSets.ContainsKey(Id);
                    var isApplied = IsApplied;

                    if (!isAdded)
                    {
                        entity.AddModSet(this);
                        Graph.Time.TriggerEvent();
                    }

                    if (!isApplied)
                    {
                        Apply();
                        Graph.Time.TriggerEvent();
                    }

                    foreach (var modGroup in Mods.GroupBy(x => x.Target))
                    {
                        var val = Graph.CalculateModsValue(modGroup.ToArray()) ?? Dice.Zero;
                        res.Set(modGroup.Key, val);
                    }

                    if (!isApplied)
                    {
                        Unapply();
                        Graph.Time.TriggerEvent();
                    }

                    if (!isAdded)
                    {
                        entity.RemoveModSet(Id);
                        Graph.Time.TriggerEvent();
                    }
                }
            }

            return res;
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            Graph!.AddMods([.. Mods]);
        }

        public override void OnRestoring(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnRestoring(graph, entity);
            Mods = Graph.GetMods(mod => _modIds.Contains(mod.Id)).ToList();
            _modIds.Clear();
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            if (!Mods.Any())
                Mods.AddRange(Graph!.GetActiveMods(x => x.OwnerId == Id));

            var ownerExpiry = GetOwnerExpiry();
            if (ownerExpiry != LifecycleExpiry.Active)
            {
                Expiry = ownerExpiry;
                return Expiry;
            }

            return base.OnStartLifecycle();
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            var ownerExpiry = GetOwnerExpiry();
            if (ownerExpiry != LifecycleExpiry.Active)
            {
                Expiry = ownerExpiry;
                return Expiry;
            }

            return base.OnUpdateLifecycle();
        }

        public PropRef[] ModPropRefs()
        {
            var res = new List<PropRef>();
            
            foreach (var mod in Mods)
                if (!res.Any(x => x.EntityId == mod.Target.EntityId && x.Prop == mod.Target.Prop))
                    res.Add(mod.Target);

            return res.ToArray();
        }

        public Dice? CalculateValue(PropRef propRef)
            => CalculateValue(propRef.EntityId, propRef.Prop);

        public Dice? CalculateValue(string entityId, string prop)
        {
            var mods = GetMods(entityId, prop) ?? [];
            return Graph?.CalculateModsValue(mods);
        }

        public Mod[] GetMods(PropRef propRef)
            => Mods
                .Where(x => x.Target.EntityId == propRef.EntityId && x.Target.Prop == propRef.Prop)
                .ToArray();

        public Mod[] GetMods(string entityId, string targetProp)
            => Mods
                .Where(x => x.Target.EntityId == entityId && x.Target.Prop == targetProp)
                .ToArray();
        
        public void AddMods(params Mod[] mods)
        {
            foreach (var mod in mods)
            {
                if (!_modIds.Contains(mod.Id))
                    _modIds.Add(mod.Id);

                if (!Mods.Any(x => x.Id == mod.Id))
                {
                    if (IsApplied) mod.Apply();
                    else mod.Unapply();

                    if (IsDisabled) mod.Disable();
                    else mod.Enable();

                    Mods.Add(mod);
                }
            }

            Graph?.AddMods(mods);
        }

        public void Apply()
        {
            IsApplied = true;
            foreach (var mod in Mods.Where(x => !x.IsApplied))
            {
                mod.Apply();
                Graph?.OnPropUpdated(mod.Target);
            }
        }

        public void Unapply()
        {
            IsApplied = false;
            foreach (var mod in Mods.Where(x => x.IsApplied))
            {
                mod.Unapply();
                Graph?.OnPropUpdated(mod.Target);
            }
        }

        public void UserEnabled()
        {
            IsDisabled = false;
            foreach (var mod in Mods.Where(x => x.IsDisabled))
            {
                mod.Enable();
                Graph?.OnPropUpdated(mod.Target);
            }
        }

        public void UserDisabled()
        {
            IsDisabled = true;
            foreach (var mod in Mods.Where(x => !x.IsDisabled))
            {
                mod.Disable();
                Graph?.OnPropUpdated(mod.Target);
            }
        }

        private LifecycleExpiry GetOwnerExpiry()
        {
            if (Graph != null && OwnerId != null)
            {
                var owner = Graph.GetObject(OwnerId);
                if (owner != null)
                    return owner.Expiry;
            }

            return LifecycleExpiry.Unset;
        }
    }

    public static class ModSetExtensions
    {
        public static T Add<T, TEntity>(this T modSet, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
                => modSet.Add(new Synced(modSet.Id), entity, targetProp, dice, valueCalc);

        public static T Add<T, TEntity>(this T modSet, Mod mod, TEntity target, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            mod.Set(target, targetProp, dice, valueCalc);
            modSet.AddMods(mod);
            return modSet;
        }

        public static T Add<T, TEntity, TTargetValue, TSourceValue>(this T modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
                => modSet.Add(new Synced(modSet.Id), entity, targetExpr, entity, sourceExpr, valueCalc);

        public static T Add<T, TTarget, TTargetValue, TSource, TSourceValue>(this T modSet, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
                => modSet.Add(new Synced(modSet.Id), target, targetExpr, source, sourceExpr, valueCalc);

        public static T Add<T, TTarget, TTargetVal, TSource, TSourceVal>(this T modSet, Mod mod, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where T : ModSet
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            mod.Set(target, targetExpr, source, sourceExpr, valueFunc);
            modSet.AddMods(mod);
            return modSet;
        }

        public static T Add<T, TEntity, TSourceValue>(this T modSet, TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
                => modSet.Add(new Synced(modSet.Id), entity, targetProp, sourceExpr, valueCalc);

        public static T Add<T, TTarget, TSourceValue>(this T modSet, Mod mod, TTarget target, string targetProp, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where T : ModSet
            where TTarget : RpgObject
        {
            mod.Set(target, targetProp, sourceExpr, valueFunc);
            modSet.AddMods(mod);
            return modSet;
        }

        public static T Add<T, TTarget, TSource, TSourceValue>(this T modSet, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
                => modSet.Add(new Synced(modSet.Id), target, targetProp, source, sourceExpr, valueFunc);

        public static T Add<T, TTarget, TSource, TSourceValue>(this T modSet, Mod mod, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            mod.Set(target, targetProp, source, sourceExpr, valueFunc);
            modSet.AddMods(mod);
            return modSet;
        }

        public static T Add<T, TTarget, TTargetValue>(this T modSet, Mod mod, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where T : ModSet
            where TTarget : RpgObject
        {
            mod.Set(target, targetExpr, dice, valueFunc);
            modSet.AddMods(mod);
            return modSet;
        }

        public static T Add<T, TEntity, TTargetValue>(this T modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
                => modSet.Add(new Synced(modSet.Id), entity, targetExpr, dice, valueCalc);


        public static T Add<T, TEntity, TTarget, TTargetValue, TSourceValue>(this T modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
                => modSet.Add(new Synced(modSet.Id), entity, targetExpr, entity, sourceExpr, valueCalc);
    }
}
