using Newtonsoft.Json;
using Rpg.Experimental;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;
using Rpg.Experimental.System.Props;

namespace Rpg.Cyborgs
{
    public class BodyPart : RpgObject
    {
        [Select()]
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

        public override void OnRestoring(RpgGraph graph)
        {
            base.OnRestoring(graph);
            foreach (var injury in Injuries)
                injury.OnRestoring(graph);
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            base.OnTimeEvent(graph);
            Injuries = CalculateInjuries(graph);
        }

        private Injury[] CalculateInjuries(RpgGraph graph)
        {
            var injuryMods = ModFilters.Active(graph.GetPropertyData<RpgPropertyDataModdable>(Id, nameof(InjurySeverity))?.Mods);
            var injuries = injuryMods.Select(x =>
            {
                var injury = new Injury
                {
                    Id = x.Id,
                    Severity = x.Source?.Value?.Roll() ?? 0,
                    BodyPartType = this.BodyPartType
                };

                injury.SetLifespan(x.Start, x.End);
                injury.OnCreating(graph, this);

                return injury;
            })
            .ToArray();

            return injuries;
        }
    }
}
