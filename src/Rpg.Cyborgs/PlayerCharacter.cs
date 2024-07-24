using Newtonsoft.Json;
using Rpg.Cyborgs.Components;
using Rpg.ModObjects;

namespace Rpg.Cyborgs
{
    public class PlayerCharacter : Actor
    {
        [JsonConstructor] protected PlayerCharacter() { }

        public PlayerCharacter(PlayerCharacterTemplate template)
            : base(template.Name)
        {
            Strength = new PropValue(template.Strength);
            Agility = new PropValue(template.Agility);
            Health = new PropValue(template.Health);
            Brains = new PropValue(template.Brains);
            Insight = new PropValue(template.Insight);
            Charisma = new PropValue(template.Charisma);
        }
    }
}
