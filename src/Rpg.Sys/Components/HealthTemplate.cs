using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.Sys.Attributes;

namespace Rpg.Sys.Components
{
    public class HealthTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [HealthUI(Group = "Health")]
        public int Physical { get; set; }

        [HealthUI(Group = "Health")]
        public int Mental { get; set; }

        [HealthUI(Group = "Health")]
        public int Cyber { get; set; }
    }
}
