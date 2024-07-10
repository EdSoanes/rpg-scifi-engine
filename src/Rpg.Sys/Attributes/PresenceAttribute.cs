using Rpg.ModObjects.Meta.Props;

namespace Rpg.Sys.Attributes
{
    public class PresenceAttribute : IntegerAttribute
    {
        public PresenceAttribute()
            : base()
        {
            Min = 0;
            Max = 10;
        }
    }
}
