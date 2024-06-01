using Newtonsoft.Json;
using Rpg.ModObjects.Cmds;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class ModSet : IGraphEvents
    {
        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string InitiatorId { get; private set; }
        [JsonProperty] public string Name { get; set; }
        [JsonIgnore] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] public ModBehavior Behavior { get; private set; }
        [JsonConstructor] protected ModSet() { }

        public ModSet(string initiatorId, string name, ModBehavior behavior)
        {
            Id = this.NewId();
            InitiatorId = initiatorId;
            Behavior = behavior;
            Name = name;
        }

        public ModSet(string initiatorId, string name)
            : this(initiatorId, name, new Synced())
        {
        }

        public ModSet(string initiatorId, string name, ModBehavior behavior, params Mod[] mods)
            : this(initiatorId, name, behavior)
        {
            Add(mods);
        }

        public ModSet Add(params Mod[] mods)
        {
            foreach (var mod in mods)
            {
                mod.ModSetId = Id;
                if (mod.Behavior.Type == ModType.Synced)
                    mod.Behavior.SyncWith(Behavior);
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

        private void SyncBehavior()
        {
            foreach (var mod in Mods.Where(x => x.Behavior.Type == ModType.Synced))
                mod.Behavior.SyncWith(Behavior);
        }

        public void SetExpired()
        {
            Behavior.SetExpired();
            SyncBehavior();
        }

        public void OnGraphCreating(RpgGraph graph, RpgObject? entity = null)
        {
            if (!Mods.Any())
                Mods = graph.GetMods().Where(x => x.ModSetId == Id).ToList();
        }

        public void OnObjectsCreating() { }

        public void OnUpdating(RpgGraph graph)
            => Behavior.OnUpdating(graph);

        public string TargetPropName => $"{InitiatorId}.{Name}.{ModCmdArg.TargetArg}";
        public string DiceRollPropName => $"{InitiatorId}.{Name}.{ModCmdArg.DiceRollArg}";
        public string OutcomePropName => $"{InitiatorId}.{Name}.{ModCmdArg.OutcomeArg}";

        public ModSet AddState<TTarget>(TTarget target)
            where TTarget : RpgObject
        {
            var state = target.GetStateInstanceName(Name)!;
            var mod = Mod.Create(new ForceState(), state, target, state, 1);
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

        public ModSet AddMod<TEntity>(ModBehavior behavior, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : RpgObject
        {
            var mod = Mod.Create(behavior, entity, targetProp, dice, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity, TTargetValue, TSourceValue>(ModBehavior behavior, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : RpgObject
        {
            var mod = Mod.Create(behavior, entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity, TSourceValue>(ModBehavior behavior, TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : RpgObject
        {
            var mod = Mod.Create(behavior, entity, targetProp, entity, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TTarget, TSource, TSourceValue>(ModBehavior behavior, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = Mod.Create(behavior, target, targetProp, source, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity, TTargetValue>(ModBehavior behavior, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : RpgObject
        {
            var mod = Mod.Create(behavior, entity, targetExpr, dice, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TEntity, TTarget, TTargetValue, TSourceValue>(ModBehavior behavior, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : RpgObject
        {
            var mod = Mod.Create(behavior, entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TTarget, TTargetValue, TSource, TSourceValue>(ModBehavior behavior, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = Mod.Create(behavior, target, targetExpr, source, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }
    }
}
