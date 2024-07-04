using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.Tests.Models
{
    internal static class ActorFactory
    {
        public static PlayerCharacterTemplate BennyTemplate 
        { 
            get => new PlayerCharacterTemplate
            {
                Name = "Benny",
                Strength = -1,
                Agility = 0,
                Health = 1,
                Brains = 1,
                Insight = 0,
                Charisma = 1
            };
        }
    }
}
