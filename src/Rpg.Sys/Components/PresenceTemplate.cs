using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class PresenceTemplate
    {
        public int Size { get; set; }
        public int Weight { get; set; }

        public int SoundMax { get; set; }
        public int SoundCurrent { get; set; }
        public int SoundRadius { get; set; }

        public int LightMax { get; set; }
        public int LightCurrent { get; set; }
        public int LightRadius { get; set; }

        public int HeatMax { get; set; }
        public int HeatCurrent { get; set; }
        public int HeatRadius { get; set; }

        public int ChemicalMax { get; set; }
        public int ChemicalCurrent { get; set; }
        public int ChemicalRadius { get; set; }

        public int RadiationMax { get; set; }
        public int RadiationCurrent { get; set; }
        public int RadiationRadius { get; set; }

        public int ElectromagneticMax { get; set; }
        public int ElectromagneticCurrent { get; set; }
        public int ElectromagneticRadius { get; set; }
    }
}
