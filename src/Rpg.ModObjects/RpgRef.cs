using Newtonsoft.Json;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public sealed class RpgObjRef
    {
        [JsonProperty] public SpanOfTime Lifespan { get; set; }
        [JsonProperty] public string EntityId { get; set; }

        [JsonConstructor] RpgObjRef() { }

        public RpgObjRef(string entityId, PointInTime start, PointInTime end)
        {
            EntityId = entityId;
            Lifespan = new SpanOfTime(start, end);
        }
    }

    public sealed class RpgRef : RpgLifecycleObject
    {
        [JsonProperty] private List<RpgObjRef> Refs { get; set; } = new();

        public string? EntityId
        {
            get
            {
                var entityRef = Refs.FirstOrDefault(x => x.Lifespan.GetExpiry(Graph.Time.Current) == LifecycleExpiry.Active);
                return entityRef?.EntityId;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && EntityId != value)
                {
                    var last = Refs.LastOrDefault();
                    if (last != null)
                        last.Lifespan = new SpanOfTime(last.Lifespan.Start, Graph.Time.Current);

                    Refs.Add(new RpgObjRef(value, Graph.Time.Current, new PointInTime(PointInTimeType.TimeEnds)));
                }
            }
        }

        [JsonConstructor] private RpgRef() { }

        public RpgRef(RpgGraph graph, string entityId)
        {
            OnCreating(graph);
            OnTimeBegins();
            EntityId = entityId;
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            base.OnUpdateLifecycle();
            if (!Graph.Time.Current.IsEncounterTime)
            {
                Refs = Refs.Where(x =>
                {
                    var expiry = x.Lifespan.GetExpiry(Graph.Time.Current);
                    return expiry != LifecycleExpiry.Expired && expiry != LifecycleExpiry.Destroyed;
                })
                .ToList();
            }

            return Expiry;
        }
    }
}
