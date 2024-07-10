using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.Sys.Attributes;

namespace Rpg.Sys.Components
{
    public class HealthTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [Health(Group = "Health")]
        public int Physical { get; set; }

        [Health(Group = "Health")]
        public int Mental { get; set; }

        [Health(Group = "Health")]
        public int Cyber { get; set; }
    }
}
