using Rpg.ModObjects;
using Rpg.ModObjects.Meta;

namespace Rpg.Sys.Components
{
    public class MovementTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [MetersUI(Group = "Movement")]
        public int MaxSpeed { get; set; }

        [AccelerationUI(Group = "Movement")]
        public int Acceleration { get; set; }

        [AccelerationUI(Group = "Movement")]
        public int Deceleration { get; set; }
    }
}
