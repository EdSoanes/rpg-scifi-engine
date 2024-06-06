using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Actions
{
    public class RpgAction : IGraphEvents, ITimeEvent
    {
        protected RpgGraph? Graph { get; set; }
        protected RpgObject? Entity { get; set; }

        [JsonProperty] public string EntityId { get; private set; }
        [JsonProperty] public string ActionName { get; private set; }
        [JsonProperty] public string InstanceName { get; private set; }
        [JsonProperty] public string? OutcomeAction { get; private set; }
        [JsonProperty] public string[] EnabledWhen { get; private set; }
        [JsonProperty] public string[] DisabledWhen { get; private set; }
        [JsonProperty] public RpgActionArg[] Args { get; private set; } = new RpgActionArg[0];

        public ModState State { get => new ModState(EntityId, ActionName); }

        public int? GetTarget(RpgObject initiator, ModSet? modSet)
            => Graph!.GetPropValue(initiator, modSet?.TargetPropName).Roll();

        public Dice? GetDiceRoll(RpgObject initiator, ModSet? modSet)
            => Graph!.GetPropValue(initiator, modSet?.DiceRollPropName);

        public int? GetOutcome(RpgObject initiator, ModSet? modSet)
            => Graph!.GetPropValue(initiator, modSet?.OutcomePropName).Roll();

        public static RpgAction Create(string entityId, string actionName, RpgActionAttribute cmdAttr, RpgActionArg[]? cmdArgs)
        {
            var cmd = new RpgAction
            {
                EntityId = entityId,
                ActionName = actionName,
                OutcomeAction = cmdAttr.OutcomeMethod,
                InstanceName = $"{entityId}.{actionName}",
                Args = cmdArgs ?? new RpgActionArg[0],
                EnabledWhen = CreateStateList(cmdAttr.EnabledWhen, cmdAttr.EnabledWhenAll),
                DisabledWhen = CreateStateList(cmdAttr.DisabledWhen, cmdAttr.DisabledWhenAll)
            };

            return cmd;
        }

        private static string[] CreateStateList(string? state, IEnumerable<string>? states)
        {
            var res = new List<string>();
            if (!string.IsNullOrEmpty(state))
                res.Add(state);

            if (states != null)
                foreach (var item in states.Where(x => !string.IsNullOrEmpty(x)))
                    res.Add(item);

            return res.Distinct().ToArray();
        }

        public RpgActionArgSet ArgSet()
            => new RpgActionArgSet(Args);

        public RpgActionArgSet ArgSet<T>(T initiator)
            where T : RpgObject
                => new RpgActionArgSet(Args).SetInitiator(initiator);

        public RpgActionArgSet ArgSet<TI, TR>(TI initiator, TR recipient)
            where TI : RpgObject
            where TR : RpgObject
                => new RpgActionArgSet(Args)
                    .SetInitiator(initiator)
                    .SetRecipient(recipient);

        public ModSet? Create(RpgActionArgSet argSet)
        {
            var entity = Graph!.GetEntity(EntityId);
            if (entity == null)
                return null;

            var modSet = CreateModSet()
                .AddState(entity);

            var args = argSet
                .SetModSet(modSet)
                .ToArgs();

            modSet = entity.ExecuteFunction<ModSet>(ActionName, args);
            return modSet;
        }

        public void Apply(ModSet? modSet)
        {
            if (modSet != null)
            {
                var entity = Graph!.GetEntity(EntityId);
                if (entity != null)
                {
                    entity.AddModSet(modSet);
                    Graph!.Time.TriggerEvent();
                }
            }
        }

        private ModSet CreateModSet()
            => new ModSet(Graph!.Time.Create().Lifecycle, EntityId, ActionName);

        public void OnGraphCreating(RpgGraph graph, RpgObject entity)
        {
            Graph = graph;
            Entity = entity;
        }

        public void OnObjectsCreating() { }

        public void OnUpdating(RpgGraph graph, TimePoint time) { }
    }
}
