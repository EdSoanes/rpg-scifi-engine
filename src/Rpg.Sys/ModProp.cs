using Newtonsoft.Json;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys
{
    public class ModProp
    {
        private Graph Graph;

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public Guid EntityId { get; private set; }
        [JsonProperty] public string Prop { get; private set; }
        [JsonProperty] public string ReturnType {  get; private set; }

        [JsonProperty] public Dice Value {  get; private set; }
        [JsonProperty] public Dice BaseValue { get; private set; }
        [JsonProperty] public string[] Description { get; private set; }

        [JsonProperty] private List<Modifier> Modifiers { get; set; } = new List<Modifier>();

        [JsonIgnore]
        public Modifier[] AllModifiers {  get => Modifiers.ToArray(); }

        [JsonIgnore]
        public Modifier[] BaseModifiers
        {
            get
            {
                var baseMods = Modifiers
                    .Where(x => x.Duration.GetExpiry(Graph.Turn) == ModifierExpiry.Active && x.ModifierType == ModifierType.Base)
                    .ToArray();

                return baseMods;
            }
        }

        [JsonIgnore]
        public Modifier[] FilteredModifiers
        {
            get
            {
                var activeModifiers = Modifiers.Where(x => x.Duration.GetExpiry(Graph.Turn) == ModifierExpiry.Active);

                var res = activeModifiers
                    .Where(x => x.ModifierType != ModifierType.Base && x.ModifierType != ModifierType.BaseOverride)
                    .ToList();

                var baseMods = activeModifiers
                    .Where(x => x.ModifierType == ModifierType.BaseOverride)
                    .ToList();

                if (!baseMods.Any())
                    baseMods = activeModifiers
                        .Where(x => x.ModifierType == ModifierType.Base)
                        .ToList();

                res.AddRange(baseMods);
                return res.ToArray();
            }
        }

        public Modifier[] MatchingModifiers(string name, ModifierType modifierType)
            => Modifiers
                .Where(x => x.Name == name && x.ModifierType == modifierType)
                .ToArray();

        public ModProp(Graph graph, Guid id, string prop, string returnType)
        {
            Graph = graph;

            EntityId = id;
            Prop = prop;
            ReturnType = returnType;
        }

        public bool Add(IEnumerable<Modifier> mods)
        {
            var res = false;
            foreach (var mod in mods)
                res |= Add(mod);
            return res;
        }

        public bool Add(Modifier mod)
        {
            var existing = Modifiers.FirstOrDefault(x => x.Id == mod.Id);
            if (existing != null)
                Modifiers.Remove(existing);

            if (mod.ModifierAction == ModifierAction.Accumulate)
            {
                Modifiers.Add(mod);
                return true;
            }

            if (mod.ModifierAction == ModifierAction.Sum)
            {
                return Sum(mod);
            }

            if (mod.ModifierAction == ModifierAction.Replace)
            {
                return Replace(mod);
            }

            return false;
        }

        private bool Sum(Modifier mod)
        {
            var matchingMods = MatchingModifiers(mod.Name, mod.ModifierType);
            var dice = Graph.Evaluate.Mod(matchingMods) + Graph.Evaluate.Mod(mod);

            Modifiers = Modifiers.Except(matchingMods).ToList();
            if (dice != Dice.Zero || mod.Duration.EndTurn != RemoveTurn.WhenZero)
            {
                mod.SetDice(dice);
                Modifiers.Add(mod);
            }

            return true;
        }

        private bool Replace(Modifier mod)
        {
            var matchingMods = Modifiers
                .Where(x => x.Name == mod.Name && x.ModifierType == mod.ModifierType)
                .ToArray();

            Modifiers = Modifiers.Except(matchingMods).ToList();
            Modifiers.Add(mod);

            return true;
        }

        public Modifier[] UpdateOnTurn(int newTurn)
        {
            var updated = new List<Modifier>();
            foreach (var mod in Modifiers)
            {
                mod.OnUpdate(newTurn);

                var expiry = mod.Duration.GetExpiry(Graph.Turn);
                if (expiry == ModifierExpiry.Remove)
                    updated.Add(mod);
                else
                {
                    var oldExpiry = mod.Duration.GetExpiry(Graph.Turn - 1);
                    if (expiry != oldExpiry)
                        updated.Add(mod);
                }
            }

            foreach (var mod in updated.Where(x => x.Duration.GetExpiry(Graph.Turn) == ModifierExpiry.Remove))
                Modifiers.Remove(mod);

            return updated.ToArray();
        }

        public Modifier[] Remove(params Modifier[] mods)
        {
            var toRemove = Modifiers
                .Where(x => mods.Select(m => m.Id).Contains(x.Id))
                .ToArray();

            foreach (var mod in toRemove)
                Modifiers.Remove(mod);

            return toRemove;
        }

        public Modifier[] Remove()
        {
            var res = Modifiers.ToArray();
            Modifiers.Clear();
            return res;
        }

        public void Clear()
        {
            var toRemove = Modifiers
                .Where(x => x.Duration.CanClear(Graph.Turn))
                .ToArray();

            foreach (var remove in toRemove)
                Modifiers.Remove(remove);
        }

        public bool AffectedBy(Guid id, string prop)
            => Modifiers.Any(x => x.Source.Id == id &&  x.Source.Prop == prop);

        public bool Contains(Modifier mod)
            => Modifiers.Any(x => x.Id == mod.Id);
    }
}
