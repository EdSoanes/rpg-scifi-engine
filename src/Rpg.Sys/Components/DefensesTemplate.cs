using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Attributes;

namespace Rpg.Sys.Components
{
    public class DefensesTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [MinZeroUI(Group = "Kinetic", DisplayName = "Kinetic Defense")]
        public int Kinetic {  get; set; }

        [PercentUI(Group = "Kinetic", DisplayName = "Kinetic Defense Shielding")]
        public int KineticShielding { get; set; }

        [MinZeroUI(Group = "Heat", DisplayName = "Heat Defense")]
        public int Heat { get; set; }

        [PercentUI(Group = "Heat", DisplayName = "Kinetic Defense Shielding")]
        public int HeatShielding { get; set; }

        [MinZeroUI(Group = "Energy", DisplayName = "Energy Defense")]
        public int Energy { get; set; }

        [PercentUI(Group = "Energy", DisplayName = "Energy Defense Shielding")]
        public int EnergyShielding { get; set; }

        [MinZeroUI(Group = "Chemical")]
        public int Chemical { get; set; }

        [PercentUI(Group = "Chemical")]
        public int ChemicalShielding { get; set; }

        [MinZeroUI(Group = "Radiation")]
        public int Radiation { get; set; }

        [PercentUI(Group = "Radiation")]
        public int RadiationShielding { get; set; }

        [MinZeroUI(Group = "Cyber")]
        public int Cyber { get; set; }

        [PercentUI(Group = "Cyber")]
        public int CyberShielding { get; set; }

        [MinZeroUI(Group = "Mental")]
        public int Mental { get; set; }

        [PercentUI(Group = "Mental")]
        public int MentalShielding { get; set; }
    }
}
