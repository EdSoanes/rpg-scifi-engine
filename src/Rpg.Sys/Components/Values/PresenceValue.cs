using Rpg.ModObjects.Meta.Props;

namespace Rpg.Sys.Components.Values
{
    public class PresenceValue : MinMaxValue
    {
        [Meters]
        public int Radius { get; protected set; }

        public PresenceValue(string name, int max, int current, int radius)
            : base(name, max, current)
                => Radius = radius;
    }
}
