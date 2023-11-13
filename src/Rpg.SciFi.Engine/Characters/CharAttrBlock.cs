using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Characters
{
    public class CharAttrBlock
    {
        public string Name = "Base";
        public CharAttr Strength { get; set; } = new CharAttr { Name = "Strength", Value = "3" };
        public CharAttr Agility { get; set; } = new CharAttr { Name = "Agility", Value = "3" };
        public CharAttr Intelligence { get; set; } = new CharAttr { Name = "Intelligence", Value = "3" };
        public CharAttr ActionPoints { get; set; } = new CharAttr { Name = "Action Points", Value = "10" };
        public CharAttr Endurance { get; set; } = new CharAttr { Name = "Endurance", Value = "10" };
        public CharAttr EnduranceRecovery { get; set; } = new CharAttr { Name = "Endurance Recovery", Value = "10" };
        public CharAttr HitPoints { get; set; } = new CharAttr { Name = "Hit Points", Value = "20" };
        public CharAttr Speed { get; set; } = new CharAttr { Name = "Speed", Value = "5" };
        public CharAttr Focus { get; set; } = new CharAttr { Name = "Focus", Value = "10" };
    }
}
