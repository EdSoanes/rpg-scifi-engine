using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class ActionPointsTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [MinZeroUI]
        public int Action {  get; set; }

        [MinZeroUI]
        public int Exertion { get; set; }

        [MinZeroUI]
        public int Focus { get; set; }
    }
}
