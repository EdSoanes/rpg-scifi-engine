using Newtonsoft.Json;
using Rpg.ModObjects.Cmds;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class ModSet : IGraphEvents, ITimeEvent
    {
        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string InitiatorId { get; private set; }
        [JsonProperty] public string Name { get; set; }
        [JsonIgnore] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] public ITimeLifecycle Lifecycle { get; private set; }
        [JsonConstructor] protected ModSet() { }

        public ModSet(string initiatorId, string name, ITimeLifecycle lifecycle)
        {
            Id = this.NewId();
            InitiatorId = initiatorId;
            Lifecycle = lifecycle;
            Name = name;
        }

        public ModSet(string initiatorId, string name)
            : this(initiatorId, name, new PermanentLifecycle())
        {
        }

        public ModSet(string initiatorId, string name, ITimeLifecycle lifecycle, params Mod[] mods)
            : this(initiatorId, name, lifecycle)
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

                var subSet = new ModSubSet(InitiatorId, mods.First(), mods, dice);
                subSets.Add(subSet);
            }

            return subSets.ToArray();
        }

        public Mod[] Get(string prop, Func<Mod, bool>? filterFunc = null)
            => Mods.Where(x => x.Prop == prop && (filterFunc?.Invoke(x) ?? true)).ToArray();


        public void OnGraphCreating(RpgGraph graph, RpgObject? entity = null)
        {
            if (!Mods.Any())
                Mods = graph.GetMods().Where(x => x.SyncedToId == Id).ToList();
        }

        public void OnObjectsCreating() { }

        public void OnUpdating(RpgGraph graph, Time.Time time)
            => Lifecycle.UpdateLifecycle(graph, time, this);

        public string TargetPropName => $"{InitiatorId}.{Name}.{ModCmdArg.TargetArg}";
        public string DiceRollPropName => $"{InitiatorId}.{Name}.{ModCmdArg.DiceRollArg}";
        public string OutcomePropName => $"{InitiatorId}.{Name}.{ModCmdArg.OutcomeArg}";

        public ModSet AddState<TTarget>(TTarget target)
            where TTarget : RpgObject
        {
            var state = target.GetStateInstanceName(Name)!;

            var template = new SyncedMod(Id, nameof(ModSet))
                .SetBehavior(new ForceState())
                .SetProps(target, state, 1);

            var mod = new Mod(state, template);
            Add(mod);

            return this;
        }

        public ModSet Target<TEntity>(TEntity entity, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : RpgObject
        {
            var mod = Mod.Create(new Synced(), TargetPropName, entity, TargetPropName, dice, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet Target<TEntity, TSourceValue>(TEntity entity, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : RpgObject
        {
            var mod = Mod.Create(new Synced(), TargetPropName, entity, TargetPropName, entity, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet Target<TTarget, TSource, TSourceValue>(TTarget target, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = Mod.Create(new Synced(), TargetPropName, target, TargetPropName, source, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet Roll<TEntity>(TEntity entity, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : RpgObject
        {
            var mod = Mod.Create(new Synced(), DiceRollPropName, entity, DiceRollPropName, dice, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet Roll<TEntity, TSourceValue>(TEntity entity, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : RpgObject
        {
            var mod = Mod.Create(new Synced(), DiceRollPropName, entity, DiceRollPropName, entity, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet Roll<TTarget, TSource, TSourceValue>(TTarget target, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = Mod.Create(new Synced(), DiceRollPropName, target, DiceRollPropName, source, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet Outcome<TTarget>(TTarget target, int outcome, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : RpgObject
        {
            var mod = Mod.Create(new Synced(), OutcomePropName, target, OutcomePropName, outcome, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity>(TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var template = new SyncedMod(Id, nameof(ModSet));
            template.SetProps(entity, targetProp, dice, valueCalc);

            var mod = new Mod(targetProp, template);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity, TTargetValue, TSourceValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var template = new SyncedMod(Id, nameof(ModSet));
            template.SetProps(entity, targetExpr, entity, sourceExpr, valueCalc);

            var mod = new Mod(template.TargetPropRef.Prop, template);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity, TSourceValue>(TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var template = new SyncedMod(Id, nameof(ModSet));
            template.SetProps(entity, targetProp, entity, sourceExpr, valueCalc);

            var mod = new Mod(template.TargetPropRef.Prop, template);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TTarget, TSource, TSourceValue>(TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var template = new SyncedMod(Id, nameof(ModSet));
            template.SetProps(target, targetProp, source, sourceExpr, valueCalc);

            var mod = new Mod(template.TargetPropRef.Prop, template);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity, TTargetValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var template = new SyncedMod(Id, nameof(ModSet));
            template.SetProps(entity, targetExpr, dice, valueCalc);

            var mod = new Mod(template.TargetPropRef.Prop, template);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity, TTarget, TTargetValue, TSourceValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var template = new SyncedMod(Id, nameof(ModSet));
            template.SetProps(entity, targetExpr, entity, sourceExpr, valueCalc);

            var mod = new Mod(template.TargetPropRef.Prop, template);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var template = new SyncedMod(Id, nameof(ModSet));
            template.SetProps(target, targetExpr, source, sourceExpr, valueCalc);

            var mod = new Mod(template.TargetPropRef.Prop, template);
            Add(mod);

            return this;
        }
    }
}
