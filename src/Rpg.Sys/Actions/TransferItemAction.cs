using Rpg.Sys.Archetypes;

namespace Rpg.Sys.Actions
{
    public class TransferItemAction : ActionBase
    {
        public Guid? SourceContainerId { get; set; }
        public Guid? TargetContainerId { get; set; }
        public Guid ArtifactId { get; set; }

        public TransferItemAction(string name, Container? source, Container? target, Artifact artifact)
            : base(name, (source?.RemoveCost ?? new ActionCost()) + (target?.AddCost ?? new ActionCost()))
        {
            SourceContainerId = source?.Id;
            TargetContainerId = target?.Id;
            ArtifactId = artifact.Id;
        }

        protected override ActionBase? NextAction()
        {
            return null;
        }

        protected override void OnResolve(Actor actor, Graph graph)
        {
            var source = graph.Get.Entity<Container>(SourceContainerId);
            var target = graph.Get.Entity<Container>(TargetContainerId);

            var artifact = source?.Remove(ArtifactId) ?? graph.Get.Entity<Artifact>(ArtifactId);
            if (artifact != null)
                target?.Add(artifact);
        }
    }
}

