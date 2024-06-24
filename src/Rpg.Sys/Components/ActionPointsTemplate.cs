using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Attributes;

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
