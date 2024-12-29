using System.Linq.Expressions;
using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.ModSets
{
    public class ModSet : Lifespan
    {
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public List<Mod> Mods { get; protected set; } = new();

        [JsonConstructor] public ModSet() { }

        public ModSet(string ownerId, bool syncToOwner)
            : base(ownerId, syncToOwner)
        { }

        public ModSet Lifespan(int duration)
        {
            Start = new TimePoint(TimePointType.Turn, 0);
            End = new TimePoint(TimePointType.Turn, duration);

            return this;
        }

        public ModSet Lifespan(int startsIn, int duration)
        {
            Start = new TimePoint(TimePointType.Turn, startsIn);
            End = new TimePoint(TimePointType.Turn, startsIn + duration);

            return this;
        }

        public ModSet Lifespan(TimePoint start, TimePoint end)
        {
            Start = start;
            End = end;

            return this;
        }

        public ModSet ExtractFor(string objectId)
        {
            var mods = Mods
                .Where(x => x.Target.ObjectId == objectId)
                .Select(x => new Mod(ModType.Standard).SetTarget(x.Target).SetSource(x.Source))
                .ToList();

            Mods = Mods.Where(x => x.Target.ObjectId != objectId).ToList();

            var res = new ModSet(objectId, false);
            res.Mods = mods;
            return res;
        }

        public override void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            base.OnCreating(graph, obj);
            SyncMods(graph);
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            SyncMods(graph);
            base.OnTimeEvent(graph);
        }

        public void Add(Mod mod)
        {
            if (!Mods.Any(x => x.Id == mod.Id))
            {
                Mods.Add(mod
                    .SetApply(IsApplied)
                    .SetDisabled(IsDisabled));
            }
        }

        private void SyncMods(RpgGraph graph)
        {
            foreach (var mod in Mods)
            {
                graph.Add(mod
                    .SetApply(IsApplied)
                    .SetDisabled(IsDisabled));
            }
            Mods.Clear();
        }

        //private Mod[] GetMods(RpgGraph graph)
        //{
        //    var mods = new List<Mod>();
        //    foreach (var byObjId in ModTargets.GroupBy(x => x.ObjectId))
        //    {
        //        var obj = graph.GetObjectData(byObjId.Key);
        //        if (obj != null)
        //        {
        //            foreach (var propRef in byObjId)
        //            {
        //                var propData = obj.GetPropData<RpgPropertyDataModdable>(propRef.Prop);
        //                var propMods = propData?.Mods.Where(x => x.SyncToObjectId == Id).ToArray() ?? [];
        //                mods.AddRange(propMods);
        //            }
        //        }
        //    }

        //    return mods.ToArray();
        //}
    }

    public static class ModSetExtensions
    {
        public static T Add<T, TEntity>(this T modSet, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            modSet.Add(new Mod(ModType.Standard)
                .SetTarget(entity, targetProp)
                .SetSource(dice, valueCalc)
                .SetOwner(modSet.Id, true));

            return modSet;
        }

        public static T Add<T, TEntity>(this T modSet, Mod mod, TEntity target, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            mod
                .SetTarget(target, targetProp)
                .SetSource(dice, valueCalc)
                .SetOwner(modSet.Id, true);

            modSet.Add(mod);
            return modSet;
        }

        public static T Add<T, TEntity, TTargetValue, TSourceValue>(this T modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            modSet.Add(new Mod(ModType.Standard)
                .SetTarget(entity, targetExpr)
                .SetSource(entity, sourceExpr, valueCalc)
                .SetOwner(modSet.Id, true));

            return modSet;
        }


        public static T Add<T, TTarget, TTargetValue, TSource, TSourceValue>(this T modSet, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            modSet.Add(new Mod(ModType.Standard)
                .SetTarget(target, targetExpr)
                .SetSource(source, sourceExpr, valueCalc)
                .SetOwner(modSet.Id, true));

            return modSet;
        }

        public static T Add<T, TTarget, TTargetVal, TSource, TSourceVal>(this T modSet, Mod mod, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where T : ModSet
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            mod
                .SetTarget(target, targetExpr)
                .SetSource(source, sourceExpr, valueFunc)
                .SetOwner(modSet.Id, true);

            modSet.Add(mod);
            return modSet;
        }

        public static T Add<T, TEntity, TSourceValue>(this T modSet, TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            modSet.Add(new Mod(ModType.Standard)
                .SetTarget(entity, targetProp)
                .SetSource(entity, sourceExpr, valueCalc)
                .SetOwner(modSet.Id, true));

            return modSet;
        }

        public static T Add<T, TTarget, TSourceValue>(this T modSet, Mod mod, TTarget target, string targetProp, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where T : ModSet
            where TTarget : RpgObject
        {
            mod
                .SetTarget(target, targetProp)
                .SetSource(target, sourceExpr, valueFunc)
                .SetOwner(modSet.Id, true);

            modSet.Add(mod);
            return modSet;
        }

        public static T Add<T, TTarget, TSource, TSourceValue>(this T modSet, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            modSet.Add(new Mod(ModType.Standard)
                .SetTarget(target, targetProp)
                .SetSource(source, sourceExpr, valueFunc)
                .SetOwner(modSet.Id, true));

            return modSet;
        }

        public static T Add<T, TTarget, TSource, TSourceValue>(this T modSet, Mod mod, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            mod
                .SetTarget(target, targetProp)
                .SetSource(source, sourceExpr, valueFunc)
                .SetOwner(modSet.Id, true);

            modSet.Add(mod);
            return modSet;
        }

        public static T Add<T, TTarget, TTargetValue>(this T modSet, Mod mod, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where T : ModSet
            where TTarget : RpgObject
        {
            mod
                .SetTarget(target, targetExpr)
                .SetSource(dice, valueFunc)
                .SetOwner(modSet.Id, true);

            modSet.Add(mod);
            return modSet;
        }

        public static T Add<T, TEntity, TTargetValue>(this T modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            modSet.Add(new Mod(ModType.Standard)
                .SetTarget(entity, targetExpr)
                .SetSource(dice, valueCalc)
                .SetOwner(modSet.Id, true));

            return modSet;
        }

        public static T Add<T, TEntity, TTarget, TTargetValue, TSourceValue>(this T modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            modSet.Add(new Mod(ModType.Standard)
                .SetTarget(entity, targetExpr)
                .SetSource(entity, sourceExpr, valueCalc)
                .SetOwner(modSet.Id, true));

            return modSet;
        }
    }
}
