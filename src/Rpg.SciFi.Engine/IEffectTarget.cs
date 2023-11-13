using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine
{
    public interface IEffectTarget
    {
        void OnAbilityUsed(string abilityName);
    }
}
