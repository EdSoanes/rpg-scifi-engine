using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Values;

namespace Rpg.Sys.Components
{
    public class DamageTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [MinZeroUI(Group = "Kinetic")]
        public Dice Kinetic { get; set; }

        [PercentUI(Group = "Kinetic")]
        public int KineticArmorPenetration { get; set; }

        [MetersUI(Group = "Kinetic")]
        public int KineticRadius { get; set; }

        [MinZeroUI(Group = "Heat")]
        public Dice Heat { get; set; }

        [PercentUI(Group = "Heat")]
        public int HeatArmorPenetration { get; set; }

        [MetersUI(Group = "Heat")]
        public int HeatRadius { get; set; }

        [MinZeroUI(Group = "Energy")]
        public Dice Energy { get; set; }

        [PercentUI(Group = "Energy")]
        public int EnergyArmorPenetration { get; set; }

        [MetersUI(Group = "Energy")]
        public int EnergyRadius { get; set; }

        [MinZeroUI(Group = "Chemical")]
        public Dice Chemical { get; set; }

        [PercentUI(Group = "Chemical")]
        public int ChemicalArmorPenetration { get; set; }

        [MetersUI(Group = "Chemical")]
        public int ChemicalRadius { get; set; }

        [MinZeroUI(Group = "Radiation")]
        public Dice Radiation { get; set; }

        [PercentUI(Group = "Radiation")]
        public int RadiationArmorPenetration { get; set; }

        [MetersUI(Group = "Radiation")]
        public int RadiationRadius { get; set; }

        [MinZeroUI(Group = "Cyber")]
        public Dice Cyber { get; set; }

        [PercentUI(Group = "Cyber")]
        public int CyberArmorPenetration { get; set; }

        [MetersUI(Group = "Cyber")]
        public int CyberRadius { get; set; }

        [MinZeroUI(Group = "Mental")]
        public Dice Mental { get; set; }

        [PercentUI(Group = "Mental")]
        public int MentalArmorPenetration { get; set; }

        [MetersUI(Group = "Mental")]
        public int MentalRadius { get; set; }
    }
}
