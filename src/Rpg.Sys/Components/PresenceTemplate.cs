using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class PresenceTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public int Size { get; set; }
        public int Weight { get; set; }

        [PresenceUI(Group = "Sound")]
        public int SoundMax { get; set; }

        [PresenceUI(Group = "Sound")]
        public int SoundCurrent { get; set; }

        [MetersUI(Group = "Sound")]
        public int SoundRadius { get; set; }

        [PresenceUI(Group = "Light")]
        public int LightMax { get; set; }

        [PresenceUI(Group = "Light")]
        public int LightCurrent { get; set; }

        [MetersUI(Group = "Light")]
        public int LightRadius { get; set; }

        [PresenceUI(Group = "Heat")]
        public int HeatMax { get; set; }

        [PresenceUI(Group = "Heat")]
        public int HeatCurrent { get; set; }

        [MetersUI(Group = "Heat")]
        public int HeatRadius { get; set; }

        [PresenceUI(Group = "Chemical")]
        public int ChemicalMax { get; set; }

        [PresenceUI(Group = "Chemical")]
        public int ChemicalCurrent { get; set; }

        [MetersUI(Group = "Chemical")]
        public int ChemicalRadius { get; set; }

        [PresenceUI(Group = "Radiation")]
        public int RadiationMax { get; set; }

        [PresenceUI(Group = "Radiation")]
        public int RadiationCurrent { get; set; }

        [MetersUI(Group = "Radiation")]
        public int RadiationRadius { get; set; }

        [PresenceUI(Group = "Electromagnetic")]
        public int ElectromagneticMax { get; set; }

        [PresenceUI(Group = "Electromagnetic")]
        public int ElectromagneticCurrent { get; set; }

        [MetersUI(Group = "Electromagnetic")]
        public int ElectromagneticRadius { get; set; }
    }
}
