using Newtonsoft.Json;
using Rpg.Cyborgs.Attributes;
using Rpg.ModObjects;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs
{
    public class BodyPart : RpgComponent
    {
        [Injury]
        [JsonIgnore] public int InjurySeverity { get; protected set; }
        [JsonProperty] public int[] Injuries { get; private set; } = Array.Empty<int>();

        [JsonProperty] public BodyPartType BodyPartType { get; protected set; }

        [JsonConstructor] public BodyPart()
            : base() { }

        public BodyPart(string name, BodyPartType bodyPartType)
            : base(name) 
        {
            BodyPartType = bodyPartType;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            var expiry = base.OnUpdateLifecycle(graph, currentTime);
            Injuries = GetActiveMods(nameof(InjurySeverity)).Select(x => Graph!.CalculateModValue(x)?.Roll() ?? 0).ToArray();

            return expiry;
        }
    }
}
