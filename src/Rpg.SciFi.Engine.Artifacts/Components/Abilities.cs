using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public sealed class Ability
    {
        public const int PassiveAbility = -1;
        public const int FreeAbility = 0;

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; private set; } = nameof(Ability);
        [JsonProperty] public string Description { get; private set; } = string.Empty;
        [JsonProperty] public TurnPoints Costs { get; private set; } = new TurnPoints(0, 0, 0);
    }

    public sealed class Abilities
    {
        [JsonProperty] private List<Ability> _abilities { get; set; } = new List<Ability>();

        public Abilities() { }
        public Abilities(params Ability[] abilities)
        {
            _abilities.AddRange(abilities);
        }

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
            if (ability != null)
                _abilities.Remove(ability);
        }
    }
}
