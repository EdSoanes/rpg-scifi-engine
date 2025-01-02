using System.Linq.Expressions;
using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.ModSets
{
    public class ModSet : Lifespan
    {
        private List<Mod> _newMods = new();
        private List<Mod> _existingMods = new();

        [JsonProperty] public string? Name { get; set; }
        [JsonIgnore] public Mod[] Mods { get => _existingMods.Concat(_newMods).ToArray(); }

        [JsonConstructor] public ModSet() { }

        public ModSet(string ownerId, bool syncToOwner)
            : base(ownerId, syncToOwner)
        { }

        public ModSet(string name, string ownerId, bool syncToOwner)
            : base(ownerId, syncToOwner)
        {
            Name = name;
        }

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
            var mods = _newMods
                .Where(x => x.Target?.ObjectId == objectId)
                .Select(x => new Mod(ModType.Standard).SetTarget(x.Target).SetSource(x.Source))
                .ToList();

            _newMods = _newMods.Where(x => x.Target?.ObjectId != objectId).ToList();

            var res = new ModSet(objectId, false);
            res._newMods = mods;
            return res;
        }

        public override void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            base.OnCreating(graph, obj);
            Sync(graph);
        }

        public override void OnRestoring(RpgGraph graph)
        {
            base.OnRestoring(graph);
            _existingMods = graph.GetOwnerMods(Id).ToList();
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            Sync(graph);
            base.OnTimeEvent(graph);
        }

        public void Add(Mod mod)
        {
            if (!_newMods.Any(x => x.Id == mod.Id))
            {
                _newMods.Add(mod
                    .SetApply(IsApplied)
                    .SetDisabled(IsDisabled));
            }
        }

        private void Sync(RpgGraph graph)
        {
            var newMods = _newMods.ToArray();
            _newMods.Clear();

            foreach (var mod in newMods)
            {
                graph.Add(mod
                    .SetApply(IsApplied)
                    .SetDisabled(IsDisabled));
            }

            _existingMods.AddRange(newMods);

            foreach (var mod in _existingMods)
            {
                if (IsApplied && !mod.IsApplied) mod.Apply();
                else if (!IsApplied && mod.IsApplied) mod.Unapply();

                if (IsDisabled && !mod.IsDisabled) mod.UserDisabled();
                else if (!IsDisabled && mod.IsDisabled) mod.UserEnabled();
            }
        }

        public void Reset()
        {
            foreach (var mod in Mods)
                mod.Expire(TimePointType.TimeBegins);
        }

        public ModSet Add<TEntity>(TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)

            where TEntity : RpgObject
        {
            Add(new Mod(ModType.Standard)
                .SetTarget(entity, targetProp)
                .SetSource(dice, valueCalc)
                .SetOwner(Id, true));

            return this;
        }

        public ModSet Add<TEntity>(Mod mod, TEntity target, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            mod
                .SetTarget(target, targetProp)
                .SetSource(dice, valueCalc)
                .SetOwner(Id, true);

            Add(mod);
            return this;
        }

        public ModSet Add<TEntity, TTargetValue, TSourceValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            Add(new Mod(ModType.Standard)
                .SetTarget(entity, targetExpr)
                .SetSource(entity, sourceExpr, valueCalc)
                .SetOwner(Id, true));

            return this;
        }


        public ModSet Add<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            Add(new Mod(ModType.Standard)
                .SetTarget(target, targetExpr)
                .SetSource(source, sourceExpr, valueCalc)
                .SetOwner(Id, true));

            return this;
        }

        public ModSet Add<TTarget, TTargetVal, TSource, TSourceVal>(Mod mod, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            mod
                .SetTarget(target, targetExpr)
                .SetSource(source, sourceExpr, valueFunc)
                .SetOwner(Id, true);

            Add(mod);
            return this;
        }

        public ModSet Add<TEntity, TSourceValue>(TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            Add(new Mod(ModType.Standard)
                .SetTarget(entity, targetProp)
                .SetSource(entity, sourceExpr, valueCalc)
                .SetOwner(Id, true));

            return this;
        }

        public ModSet Add<TTarget, TSourceValue>(Mod mod, TTarget target, string targetProp, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            mod
                .SetTarget(target, targetProp)
                .SetSource(target, sourceExpr, valueFunc)
                .SetOwner(Id, true);

            Add(mod);
            return this;
        }

        public ModSet Add<TTarget, TSource, TSourceValue>(TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            Add(new Mod(ModType.Standard)
                .SetTarget(target, targetProp)
                .SetSource(source, sourceExpr, valueFunc)
                .SetOwner(Id, true));

            return this;
        }

        public ModSet Add<TTarget, TSource, TSourceValue>(Mod mod, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            mod
                .SetTarget(target, targetProp)
                .SetSource(source, sourceExpr, valueFunc)
                .SetOwner(Id, true);

            Add(mod);
            return this;
        }

        public ModSet Add<TTarget, TTargetValue>(Mod mod, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            mod
                .SetTarget(target, targetExpr)
                .SetSource(dice, valueFunc)
                .SetOwner(Id, true);

            Add(mod);
            return this;
        }

        public ModSet Add<TEntity, TTargetValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            Add(new Mod(ModType.Standard)
                .SetTarget(entity, targetExpr)
                .SetSource(dice, valueCalc)
                .SetOwner(Id, true));

            return this;
        }

        public ModSet Add<TEntity, TTarget, TTargetValue, TSourceValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            Add(new Mod(ModType.Standard)
                .SetTarget(entity, targetExpr)
                .SetSource(entity, sourceExpr, valueCalc)
                .SetOwner(Id, true));

            return this;
        }
    }
}
