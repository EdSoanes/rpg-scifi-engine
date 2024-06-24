using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;

namespace Rpg.ModObjects.Behaviors
{
    public class Add : BaseBehavior
    {
        public Add(ModType modType)
        {
            Type = modType;
        }

        public override void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            if (!modProp.Contains(mod))
                modProp.Mods.Add(mod);
        }
    }
}
