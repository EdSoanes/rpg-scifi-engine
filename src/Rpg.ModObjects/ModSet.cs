using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects
{
    public class ModSet<T> : ModSet
        where T : ModObject
    {
        public ModSet(string name)
            : base(name)
        { }

        public ModSet(string name, ModDuration duration)
            : base(name, duration)
        { }
    }

    public class ModSet : ITemporal
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string? Name { get; set; }
        [JsonIgnore] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] private ModDuration Duration { get; set; } = new ModDuration();
        [JsonProperty] private Guid[] ModIds { get; set; } = new Guid[0];

        [JsonConstructor] private ModSet() { }

        public ModSet(string name, ModDuration duration)
        {
            Name = name;
            Duration = duration;
        }

        public ModSet(string name)
            : this (name, ModDuration.External())
        {
        }

        public ModSet(string name, ModDuration duration, params Mod[] mods)
            : this(name, duration)
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

        public ModSet AddMod<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = ExternalMod.Create(entity, targetExpr, value, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = ExternalMod.Create(target, targetExpr, source, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddMod<TTarget, TSource, TSourceValue>(TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = ExternalMod.Create(target, targetProp, source, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddTurnMod<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = TurnMod.Create(entity, targetExpr, value, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddTurnMod<TTarget, TTargetValue, TSource, TSourceValue>(int turn, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = TurnMod.Create(target, targetExpr, source, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddSumMod<TTarget, TTargetValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = SumMod.Create(target, targetExpr, value, diceCalcExpr);
            Add(mod);

            return this;
        }

        public ModSet AddSumMod<TTarget, TTargetValue, TSource, TSourceValue>(int turn, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = SumMod.Create(target, targetExpr, source, sourceExpr, diceCalcExpr);
            Add(mod);

            return this;
        }
    }
}
