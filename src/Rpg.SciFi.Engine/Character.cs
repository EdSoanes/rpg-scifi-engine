using Rpg.SciFi.Engine.Archetypes.Abilities;
using Rpg.SciFi.Engine.Characters;

namespace Rpg.SciFi.Engine
{
    public class Character : Artifact
    {
        public Character() 
        {
            Emissions.Heat.Value = "36";
            Abilities = new Ability[]
            {
                new Look(),
                new Jump(),
                new PickUp(),
                new Drop()
            };
        }

        public CharAttrBlock Attributes { get; set; } = new CharAttrBlock();
        public DamageSignature Damages { get; set; } = new DamageSignature();

        public List<Artifact> Equipment { get; set; } = new List<Artifact>();

        public void PickUp(Artifact artifact)
        {
            if (!Equipment.Contains(artifact))
                Equipment.Add(artifact);
        }

        public void Drop(Artifact artifact)
        {
            if (Equipment.Contains(artifact))
                Equipment.Remove(artifact);
        }

        protected override void OnAbilityUsed(Ability ability)
        {
        }
    }
}
