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
            var source = graph.Entities?.Get(SourceContainerId) as Container;
            var target = graph.Entities?.Get(TargetContainerId) as Container;

            var artifact = source?.Remove(ArtifactId) ?? graph.Entities!.Get(ArtifactId) as Artifact;
            if (artifact != null)
                target?.Add(artifact);
        }
    }
}

