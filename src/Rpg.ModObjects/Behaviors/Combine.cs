using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Behaviors
{
    public class Combine : BaseBehavior
    {
        public override void OnAdding(RpgGraph graph, Prop prop, Mod mod)
        {
            var matchingMods = MatchingMods<Combine>(graph, mod);
            var value = Dice.Add(
                graph.CalculateModsValue(matchingMods),
                graph.CalculateModValue(mod)
            );

            mod.Set(value);

            foreach (var matchingMod in matchingMods)
                prop.Remove(matchingMod);

            if (mod.SourceValue != null && mod.SourceValue != Dice.Zero)
            {
                prop.Add(mod);
                OnScoping(graph, prop, mod);
            }
        }

        protected Mod[] MatchingMods<T>(RpgGraph graph, Mod mod)
            where T : BaseBehavior
                => graph.GetMods(mod.TargetPropRef, x => x.Behavior is T && x.Behavior.Type == mod.Behavior.Type && x.Name == mod.Name);
    }
}
