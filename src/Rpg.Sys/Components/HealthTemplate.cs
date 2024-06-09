using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.Sys.Components.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
