using Rpg.ModObjects;

namespace Rpg.Sys.Components
{
    public class DefensesTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
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
        public int Cyber { get; set; }
        public int CyberShielding { get; set; }
        public int Mental { get; set; }
        public int MentalShielding { get; set; }
    }
}
