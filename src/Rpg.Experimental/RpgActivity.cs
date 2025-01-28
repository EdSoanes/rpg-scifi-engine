using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection.Args;
using Rpg.Experimental.Time;

namespace Rpg.Experimental
{
    public class RpgActivity : RpgObject
    {
        [JsonProperty] public List<RpgObject> ActivityActions { get; protected set; } = new();
        [JsonIgnore] public RpgActivityAction? CurrentActivityAction { get => ActivityActions.FirstOrDefault() as RpgActivityAction; }
        public RpgArg[] Args { get; protected set; } = [];

        public RpgActivity(RpgObject owner, TimePoint start, TimePoint end)
            : base($"{owner.Id}_Activity", owner.Id, start, end)
        {
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            base.OnTimeEvent(graph);
            Args = RpgArg.CreateArgs(graph, Args, [.. ActivityActions.Cast<RpgActivityAction>().Select(x => x.Args)]);
        }

        public override void OnSyncProperty(RpgGraph graph, string prop)
        {
            if (prop == nameof(ActivityActions))
                Args = RpgArg.CreateArgs(graph, Args, [.. ActivityActions.Cast<RpgActivityAction>().Select(x => x.Args)]);
        }
    }
}
