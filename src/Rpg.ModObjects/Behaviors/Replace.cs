using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;

namespace Rpg.ModObjects.Behaviors
{
    public class Replace : BaseBehavior
    {
        public Replace() { }

        public override void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            var oldMods = modProp.Get((x) => x.GetType() == mod.GetType() && x.Name == mod.Name);
            foreach (var oldMod in oldMods)
            {
                RemoveScopedMods(graph, oldMod);
                modProp.Remove(oldMod);
            }

            base.OnAdding(graph, modProp, mod);
        }
    }
}
