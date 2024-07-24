using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Behaviors
{
    public class Replace : BaseBehavior
    {
        [JsonConstructor] protected Replace() { }

        public Replace(ModType modType)
        {
            Type = modType;
        }

        public override void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            base.OnAdding(graph, modProp, mod);
            var oldMods = modProp.Get((x) => x.Behavior.Type == mod.Behavior.Type && x.Name == mod.Name);

            foreach (var oldMod in oldMods)
                modProp.Remove(oldMod);

            //Don't add if the source is a Value without a ValueFunction and the Value = null
            if (mod.SourcePropRef != null || mod.SourceValue != null || mod.SourceValueFunc != null)
            {
                modProp.Add(mod);
                OnScoping(graph, modProp, mod);
            }
        }
    }
}
