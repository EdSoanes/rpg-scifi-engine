using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Characters
{
    internal class CharResistanceBlock : DamageSignature
    {
        public CharResistanceBlock()
        {
            Impact.Value = "3";
            Pierce.Value = "3";
            Blast.Value = "3";
            Stun.Value = "3";
            Chemical.Value = "3";
            Biological.Value = "3";
            Radiation.Value = "3";
            Electromagnetic.Value = "3";
            Heat.Value = "3";
        }
    }
}
