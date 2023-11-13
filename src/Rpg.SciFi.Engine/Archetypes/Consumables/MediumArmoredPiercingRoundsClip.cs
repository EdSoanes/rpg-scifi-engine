using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Archetypes.Consumables
{
    public class MediumArmoredPiercingRoundsClip : ConsumableArtifact, IAmmunitionConsumable
    {
        public DamageSignature Damage => new DamageSignature();

        public MediumArmoredPiercingRoundsClip()
        {
            Name = "Medium AP Rounds";
            Description = "Standard Medium .303mm Armored Piercing Rounds";
            ContentType = "medium-ap-bullet";
            Max = 100;
            Weight = 1.0;

            Damage.Pierce.Value = "30";
            Damage.Impact.Value = "10";
        }
    }
}
