using Rpg.SciFi.Engine.Turns;

namespace Rpg.SciFi.Engine.Archetypes.Abilities
{
    public class FireBurstShot : Ability
    {
        public FireBurstShot()
        {
            Description = "Fire 3 rounds of ammunition";
            ActionPointCost = 2;
            Exertion = 1;
        }

        [Input(Param = "weapon", BindsTo = "this")]
        [Input(Param = "character", BindsTo = "CharacterSheet.Character")]
        [Input(Param = "encounter", BindsTo = "CharacterSheet.Encounter")]
        public TurnAction[]? Use(Weapon weapon, Character character, Encounter encounter)
        {
            var ammo = (weapon as IPoweredArtifact)?.PowerSource as IAmmunitionConsumable;

            return new[]
            {
                new TurnAction
                {
                    ActionPointCost = ActionPointCost,
                    ExertionCost = Exertion,
                    SuccessRoll = 45, //Calculate the chance of success based on current state
                    UsingArtifact = weapon,
                    SuccessConsequences = new[]
                    {
                        new Consequence
                        {
                            Target = character,
                            DurationType = "Permanent",
                            BindsTo = "Attributes.HitPoints.Value",
                            Value = "-2d6+2"
                        },
                        new Consequence
                        {
                            Target = weapon,
                            DurationType = "Permanent",
                            BindsTo = "PowerSource.Consume()",
                            Value = "3"
                        },
                        new Consequence
                        {
                            Target = weapon,
                            DurationType = "Turn",
                            Turns = "1",
                            BindsTo = "Emissions.Sound.Value",
                            Value = "2"
                        }
                    },
                    FailConsequences = new[]
                    {
                        new Consequence
                        {
                            Target = weapon,
                            DurationType = "Permanent",
                            BindsTo = "PowerSource.Consume()",
                            Value = "3"
                        },
                        new Consequence
                        {
                            Target = weapon,
                            DurationType = "Turn",
                            Turns = "1",
                            BindsTo = "Emissions.Sound.Value",
                            Value = "2"
                        }
                    }
                },
            };
        }
    }
}

