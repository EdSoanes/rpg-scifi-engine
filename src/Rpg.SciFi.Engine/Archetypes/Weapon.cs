using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Archetypes
{
    public class Weapon : Artifact
    {
        public virtual DamageSignature Damage { get; set; } = new DamageSignature();
    }
}
