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
            var oldMods = modProp.Get((x) => x.Behavior.Type == mod.Behavior.Type && x.Name == mod.Name);
            if (oldMods.Any())
            {
                var val = graph.CalculateModValue(mod);
                if (val == null)
                    return;

                foreach (var oldMod in oldMods)
                {
                    var oldVal = graph.CalculateModValue(oldMod);
                    if (oldVal != null && oldVal.Value.Roll() > val.Value.Roll())
                        return;
                }

                foreach (var oldMod in oldMods)
                    modProp.Remove(oldMod);
            }

            modProp.Add(mod);
            OnScoping(graph, modProp, mod);
        }
    }
}
