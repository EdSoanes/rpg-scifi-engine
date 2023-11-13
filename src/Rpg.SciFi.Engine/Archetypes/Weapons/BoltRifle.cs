using Rpg.SciFi.Engine.Archetypes.Abilities;
using Rpg.SciFi.Engine.Turns;

namespace Rpg.SciFi.Engine.Archetypes.Weapons
{
    public class BoltRifle : Weapon, IRangedArtifact, IPoweredArtifact
    {
        public class Reload : Ability
        {
            public Reload()
            {
                Name = "Reload";
                Description = "Reload the weapon ammunition";
                ActionPointCost = 4;
                Exertion = 1;
            }

            [Input(Param = "weapon", BindsTo = "this")]
            [Input(Param = "character", BindsTo = "CharacterSheet.Character")]
            [Input(Param = "encounter", BindsTo = "CharacterSheet.Encounter")]
            public TurnAction[]? Use(Weapon weapon, Character character, Encounter encounter)
            {
                return null;
            }
        }

        public BoltRifle()
        {
            Name = "Bolt Rifle";
            Description = "Standard military bolt rifle";

            Damage.Pierce.Value = "d10";
            Damage.Impact.Value = "d10";

            Abilities = new Ability[]
            {
                new FireSingleShot(),
                new FireBurstShot(),
                new Reload()
            };
        }

        protected override void OnAbilityUsed(Ability ability)
        {
        }

        public void Consume(int count)
        {
            PowerSource?.Consume(count);
        }

        public void Refill(int count)
        {
            PowerSource?.Refill(count);
        }

        public int BaseRange => 1000;

        public int EffectDropOffThreshold => 100;

        public string[] AllowedPowerSources => new[]
        {
            "medium-ap-bullet"
        };

        public ConsumableArtifact? PowerSource {  get; set; }
    }
}
