using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Attributes;

namespace Rpg.Sys.Components
{
    public class HealthTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [MinZeroUI(Group = "Health")]
        public int Physical { get; set; }

        [MinZeroUI(Group = "Health")]
        public int Mental { get; set; }

        [MinZeroUI(Group = "Health")]
        public int Cyber { get; set; }
    }
}
