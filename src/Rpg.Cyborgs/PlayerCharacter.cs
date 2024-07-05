using Rpg.ModObjects;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs
{
    public class PlayerCharacter : Actor
    {
        [JsonConstructor] protected PlayerCharacter() { }

        public PlayerCharacter(PlayerCharacterTemplate template)
            : base(template.Name)
        {
            Strength = template.Strength;
            Agility = template.Agility;
            Health = template.Health;
            Brains = template.Brains;
            Insight = template.Insight;
            Charisma = template.Charisma;
        }

        protected override void OnLifecycleStarting()
        {
            base.OnLifecycleStarting();
            this.InitActionsAndStates(Graph!);
        }
    }
}
