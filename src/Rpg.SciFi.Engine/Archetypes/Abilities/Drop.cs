using Rpg.SciFi.Engine.Turns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Archetypes.Abilities
{
    public class Drop : Ability
    {
        [Input(Param = "artifact", BindsTo = "this")]
        [Input(Param = "character", BindsTo = "CharacterSheet.Character")]
        [Input(Param = "encounter", BindsTo = "CharacterSheet.Encounter")]
        public TurnAction Use(Artifact artifact, Character character, Encounter encounter)
        {
            var item = character.Equipment.FirstOrDefault(x => x == artifact);
            var actionPointCost = item == null
                ? 0
                : ActionPointCost;

            var successRoll = item == null
                ? -1
                : 0;

            return new TurnAction
            {
                ActionPointCost = actionPointCost,
                UsingArtifact = artifact,
                ExertionCost = Exertion,
                SuccessRoll = successRoll,
                SuccessConsequences = new[]
                {
                    new Consequence
                    {
                        Target = character,
                        BindsTo = "Drop()",
                        Value = artifact
                    }
                }
            };
        }
    }
}
