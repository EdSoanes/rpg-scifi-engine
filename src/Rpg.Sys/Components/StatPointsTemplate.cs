using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
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

        [ScoreUI]
        public int Strength { get; set; }

        [ScoreUI]
        public int Intelligence { get; set; }

        [ScoreUI]
        public int Wisdom { get; set; }

        [ScoreUI]
        public int Dexterity { get; set; }

        [ScoreUI]
        public int Constitution { get; set; }

        [ScoreUI]
        public int Charisma { get; set; }
    }
}
