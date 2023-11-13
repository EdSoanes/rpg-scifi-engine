using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.SciFi.Engine.Characters;

namespace Rpg.SciFi.Engine
{
    public abstract class Signature
    {
        public string Name = "Base";

        public CharAttr Impact { get; set; } = new CharAttr { Name = "Impact", Value = "0" };
        public CharAttr Pierce { get; set; } = new CharAttr { Name = "Pierce", Value = "0" };
        public CharAttr Blast { get; set; } = new CharAttr { Name = "Blast", Value = "0" };
        public CharAttr Stun { get; set; } = new CharAttr { Name = "Stun", Value = "0" };
        public CharAttr Heat { get; set; } = new CharAttr { Name = "Heat", Value = "0" };
        public CharAttr Chemical { get; set; } = new CharAttr { Name = "Chemical", Value = "0" };
        public CharAttr Biological { get; set; } = new CharAttr { Name = "Biological", Value = "0" };
        public CharAttr Radiation { get; set; } = new CharAttr { Name = "Radiation", Value = "0" };
        public CharAttr Electromagnetic { get; set; } = new CharAttr { Name = "Electromagnetic", Value = "0" };
    }
}
