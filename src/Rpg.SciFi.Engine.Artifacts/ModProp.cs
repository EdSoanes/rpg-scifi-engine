using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class ModProp
    {
        public Guid Id { get; set; }
        public string Prop { get; set; }

        public List<Modifier> Modifiers { get; set; } = new List<Modifier>();

        [JsonConstructor] private ModProp() { }

        public ModProp(Guid id, string prop)
        {
            Id = id;
            Prop = prop;
        }

        public Modifier[] MatchingMods(Modifier mod)
        {
            return Modifiers
                .Where(x => x.Name == mod.Name && x.ModifierType == mod.ModifierType)
                .ToArray();
        }

        public void RemoveMatchingMods(Modifier mod)
        {
            var matching = MatchingMods(mod);
            Modifiers = Modifiers.Except(matching).ToList();
        }
    }
}
