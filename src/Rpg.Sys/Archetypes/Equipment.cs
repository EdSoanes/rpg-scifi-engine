using Newtonsoft.Json;
using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.Archetypes
{
    public class Equipment : Artifact
    {
        [JsonConstructor] protected Equipment() { }
        
        public Equipment(ArtifactTemplate template)
            : base(template)
        {
            States.Add(
                new OnState(Id)
            );
        }
    }

    public class OnState : State<Equipment> 
    {
        public OnState(Guid id) 
            : base(id, "On") 
        {
        }

        protected override Modifier[] Effects(Actor actor, Equipment artifact)
        {
            return new[]
            {
                StateModifier.Create(artifact.Id, Name, artifact, 1, x => x.Presence.Sound.Current)
            };
        }
    }
}
