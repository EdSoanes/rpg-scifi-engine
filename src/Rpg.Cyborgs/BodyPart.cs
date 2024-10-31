using Rpg.Cyborgs.Attributes;
using Rpg.ModObjects;
using Rpg.ModObjects.Time;
using Newtonsoft.Json;

namespace Rpg.Cyborgs
{
    public class BodyPart : RpgComponent
    {
        [Injury]
        [JsonIgnore] public int InjurySeverity { get; protected set; }
        [JsonProperty] public Injury[] Injuries { get; protected set; } = Array.Empty<Injury>();

        [JsonProperty] public BodyPartType BodyPartType { get; protected set; }

        [JsonConstructor] public BodyPart()
            : base() { }

        public BodyPart(string name, BodyPartType bodyPartType)
            : base(name) 
        {
            BodyPartType = bodyPartType;
        }

        public override void OnRestoring(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnRestoring(graph, entity);
            foreach (var injury in Injuries)
                injury.OnRestoring(Graph, this);
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            var expiry = base.OnUpdateLifecycle();
            Injuries = CalculateInjuries();

            return expiry;
        }

        private Injury[] CalculateInjuries()
        {
            var injuryMods = GetActiveMods(nameof(InjurySeverity));
            var injuries = injuryMods.Select(x =>
            {
                var injury = new Injury
                {
                    Id = x.Id,
                    Severity = Graph!.CalculateModValue(x)?.Roll() ?? 0,
                    BodyPartType = this.BodyPartType
                };

                injury.SetLifespan(x);
                if (Graph != null)
                {
                    injury.OnCreating(Graph, this);
                    injury.OnTimeBegins();
                }

                return injury;
            })
            .ToArray();

            return injuries;
        }
    }
}
