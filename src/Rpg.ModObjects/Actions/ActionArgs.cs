using Rpg.ModObjects.Reflection.Args;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Actions
{
    public sealed class ActionArgs
    {
        [JsonProperty] public RpgArg[] Args { get; private init; }
        [JsonProperty] public Dictionary<string, object?> Values { get; private init; } = new();

        [JsonConstructor] private ActionArgs() { }

        internal ActionArgs(RpgArg[] args, Dictionary<string, object?> values)
        {
            Args = args;
            foreach (var arg in args)
            {
                if (values.ContainsKey(arg.Name))
                    Values.Add(arg.Name, values[arg.Name]);
            }
        }
    }
}
