using Rpg.ModObjects;

namespace Rpg.Sys.Components
{
    public class ActionPointsTemplate : IRpgComponentTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Action {  get; set; }
        public int Exertion { get; set; }
        public int Focus { get; set; }
    }
}
