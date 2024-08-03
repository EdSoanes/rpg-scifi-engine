using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Behaviors
{
    public class ExpiresOn : Combine
    {
        [JsonProperty] public Dice Value { get; private set; }

        public ExpiresOn(int value)
            : base()
        {
            Value = value;
        }

        public override void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            var matchingMods = MatchingMods<ExpiresOn>(graph, mod);
            var value = Dice.Add(
                graph.CalculateModsValue(matchingMods),
                graph.CalculateModValue(mod)
            );

            mod.SetSource(value);

            foreach (var matchingMod in matchingMods)
                modProp.Remove(matchingMod);

            if (mod.SourceValue != null && mod.SourceValue != Value)
            {
                modProp.Add(mod);
                OnScoping(graph, modProp, mod);
            }
        }
    }
}
