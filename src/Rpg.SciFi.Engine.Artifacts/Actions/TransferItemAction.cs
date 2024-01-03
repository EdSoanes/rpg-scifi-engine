using Rpg.SciFi.Engine.Artifacts.Archetypes;
using Rpg.SciFi.Engine.Artifacts.Components;

namespace Rpg.SciFi.Engine.Artifacts.Actions
{
    public class TransferItemAction : BaseAction
    {
        public Guid SourceContainerId { get; set; }
        public Guid TargetContainerId { get; set; }
        public Guid ArtifactId { get; set; }

        public TransferItemAction(EntityGraph graph, string name, Container source, Container target, Artifact artifact)
            : base(graph, name, source.RemoveCost + target.AddCost)
        {
            SourceContainerId = source.Id;
            TargetContainerId = target.Id;
            ArtifactId = artifact.Id;
        }

        protected override BaseAction? NextAction()
        {
            return null;
        }

        protected override void OnAct(Actor actor, int diceRoll = 0)
        {
            var source = Graph?.Entities?.Get(SourceContainerId) as Container;
            var target = Graph?.Entities?.Get(TargetContainerId) as Container;

            var artifact = source?.Remove(ArtifactId);
            if (artifact != null)
                target?.Add(artifact);
        }
    }
}

