using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Archetypes
{
    public interface IAmmunitionConsumable
    {
        DamageSignature Damage { get; }
    }
}
