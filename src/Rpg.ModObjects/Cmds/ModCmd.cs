using Newtonsoft.Json;
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

        public int? GetTargetRoll(ModObject initiator, ModSet? modSet)
            => initiator.GetPropValue(modSet?.TargetProp)?.Roll();

        public Dice? GetDiceRoll(ModObject initiator, ModSet? modSet)
            => initiator.GetPropValue(modSet?.DiceRollProp);

        public int? GetOutcome(ModObject initiator, ModSet? modSet)
            => initiator.GetPropValue(modSet?.OutcomeProp)?.Roll();

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

        public Dictionary<string, object?> ExecutionArgSet()
        {
            var parms = new Dictionary<string, object?>();
            foreach (var arg in Args.Where(x => !ModCmdArg.ReservedArgs.Contains(x.Name)))
                parms.Add(arg.Name, null);

            return parms;
        }

        public ModSet? Execute(ModObject? initiator, Dictionary<string, object?> parms)
            => Execute(initiator, null, null, null, parms);

        public ModSet? Execute(ModObject? initiator, int? targetRoll, Dice? diceRoll, int? outcome, Dictionary<string, object?> parms)
        {
            var entity = Graph!.GetEntity(EntityId);
            if (entity == null)
                return null;

            var args = ToArgs(initiator, targetRoll, diceRoll, outcome, parms);
            var modSet = entity.ExecuteFunction<ModSet>(CommandName, args);

            entity.ManuallyActivateState(CommandName);
            if (modSet != null)
            {
                entity.AddModSet(modSet);
                Graph!.TriggerUpdate();
            }

            return modSet;
        }

        private object?[] ToArgs(ModObject? initiator, int? targetRoll, Dice? diceRoll, int? outcome, Dictionary<string, object?> parms)
        {
            var res = new List<object?>();
            foreach (var cmdArg in Args)
            {
                object? obj = null;
                if (cmdArg.IsReserved)
                {
                    obj = cmdArg.Name switch
                    {
                        ModCmdArg.InitiatorArg => initiator,
                        ModCmdArg.ModSetArg => CreateModSet(),
                        ModCmdArg.TargetArg => targetRoll,
                        ModCmdArg.DiceRollArg => diceRoll,
                        ModCmdArg.OutcomeArg => outcome,
                        _ => null
                    };
                }
                else if (parms.ContainsKey(cmdArg.Name) && cmdArg.IsOfType(parms[cmdArg.Name]))
                    obj = parms[cmdArg.Name];

                res.Add(obj);
            }

            return res.ToArray();
        }

        private ModSet CreateModSet()
            => new ModSet(EntityId, CommandName, ModDuration.OnNewTurn(Graph!.Turn));

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
