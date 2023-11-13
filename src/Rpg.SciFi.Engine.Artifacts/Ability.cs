using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Ability
    {
        public const int PassiveAbility = -1;
        public const int FreeAbility = 0;

        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; } = nameof(Ability);
        [JsonProperty] public string Description { get; protected set; } = string.Empty;
        [JsonProperty] public int ActionPointCost { get; protected set; } = FreeAbility;
        [JsonProperty] public int Exertion { get; protected set; } = 0;
    }

    public class Abilities
    {
        [JsonProperty] private List<Ability> _abilities { get; set; } = new List<Ability>();

        public void AddAbility(Ability ability)
        {
            if (!_abilities.Any(x => x.Id == ability.Id))
                _abilities.Add(ability);
        }

        public void AddAbilities(Ability[] abilities)
        {
            foreach (var ability in abilities)
                AddAbility(ability);
        }

        public void RemoveAbility(Guid id)
        {
            var ability = _abilities.FirstOrDefault(x => x.Id == id);
            if(ability != null)
                _abilities.Remove(ability);
        }
    }
}
