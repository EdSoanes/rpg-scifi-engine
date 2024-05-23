using Newtonsoft.Json;

namespace Rpg.ModObjects.Actions
{
    public class ModCmd
    {
        [JsonProperty] public Guid EntityId { get; private set; }
        [JsonProperty] public string CommandName { get; private set; }
        [JsonProperty] public string? OutcomeCommandName { get; private set; }

        [JsonProperty] public ModCmdArg[] Args { get; private set; } = new ModCmdArg[0];

        public ModCmd(Guid entityId, string commandName, string? outcomeCommandName, ModCmdArg[] args)
        {
            EntityId = entityId;
            CommandName = commandName;
            OutcomeCommandName = outcomeCommandName;
            Args = args;
        }

        public ModSet? ExecuteCommand(ModGraph graph, Dictionary<string, object?> parms)
        {
            var entity = graph.GetEntity(EntityId);
            var modSet = CreateModSet();

            var args = new List<object?> { modSet };
            var valid = true;
            foreach (var cmdArg in Args)
            {
                if (cmdArg.IsOfType(this))
                    args.Add(this);
                else if (cmdArg.IsOfType(modSet))
                    args.Add(modSet);
                else if (parms.ContainsKey(cmdArg.Name) && cmdArg.IsOfType(parms[cmdArg.Name]))
                    args.Add(parms[cmdArg.Name]);
                else
                {
                    valid = false;
                    break;
                }
            }

            return valid
                ? entity.ExecuteFunction<ModSet>(CommandName, args.ToArray())
                : modSet;
        }

        public ModSet CreateModSet()
            => new ModSet($"{EntityId}.{CommandName}");

        public string Target => $"{EntityId}.{CommandName}.Target";
        public string Roll => $"{EntityId}.{CommandName}.Roll";
    }
}
