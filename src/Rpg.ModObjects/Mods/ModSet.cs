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

        [JsonIgnore] public List<Mod> Mods { get; protected set; } = new List<Mod>();

        [JsonConstructor] protected ModSet() { }

        public ModSet(string? ownerId, string name)
        {
            Id = this.NewId();
            Name = name ?? this.GetType().Name;
            OwnerId = ownerId;
        }

        public ModSet ExtractFor(string entityId)
        {
            var mods = Mods
                .Where(x => x.EntityId == entityId)
                .Select(x => new Permanent(entityId).Set(x.Target, x))
                .ToList();

            Mods = Mods.Where(x => x.EntityId != entityId).ToList();

            var res = new ModSet(entityId, Name);
            res.Mods = mods;
            return res;
        }

        public override void SetExpired()
        {
            base.SetExpired();
            foreach (var mod in ModFilters.SyncedToOwner(Mods, Id))
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
                        var val = ModCalculator.Value(Graph, modGroup) ?? Dice.Zero;
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

            foreach (var rpgObj in Graph.GetObjects())
                Mods.AddRange(rpgObj.GetMods().Where(x => x.OwnerId == Id));
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            var expiry = base.OnStartLifecycle();
            Graph.AddMods([.. Mods]);
            return expiry;
        }

        protected override void CalculateExpiry()
             => CalculateExpiry(OwnerId);

        public PropRef[] ModPropRefs()
        {
            var res = new List<PropRef>();
            
            foreach (var mod in Mods)
                if (!res.Any(x => x.EntityId == mod.Target.EntityId && x.Prop == mod.Target.Prop))
                    res.Add(mod.Target);

            return res.ToArray();
        }

        public void AddMods(params Mod[] mods)
        {
            foreach (var mod in mods)
            {
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


    }

    public static class ModSetExtensions
    {
        public static T Add<T, TEntity>(this T modSet, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
                => modSet.Add(new Permanent(modSet.Id), entity, targetProp, dice, valueCalc);

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
                => modSet.Add(new Permanent(modSet.Id), entity, targetExpr, entity, sourceExpr, valueCalc);

        public static T Add<T, TTarget, TTargetValue, TSource, TSourceValue>(this T modSet, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
                => modSet.Add(new Permanent(modSet.Id), target, targetExpr, source, sourceExpr, valueCalc);

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
                => modSet.Add(new Permanent(modSet.Id), entity, targetProp, sourceExpr, valueCalc);

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
                => modSet.Add(new Permanent(modSet.Id), target, targetProp, source, sourceExpr, valueFunc);

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
                => modSet.Add(new Permanent(modSet.Id), entity, targetExpr, dice, valueCalc);


        public static T Add<T, TEntity, TTarget, TTargetValue, TSourceValue>(this T modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
                => modSet.Add(new Permanent(modSet.Id), entity, targetExpr, entity, sourceExpr, valueCalc);
    }
}
