using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class DefensesTemplate
    {
        public int Kinetic {  get; set; }
        public int KineticShielding { get; set; }
        public int Heat { get; set; }
        public int HeatShielding { get; set; }
        public int Energy { get; set; }
        public int EnergyShielding { get; set; }
        public int Chemical { get; set; }
        public int ChemicalShielding { get; set; }
        public int Radiation { get; set; }
        public int RadiationShielding { get; set; }
        public int Cyber { get; set; } = int.MaxValue;
        public int CyberShielding { get; set; } = int.MaxValue;
        public int Mental { get; set; } = int.MaxValue;
        public int MentalShielding { get; set; } = int.MaxValue;
    }
}
