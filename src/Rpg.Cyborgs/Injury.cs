using Rpg.Experimental;

namespace Rpg.Cyborgs
{
    public class Injury : RpgLifecycleObject
    {
        public new string Id { get; set; }
        public BodyPartType BodyPartType { get; set; }
        public int Severity { get; set; }
    }
}
