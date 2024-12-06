using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Behaviors
{
    public class Combine : BaseBehavior
    {
        public override void OnAdding(RpgGraph graph, Prop prop, Mod mod)
        {
            var matchingMods = GetMatchingMods<ExpiresOn>(graph, mod);
            var value = Dice.Add(
                ModCalculator.Value(graph, matchingMods),
                mod.Value()
            );

            mod.Set(value);

            foreach (var matchingMod in matchingMods)
            {
                prop.Remove(matchingMod);
                RemoveScopedMods(graph, matchingMod);
            }

            base.OnAdding(graph, prop, mod);
        }
    }
}
