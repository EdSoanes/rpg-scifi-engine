using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Meta.Props;
using Rpg.Sys.Attributes;
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

        [Presence(Group = "Sound")]
        public int SoundMax { get; set; }

        [Presence(Group = "Sound")]
        public int SoundCurrent { get; set; }

        [Meters(Group = "Sound")]
        public int SoundRadius { get; set; }

        [Presence(Group = "Light")]
        public int LightMax { get; set; }

        [Presence(Group = "Light")]
        public int LightCurrent { get; set; }

        [Meters(Group = "Light")]
        public int LightRadius { get; set; }

        [Presence(Group = "Heat")]
        public int HeatMax { get; set; }

        [Presence(Group = "Heat")]
        public int HeatCurrent { get; set; }

        [Meters(Group = "Heat")]
        public int HeatRadius { get; set; }

        [Presence(Group = "Chemical")]
        public int ChemicalMax { get; set; }

        [Presence(Group = "Chemical")]
        public int ChemicalCurrent { get; set; }

        [Meters(Group = "Chemical")]
        public int ChemicalRadius { get; set; }

        [Presence(Group = "Radiation")]
        public int RadiationMax { get; set; }

        [Presence(Group = "Radiation")]
        public int RadiationCurrent { get; set; }

        [Meters(Group = "Radiation")]
        public int RadiationRadius { get; set; }

        [Presence(Group = "Electromagnetic")]
        public int ElectromagneticMax { get; set; }

        [Presence(Group = "Electromagnetic")]
        public int ElectromagneticCurrent { get; set; }

        [Meters(Group = "Electromagnetic")]
        public int ElectromagneticRadius { get; set; }
    }
}
