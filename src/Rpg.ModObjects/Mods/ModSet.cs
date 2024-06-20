using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Mods
{
    public class ModSet : ILifecycle
    {
        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string InitiatorId { get; private set; }
        [JsonProperty] public string Name { get; set; }
        [JsonIgnore] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] public ILifecycle Lifecycle { get; private set; }
        [JsonConstructor] protected ModSet() { }

        public ModSet(ILifecycle lifecycle, string initiatorId, string name)
        {
            Id = this.NewId();
            InitiatorId = initiatorId;
            Lifecycle = lifecycle;
            Name = name;
        }

        public ModSet(string initiatorId, string name)
            : this(new PermanentLifecycle(), initiatorId, name)
        {
        }

        public ModSet(ILifecycle lifecycle, string initiatorId, string name, params Mod[] mods)
            : this(lifecycle, initiatorId, name)
        {
            Add(mods);
        }

        public ModSet Add(params Mod[] mods)
        {
            foreach (var mod in mods)
            {
                mod.SyncedToId = Id;
                Mods.Add(mod);
            }

            return this;
        }

        public ModSubSet[] SubSets(RpgGraph graph)
        {
            var subSets = new List<ModSubSet>();
            foreach (var modGroup in Mods.GroupBy(x => $"{x.EntityId}.{x.Prop}"))
            {
                var mods = modGroup.ToArray();
                var dice = graph.CalculateModsValue(mods);

                var subSet = new ModSubSet(Lifecycle, InitiatorId, mods.First(), mods, dice);
                subSets.Add(subSet);
            }

            return subSets.ToArray();
        }

        public Mod[] Get(string prop, Func<Mod, bool>? filterFunc = null)
            => Mods.Where(x => x.Prop == prop && (filterFunc?.Invoke(x) ?? true)).ToArray();


        public void SetExpired(TimePoint currentTime)
            => Lifecycle.SetExpired(currentTime);

        public void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        {
            if (!Mods.Any())
                Mods = graph.GetMods().Where(x => x.SyncedToId == Id).ToList();

            Lifecycle.OnBeginningOfTime(graph, entity);
        }

        public LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
            => Lifecycle.OnStartLifecycle(graph, currentTime);

        public LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
            => Lifecycle.OnUpdateLifecycle(graph, currentTime);

        public string TargetPropName => $"{InitiatorId}.{Name}.{RpgActionArg.TargetArg}";
        public string DiceRollPropName => $"{InitiatorId}.{Name}.{RpgActionArg.DiceRollArg}";
        public string OutcomePropName => $"{InitiatorId}.{Name}.{RpgActionArg.OutcomeArg}";

        public LifecycleExpiry Expiry => throw new NotImplementedException();

        //public ModSet AddState<TTarget>(TTarget target)
        //    where TTarget : RpgObject
        //{
        //    var state = target.GetStateInstanceName(Name)!;

        //    var mod = new SyncedMod(Id, nameof(ModSet))
        //        .SetBehavior(new ForceState())
        //        .SetProps(target, state, 1)
        //        .Create(state);

        //    Add(mod);

        //    return this;
        //}

        public ModSet Target<TEntity>(TEntity entity, Dice value, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var mod = new SyncedMod(Id, nameof(ModSet))
                .SetProps(entity, TargetPropName, value, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet Target<TEntity, TSourceValue>(TEntity entity, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var mod = new SyncedMod(Id, nameof(ModSet))
                .SetProps(entity, TargetPropName, entity, sourceExpr, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet Target<TTarget, TSource, TSourceValue>(TTarget target, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = new SyncedMod(Id, nameof(ModSet))
                .SetProps(target, TargetPropName, source, sourceExpr, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet Roll<TEntity>(TEntity entity, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var mod = new SyncedMod(Id, nameof(ModSet))
                .SetProps(entity, DiceRollPropName, dice, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet Roll<TEntity, TSourceValue>(TEntity entity, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var mod = new SyncedMod(Id, nameof(ModSet))
                .SetProps(entity, DiceRollPropName, entity, sourceExpr, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet Roll<TTarget, TSource, TSourceValue>(TTarget target, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = new SyncedMod(Id, nameof(ModSet))
                .SetProps(target, DiceRollPropName, source, sourceExpr, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet Outcome<TTarget>(TTarget target, int outcome, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
        {
            var mod = new SyncedMod(Id, nameof(ModSet))
                .SetProps(target, OutcomePropName, outcome, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet SyncedMod<TEntity>(TEntity entity, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
                => AddMod(new SyncedMod(Id, nameof(ModSet)), entity, targetProp, value, valueCalc);

        public ModSet SyncedMod<TEntity, TTargetValue, TSourceValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
             => AddMod(new SyncedMod(Id, nameof(ModSet)), entity, targetExpr, sourceExpr, valueCalc);

        public ModSet SyncedMod<TEntity, TSourceValue>(TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
                => AddMod(new SyncedMod(Id, nameof(ModSet)), entity, targetProp, sourceExpr, valueCalc);

        public ModSet SyncedMod<TTarget, TSource, TSourceValue>(TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
                => AddMod(new SyncedMod(Id, nameof(ModSet)), target, targetProp, source, sourceExpr, valueCalc);

        public ModSet SyncedMod<TEntity, TTargetValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
                => AddMod(new SyncedMod(Id, nameof(ModSet)), entity, targetExpr, value, valueCalc); 

        public ModSet SyncedMod<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
                => AddMod(new SyncedMod(Id, nameof(ModSet)), target, targetExpr, source, sourceExpr, valueCalc);

        public ModSet AddMod<TEntity>(ModTemplate template, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetProp, dice, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity, TTargetValue, TSourceValue>(ModTemplate template, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetExpr, entity, sourceExpr, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity, TSourceValue>(ModTemplate template, TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetProp, entity, sourceExpr, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet AddMod<TTarget, TSource, TSourceValue>(ModTemplate template, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = template
                .SetProps(target, targetProp, source, sourceExpr, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity, TTargetValue>(ModTemplate template, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetExpr, dice, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity, TTarget, TTargetValue, TSourceValue>(ModTemplate template, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetExpr, entity, sourceExpr, valueCalc)
                .Create();

            Add(mod);

            return this;
        }

        public ModSet AddMod<TTarget, TTargetValue, TSource, TSourceValue>(ModTemplate template, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = template
                .SetProps(target, targetExpr, source, sourceExpr, valueCalc)
                .Create();

            Add(mod);

            return this;
        }
    }
}
