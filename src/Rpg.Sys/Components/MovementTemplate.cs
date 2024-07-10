using Rpg.ModObjects;

namespace Rpg.Sys.Components
{
    public class MovementTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int MaxSpeed { get; set; }
        public int Acceleration { get; set; }
        public int Deceleration { get; set; }
    }
}
