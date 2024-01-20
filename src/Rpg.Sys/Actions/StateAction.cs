using Newtonsoft.Json;
using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;

namespace Rpg.Sys.Actions
{
    public class StateAction : ActionBase
    {
        [JsonProperty] public Guid ArtifactId { get; private set; }
        [JsonProperty] public string StateName { get; private set; }
        [JsonProperty] public bool Activate { get; private set; }

        [JsonConstructor] protected StateAction() { }

        public StateAction(Artifact artifact, IState state, bool activate)
            : base(state.Name, activate ? state.ActivateCost : state.DeactivateCost)
        {
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
            var artifact = graph.Get.Entity<Artifact>(ArtifactId);
            if (artifact != null)
            {
                if (Activate)
                    artifact.States.Activate(actor, StateName);
                else
                    artifact.States.Deactivate(actor, StateName);
            }
        }
    }
}

