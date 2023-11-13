using Rpg.SciFi.Engine.Archetypes.Abilities;

namespace Rpg.SciFi.Engine.Archetypes.Containers
{
    public class Backpack : Artifact, IContainerArtifact
    {
        public Backpack() 
        {
            Name = "Backpack";
            Description = "Simple canvas backpack";

            Abilities = new Ability[]
            {
                new Stow(),
                new Take()
            };
        }

        public List<Artifact> Contents => new List<Artifact>();

        public int Capacity => 10;
    }
}
