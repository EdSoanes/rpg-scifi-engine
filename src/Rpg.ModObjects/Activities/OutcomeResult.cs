using Rpg.ModObjects.Mods;

namespace Rpg.ModObjects.Activities
{
    public class OutcomeResult : ActionResult
    {
        public List<ModSet> States { get; set; } = new();
        public List<ActionRef> NextActions { get; set; } = new();
    }
}
