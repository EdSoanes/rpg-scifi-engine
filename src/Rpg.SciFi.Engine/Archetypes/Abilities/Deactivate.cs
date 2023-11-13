using Rpg.SciFi.Engine.Turns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Archetypes.Abilities
{
    public class Deactivate : Ability
    {
        public Deactivate()
        {
            ActionPointCost = 1;
        }

        [Input(Param = "artifact", BindsTo = "this")]
        [Input(Param = "actionPoints", BindsTo = "Character.Attributes.ActionPoints.Value")]
        public TurnAction Use(Artifact artifact, int actionPoints)
        {
            var successRoll = actionPoints < ActionPointCost || !artifact.HasState("activated")
                ? -1
                : 1;

            return new TurnAction
            {
                ActionPointCost = ActionPointCost,
                UsingArtifact = artifact,
                SuccessRoll = successRoll,
                SuccessConsequences = new[]
                {
                    new Consequence
                    {
                        Target = artifact,
                        BindsTo = "DeactivateState()",
                        Value = "activate"
                    }
                }
            };
        }
    }
}
