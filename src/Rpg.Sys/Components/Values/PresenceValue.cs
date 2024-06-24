using Rpg.ModObjects.Meta.Attributes;

namespace Rpg.Sys.Components.Values
{
    public class PresenceValue : MinMaxValue
    {
        [MetersUI]
        public int Radius { get; protected set; }

        public PresenceValue(string entityId, string name, int max, int current, int radius)
            : base(entityId, name, max, current)
                => Radius = radius;
    }
}
