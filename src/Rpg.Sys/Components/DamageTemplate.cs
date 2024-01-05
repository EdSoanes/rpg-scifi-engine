using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class DamageTemplate
    {
        public Dice Kinetic { get; set; }
        public int KineticArmorPenetration { get; set; }
        public int KineticRadius { get; set; }

        public Dice Heat { get; set; }
        public int HeatArmorPenetration { get; set; }
        public int HeatRadius { get; set; }

        public Dice Energy { get; set; }
        public int EnergyArmorPenetration { get; set; }
        public int EnergyRadius { get; set; }

        public Dice Chemical { get; set; }
        public int ChemicalArmorPenetration { get; set; }
        public int ChemicalRadius { get; set; }

        public Dice Radiation { get; set; }
        public int RadiationArmorPenetration { get; set; }
        public int RadiationRadius { get; set; }

        public Dice Cyber { get; set; }
        public int CyberArmorPenetration { get; set; }
        public int CyberRadius { get; set; }

        public Dice Mental { get; set; }
        public int MentalArmorPenetration { get; set; }
        public int MentalRadius { get; set; }
    }
}
