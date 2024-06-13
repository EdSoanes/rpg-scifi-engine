using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class DefensesTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [MinZeroUI(Group = "Kinetic", EditorName = "Kinetic Defense")]
        public int Kinetic {  get; set; }

        [PercentUI(Group = "Kinetic", EditorName = "Kinetic Defense Shielding")]
        public int KineticShielding { get; set; }

        [MinZeroUI(Group = "Heat", EditorName = "Heat Defense")]
        public int Heat { get; set; }

        [PercentUI(Group = "Heat", EditorName = "Kinetic Defense Shielding")]
        public int HeatShielding { get; set; }

        [MinZeroUI(Group = "Energy", EditorName = "Energy Defense")]
        public int Energy { get; set; }

        [PercentUI(Group = "Energy", EditorName = "Energy Defense Shielding")]
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
