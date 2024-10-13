using Rpg.ModObjects.Behaviors;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Override : Mod
    {
        public Override()
            : base(nameof(Override))
        {
            Behavior = new Replace();
        }
    }
}
