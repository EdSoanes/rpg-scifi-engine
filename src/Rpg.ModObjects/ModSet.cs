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
        [JsonProperty] private ModDuration Duration { get; set; } = new ModDuration();
        [JsonProperty] private Guid[] ModIds { get; set; } = new Guid[0];

        [JsonConstructor] protected ModSet() { }

        public ModSet(Guid initiatorId, string name, ModDuration duration)
        {
            InitiatorId = initiatorId;
            Name = name;
            Duration = duration;
        }

        public ModSet(Guid initiatorId, string name)
            : this(initiatorId, name, ModDuration.External())
        {
        }

        public ModSet(Guid initiatorId, string name, ModDuration duration, params Mod[] mods)
            : this(initiatorId, name, duration)
        {
            Add(mods);
        }

        public ModSet Add(params Mod[] mods)
        {
            Mods.AddRange(mods);
            ModIds = Mods.Select(x => x.Id).ToArray();

            SetModDuration();

            return this;
        }

        private void SetModDuration()
        {
            foreach (var mod in Mods.Where(x => x.Duration.Type == ModDurationType.External))
                mod.Duration.SetDuration(Duration.StartTurn, Duration.EndTurn);
        }

        public ModExpiry GetExpiry(int turn)
            => Duration.GetExpiry(turn);

        public void SetExpired()
        {
            Duration.SetExpired();
            SetModDuration();
        }

        public void SetPending(int turn)
        {
            Duration.SetPending(turn);
            SetModDuration();
        }

        public void SetActive()
        {
            Duration.SetActive();
            SetModDuration();
        }

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null)
        {
            if (!Mods.Any())
                Mods = graph.GetAllMods().Where(x => ModIds.Contains(x.Id)).ToList();
        }

        public void OnTurnChanged(int turn) { }
        public void OnBeginEncounter() { }

        public void OnEndEncounter()
        {
            var toRemove = new List<Mod>();

            foreach (var mod in Mods)
            {
                var expiry = mod.Duration.GetExpiry(0);
                if (expiry == ModExpiry.Expired && mod.Duration.CanRemove(0))
                    toRemove.Add(mod);
            }

            foreach (var mod in toRemove)
            {
                Mods.Remove(mod);
                ModIds = Mods.Select(x => x.Id).ToArray();
            }
        }

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
            var mod = Mod.Create<StateMod, TTarget>(state, target, state, 1);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Target<TEntity>(this ModSet modSet, TEntity entity, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity>(modSet.TargetProp, entity, modSet.TargetProp, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Target<TEntity, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity, TEntity, TSourceValue>(modSet.TargetProp, entity, modSet.TargetProp, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Target<TTarget, TSource, TSourceValue>(this ModSet modSet, TTarget target, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create<ExternalMod, TTarget, TSource, TSourceValue>(modSet.TargetProp, target, modSet.TargetProp, source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Roll<TEntity>(this ModSet modSet, TEntity entity, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity>(modSet.DiceRollProp, entity, modSet.DiceRollProp, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Roll<TEntity, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity, TEntity, TSourceValue>(modSet.DiceRollProp, entity, modSet.DiceRollProp, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Roll<TTarget, TSource, TSourceValue>(this ModSet modSet, TTarget target, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create<ExternalMod, TTarget, TSource, TSourceValue>(modSet.DiceRollProp, target, modSet.DiceRollProp, source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Outcome<TTarget>(this ModSet modSet, TTarget target, int outcome, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create<ExternalMod, TTarget>(modSet.OutcomeProp, target, modSet.OutcomeProp, outcome, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity>(this ModSet modSet, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity>(entity, targetProp, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity, TTargetValue, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity, TTargetValue, TEntity, TSourceValue>(entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity, TSourceValue>(this ModSet modSet, TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity, TEntity, TSourceValue>(entity, targetProp, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TTarget, TSource, TSourceValue>(this ModSet modSet, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create<ExternalMod, TTarget, TSource, TSourceValue>(target, targetProp, source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity, TTargetValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity, TTargetValue>(entity, targetExpr, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity, TTarget, TTargetValue, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity, TTargetValue, TEntity, TSourceValue>(entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddTurnMod<TEntity>(this ModSet modSet, TEntity entity, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<TurnMod, TEntity>(entity, targetProp, value, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddTurnMod<TTarget, TTargetValue>(this ModSet modSet, TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create<TurnMod, TTarget, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddSumMod<TTarget, TTargetValue>(this ModSet modSet, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create<SumMod, TTarget, TTargetValue>(target, targetExpr, value, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddSumMod<TTarget, TTargetValue, TSource, TSourceValue>(this ModSet modSet, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create<SumMod, TTarget, TTargetValue, TSource, TSourceValue>(target, targetExpr, source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddPermanentMod<TEntity>(this ModSet modSet, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<PermanentMod, TEntity>(entity, targetProp, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddPermanentMod<TEntity, TTargetValue, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<PermanentMod, TEntity, TTargetValue, TEntity, TSourceValue>(entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddPermanentMod<TEntity, TTargetValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<PermanentMod, TEntity, TTargetValue>(entity, targetExpr, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddPermanentMod<TEntity, TTarget, TTargetValue, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<PermanentMod, TEntity, TTargetValue, TEntity, TSourceValue>(entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }
    }
}
