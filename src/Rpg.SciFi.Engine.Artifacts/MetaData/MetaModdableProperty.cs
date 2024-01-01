using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class MetaModdableProperty
    {
        public Guid Id { get; set; }
        public string Prop { get; set; }

        public List<Modifier> Modifiers { get; set; } = new List<Modifier>();

        [JsonConstructor] private MetaModdableProperty() { }

        public MetaModdableProperty(Guid id, string prop)
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
