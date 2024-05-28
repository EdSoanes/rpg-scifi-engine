using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Cmds
{
    public class ModCmd : ITemporal
    {
        protected ModGraph? Graph { get; set; }
        protected ModObject? Entity { get; set; }

        [JsonProperty] public Guid EntityId { get; private set; }
        [JsonProperty] public string CommandName { get; private set; }
        [JsonProperty] public string InstanceName { get; private set; }
        [JsonProperty] public string? OutcomeCommandName { get; private set; }
        [JsonProperty] public string[] EnabledOnStates { get; private set; }
        [JsonProperty] public string[] DisabledOnStates { get; private set; }
        [JsonProperty] public ModCmdArg[] Args { get; private set; } = new ModCmdArg[0];

        public ModState State { get => new ModState(EntityId, CommandName); }

        public int? GetTarget(ModObject initiator, ModSet? modSet)
            => Graph!.GetPropValue(initiator, modSet?.TargetProp).Roll();

        public Dice? GetDiceRoll(ModObject initiator, ModSet? modSet)
            => Graph!.GetPropValue(initiator, modSet?.DiceRollProp);

        public int? GetOutcome(ModObject initiator, ModSet? modSet)
            => Graph!.GetPropValue(initiator, modSet?.OutcomeProp).Roll();

        public static ModCmd Create(Guid entityId, string commandName, ModCmdAttribute cmdAttr, ModCmdArg[]? cmdArgs)
        {
            var cmd = new ModCmd
            {
                EntityId = entityId,
                CommandName = commandName,
                OutcomeCommandName = cmdAttr.OutcomeMethod,
                InstanceName = $"{entityId}.{commandName}",
                Args = cmdArgs ?? new ModCmdArg[0],
                EnabledOnStates = CreateStateList(cmdAttr.EnabledOnState, cmdAttr.EnabledOnStates),
                DisabledOnStates = CreateStateList(cmdAttr.DisabledOnState, cmdAttr.DisabledOnStates)
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

        public ModCmdArgSet ArgSet()
            => new ModCmdArgSet(Args);

        public ModCmdArgSet ArgSet<T>(T initiator)
            where T : ModObject
                => new ModCmdArgSet(Args).SetInitiator(initiator);

        public ModCmdArgSet ArgSet<TI, TR>(TI initiator, TR recipient)
            where TI : ModObject
            where TR : ModObject
                => new ModCmdArgSet(Args)
                    .SetInitiator(initiator)
                    .SetRecipient(recipient);

        public ModSet? Create(ModCmdArgSet argSet)
        {
            var entity = Graph!.GetEntity(EntityId);
            if (entity == null)
                return null;

            var modSet = CreateModSet()
                .AddState(entity);

            var args = argSet
                .SetModSet(modSet)
                .ToArgs();

            modSet = entity.ExecuteFunction<ModSet>(CommandName, args);
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
                    Graph!.TriggerUpdate();
                }
            }
        }

        private ModSet CreateModSet()
            => new ModSet(EntityId, CommandName, new Turn());

        public void OnGraphCreating(ModGraph graph, ModObject entity)
        {
            Graph = graph;
            Entity = entity;
        }

        public void OnTurnChanged(int turn)
        {
            throw new NotImplementedException();
        }

        public void OnBeginEncounter()
        {
            throw new NotImplementedException();
        }

        public void OnEndEncounter()
        {
            throw new NotImplementedException();
        }
    }
}
