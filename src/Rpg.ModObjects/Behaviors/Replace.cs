using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;

namespace Rpg.ModObjects.Behaviors
{
    public class Replace : BaseBehavior
    {
        public Replace(ModType modType)
        {
            Type = modType;
        }

        public override void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            base.OnAdding(graph, modProp, mod);
            var oldMods = modProp.Get(mod.Behavior.Type, mod.Name);

            foreach (var oldMod in oldMods)
                modProp.Remove(oldMod);

            //Don't add if the source is a Value without a ValueFunction and the Value = null
            if (mod.SourcePropRef != null || mod.SourceValue != null || mod.SourceValueFunc.IsCalc)
            {
                modProp.Add(mod);
                OnScoping(graph, modProp, mod);
            }
        }
    }
}
