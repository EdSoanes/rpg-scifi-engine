using Rpg.Sys.Components;

namespace Rpg.Sys.Archetypes
{
    public class ActorTemplate : ArtifactTemplate
    {
        public string? Class { get; set; }

        public StatPointsTemplate Stats { get; set; } = new StatPointsTemplate();

        public MovementTemplate Movement { get; set; } = new MovementTemplate();

        public ActionPointsTemplate Actions { get; set; } = new ActionPointsTemplate();
    }
}
