using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.Sys.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class StatPointsTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [Score]
        public int Strength { get; set; }

        [Score]
        public int Intelligence { get; set; }

        [Score]
        public int Wisdom { get; set; }

        [Score]
        public int Dexterity { get; set; }

        [Score]
        public int Constitution { get; set; }

        [Score]
        public int Charisma { get; set; }
    }
}
