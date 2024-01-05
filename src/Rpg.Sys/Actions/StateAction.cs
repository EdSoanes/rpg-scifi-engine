using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;

namespace Rpg.Sys.Actions
{
    public class StateAction : ActionBase
    {
        public Guid ActorId { get; set; }
        public Guid ArtifactId { get; set; }
        public string StateName { get; set; }
        public bool Activate { get; set; }

        public StateAction(Actor actor, Artifact artifact, IState state, bool activate)
            : base(state.Name, activate ? state.ActivateCost : state.DeactivateCost)
        {
            ActorId = actor.Id;
            ArtifactId = artifact.Id;
            StateName = state.Name;
            Activate = activate;
        }

        protected override ActionBase? NextAction()
        {
            return null;
        }

        protected override void OnResolve(Actor actor, Graph graph)
        {
            var artifact = graph.Entities?.Get(ArtifactId) as Artifact;
            if (artifact != null)
            {
                if (Activate)
                    artifact.States.Activate(StateName);
                else
                    artifact.States.Deactivate(StateName);
            }
        }
    }
}

