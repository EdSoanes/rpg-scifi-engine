using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;

namespace Rpg.ModObjects.Behaviors
{
    public class Highest : BaseBehavior
    {
        public Highest()
        { }

        public override void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            var existing = modProp.Get((x) => x.GetType() == mod.GetType() && x.Name == mod.Name);
            var highestMod = HighestMod(graph, existing, mod);

            foreach (var oldMod in existing)
            {
                modProp.Remove(oldMod);
                RemoveScopedMods(graph, oldMod);
            }
            
            if (highestMod != null)
            {
                modProp.Add(mod);
                CreateScopedMods(graph, mod);
            }
        }

        private Mod? HighestMod(RpgGraph graph, Mod[] existing, Mod mod)
        {
            var val = graph.CalculateModValue(mod);
            if (!existing.Any())
                return val == null ? null : mod;
            
            foreach (var oldMod in existing)
            {
                var oldVal = graph.CalculateModValue(oldMod);
                if (val == null ||(oldVal != null && oldVal.Value.Roll() > val.Value.Roll()))
                    return oldMod;
            }

            return mod;
        }
    }
}
