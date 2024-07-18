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
            Strength = new PropValue(Id, nameof(Strength), template.Strength);
            Agility = new PropValue(Id, nameof(Agility), template.Agility);
            Health = new PropValue(Id, nameof(Health), template.Health);
            Brains = new PropValue(Id, nameof(Brains), template.Brains);
            Insight = new PropValue(Id, nameof(Insight), template.Insight);
            Charisma = new PropValue(Id, nameof(Charisma), template.Charisma);
        }

        protected override void OnLifecycleStarting()
        {
            base.OnLifecycleStarting();
            this.InitActionsAndStates(Graph!);
        }
    }
}
