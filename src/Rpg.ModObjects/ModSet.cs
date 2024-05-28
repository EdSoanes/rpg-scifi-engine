using Newtonsoft.Json;
using Rpg.ModObjects.Cmds;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects
{
    public class ModSet : ITemporal
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public Guid InitiatorId { get; private set; }
        [JsonProperty] public string Name { get; set; }
        [JsonIgnore] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] public ModBehavior Behavior { get; private set; }
        [JsonConstructor] protected ModSet() { }

        public ModSet(Guid initiatorId, string name, ModBehavior behavior)
        {
            InitiatorId = initiatorId;
            Name = name;
            Behavior = behavior;
        }

        public ModSet(Guid initiatorId, string name)
            : this(initiatorId, name, new Synced())
        {
        }

        public ModSet(Guid initiatorId, string name, ModBehavior behavior, params Mod[] mods)
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

        public ModSubSet[] SubSets(ModGraph graph)
        {
            var subSets = new List<ModSubSet>();
            foreach (var modGroup in Mods.GroupBy(x => new ModPropRef(x.EntityId, x.Prop)))
            {
                var mods = modGroup.ToArray();
                var dice = graph.CalculateModsValue(mods);

                var subSet = new ModSubSet(modGroup.Key, mods, dice);
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

        public ModExpiry GetExpiry(ModGraph graph)
            => Behavior.GetExpiry(graph);

        public void SetExpired()
        {
            Behavior.SetExpired();
            SyncBehavior();
        }

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null)
        {
            if (!Mods.Any())
                Mods = graph.GetAllMods().Where(x => x.ModSetId == Id).ToList();
        }

        public void OnTurnChanged(int turn) { }
        public void OnBeginEncounter() { }
        public void OnEndEncounter() { }

        public string TargetProp => $"{InitiatorId}.{Name}.{ModCmdArg.TargetArg}";
        public string DiceRollProp => $"{InitiatorId}.{Name}.{ModCmdArg.DiceRollArg}";
        public string OutcomeProp => $"{InitiatorId}.{Name}.{ModCmdArg.OutcomeArg}";
    }

    public static class ModSetExtensions
    {
        public static ModSet AddState<TTarget>(this ModSet modSet, TTarget target)
            where TTarget : ModObject
        {
            var state = target.GetStateInstanceName(modSet.Name)!;
            var mod = Mod.Create(new ForceState() ,state, target, state, 1);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Target<TEntity>(this ModSet modSet, TEntity entity, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Synced(), modSet.TargetProp, entity, modSet.TargetProp, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Target<TEntity, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Synced(), modSet.TargetProp, entity, modSet.TargetProp, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Target<TTarget, TSource, TSourceValue>(this ModSet modSet, TTarget target, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create(new Synced(), modSet.TargetProp, target, modSet.TargetProp, source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Roll<TEntity>(this ModSet modSet, TEntity entity, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Synced(), modSet.DiceRollProp, entity, modSet.DiceRollProp, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Roll<TEntity, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Synced(), modSet.DiceRollProp, entity, modSet.DiceRollProp, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Roll<TTarget, TSource, TSourceValue>(this ModSet modSet, TTarget target, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create(new Synced(), modSet.DiceRollProp, target, modSet.DiceRollProp, source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Outcome<TTarget>(this ModSet modSet, TTarget target, int outcome, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create(new Synced(), modSet.OutcomeProp, target, modSet.OutcomeProp, outcome, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity>(this ModSet modSet, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Synced(), entity, targetProp, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity, TTargetValue, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Synced(), entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity, TSourceValue>(this ModSet modSet, TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Synced(), entity, targetProp, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TTarget, TSource, TSourceValue>(this ModSet modSet, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create(new Synced(), target, targetProp, source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity, TTargetValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Synced(), entity, targetExpr, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity, TTarget, TTargetValue, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Synced(), entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddTurnMod<TEntity>(this ModSet modSet, TEntity entity, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Turn(), entity, targetProp, value, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddTurnMod<TTarget, TTargetValue>(this ModSet modSet, TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create(new Turn(), entity, targetExpr, value, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddSumMod<TTarget, TTargetValue>(this ModSet modSet, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create(new ExpireOnZero(), target, targetExpr, value, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddSumMod<TTarget, TTargetValue, TSource, TSourceValue>(this ModSet modSet, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create(new ExpireOnZero(), target, targetExpr, source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddPermanentMod<TEntity>(this ModSet modSet, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Permanent(), entity, targetProp, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddPermanentMod<TEntity, TTargetValue, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Permanent(), entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddPermanentMod<TEntity, TTargetValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Permanent(), entity, targetExpr, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddPermanentMod<TEntity, TTarget, TTargetValue, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create(new Permanent(), entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }
    }
}
