using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Mods
{
    public class ModSet : RpgLifecycleObject
    {
        [JsonInclude] public string Id { get; private set; }
        [JsonInclude] public string? OwnerId { get; private set; }
        [JsonInclude] public string Name { get; set; }

        [JsonInclude] public bool IsApplied { get; private set; } = true;
        [JsonInclude] public bool IsDisabled { get; private set; }
        public bool IsActive { get => IsApplied && !IsDisabled; }

        [JsonIgnore] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonInclude] private List<string> _modIds { get; init; } = new();

        [JsonConstructor] protected ModSet() { }

        public ModSet(string ownerId, string name)
        {
            Id = this.NewId();
            Name = name ?? this.GetType().Name;
            OwnerId = ownerId;
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

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            Graph!.AddMods([.. Mods]);
        }

        public override void OnRestoring(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnRestoring(graph, entity);
            Mods = Graph.GetMods(mod => _modIds.Contains(mod.Id)).ToList();
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            if (!Mods.Any())
                Mods.AddRange(Graph!.GetActiveMods(x => x.OwnerId == Id));

            return base.OnStartLifecycle();
        }

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
            => Graph!.ApplyMods([.. Mods]);

        public void Unapply()
            => Graph!.UnapplyMods([.. Mods]);

        public void UserEnabled()
            => Graph!.EnableMods([.. Mods]);

        public void UserDisabled()
            => Graph!.DisableMods([.. Mods]);
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
