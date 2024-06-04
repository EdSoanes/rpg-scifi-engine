using Rpg.ModObjects.Props;

namespace Rpg.ModObjects.Modifiers
{
    public interface IBehavior
    {
        ModExpiry Expiry { get; }
        ModScope Scope { get; }
        ModType Type { get; }

        BaseBehavior Clone<T>(ModScope scope = ModScope.Standard) where T : BaseBehavior;
        void OnAdding(RpgGraph graph, Prop modProp, Mod mod);
        bool OnRemoving(RpgGraph graph, Prop modProp, Mod mod);
        void OnUpdating(RpgGraph graph, Prop modProp, Mod mod);
    }
}