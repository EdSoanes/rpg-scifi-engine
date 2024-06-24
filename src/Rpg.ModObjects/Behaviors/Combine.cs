using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Behaviors
{
    public class Combine : BaseBehavior
    {
        public Combine(ModType modType)
        {
            Type = modType;
        }

        public override void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            var matchingMods = MatchingMods(graph, mod);
            var oldValue = graph.CalculateModsValue(matchingMods);
            var newValue = oldValue + graph.CalculateModValue(mod);

            mod.SetSource(newValue);

            foreach (var matchingMod in matchingMods)
                modProp.Remove(matchingMod);

            if (mod.SourceValue != null && mod.SourceValue != Dice.Zero)
            {
                modProp.Add(mod);
                OnScoping(graph, modProp, mod);
            }
        }

        protected Mod[] MatchingMods(RpgGraph graph, Mod mod)
            => graph.GetMods(mod, x => x.Behavior is ExpiresOn && x.Behavior.Type == mod.Behavior.Type && x.Name == mod.Name);
    }
}
