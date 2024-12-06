using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Behaviors
{
    public class ExpiresOn : BaseBehavior
    {
        [JsonProperty] public Dice Value { get; protected set; }

        [JsonConstructor] protected ExpiresOn() { }

        public ExpiresOn(int value)
            : base()
        {
            Value = value;
        }

        public override void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            var matchingMods = GetMatchingMods<ExpiresOn>(graph, mod);
            var value = Dice.Add(
                ModCalculator.Value(graph, matchingMods),
                mod.Value()
            );

            mod.Set(Value == value ? null : value);

            foreach (var matchingMod in matchingMods)
            {
                modProp.Remove(matchingMod);
                RemoveScopedMods(graph, matchingMod);
            }

            base.OnAdding(graph, modProp, mod);
            if (mod.SourceValue != null && mod.SourceValue != Value)
            {
                modProp.Add(mod);
                CreateScopedMods(graph, mod);
            }
        }
    }
}
