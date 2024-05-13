using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects
{
    public class ModSet<T> : ModSet
        where T : ModObject
    {
        public ModSet()
            : base(ModDuration.External())
        {
        }

        public ModSet<T> Add<T1>(T entity, Expression<Func<T, T1>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
        {
            var mod = ExternalMod.Create(entity, targetExpr, dice, diceCalcExpr);
            Add(mod);

            return this;
        }
    }

    public class ModSet : ITemporal
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string? Name { get; set; }
        [JsonIgnore] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] private ModDuration Duration { get; set; } = new ModDuration();
        [JsonProperty] private Guid[] ModIds { get; set; } = new Guid[0];

        [JsonConstructor] private ModSet() { }

        public ModSet(ModDuration duration)
        {
            Duration = duration;
        }

        public ModSet(ModDuration duration, params Mod[] mods)
            : this(duration)
        {
            Add(mods);
        }

        public void Add(params Mod[] mods)
        {
            Mods.AddRange(mods);
            ModIds = Mods.Select(x => x.Id).ToArray();

            SetModDuration();
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

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null) { }
        public void OnGraphCreated(ModGraph graph)
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
    }
}
