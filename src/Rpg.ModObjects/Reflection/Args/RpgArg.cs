using Newtonsoft.Json;
using System.Reflection;

namespace Rpg.ModObjects.Reflection.Args
{
    public abstract class RpgArg
    {
        [JsonProperty] public string Name { get; internal set; }
        [JsonProperty] public string Type { get; internal set; }
        [JsonProperty] public bool IsNullable { get; internal set; }
        [JsonProperty] public object? Value { get; protected set; }
        [JsonProperty] public string[] Groups { get; internal set; } = [];

        [JsonConstructor] protected RpgArg() { }

        internal RpgArg(ParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name!;
            IsNullable = Nullable.GetUnderlyingType(parameterInfo.ParameterType) != null;
            Type = !IsNullable
                ? parameterInfo.ParameterType.Name!
                : parameterInfo.ParameterType.GetGenericArguments().First().Name;
        }

        public abstract RpgArg Clone();
        public abstract void SetValue(object? value, RpgGraph? graph = null);
        public abstract void FillValue(object? value, RpgGraph? graph = null);

    }

    public static class RpgArgExtensions
    {
        public static bool IsComplete(this RpgArg[]? rpgArgs)
            => rpgArgs?.All(x => x.IsNullable || x.Value != null) ?? false;

        public static Dictionary<string, object?> ToDictionary(this RpgArg[]? rpgArgs, RpgGraph graph)
        {
            var res = new Dictionary<string, object?>();
            if (rpgArgs != null)
            {
                foreach (var arg in rpgArgs)
                {
                    if (arg is RpgObjectArg)
                    {
                        var rpgObj = graph.GetObject(arg.Value?.ToString());
                        res.Add(arg.Name, rpgObj);
                    }
                    else
                        res.Add(arg.Name, arg.Value);
                }
            }
            return res;
        }

        public static bool Has(this RpgArg[]? rpgArgs, string argName)
            => rpgArgs?.Any(x => x.Name == argName) ?? false;

        public static object? Val(this RpgArg[]? rpgArgs, string argName)
            => rpgArgs.Find(argName)?.Value;

        public static void Set(this RpgArg[]? rpgArgs, string argName, object? value, RpgGraph? graph = null)
            => rpgArgs.Find(argName)?.SetValue(value, graph);

        public static RpgArg[] Fill(this RpgArg[] rpgArgs, RpgArg[]? from, RpgGraph? graph = null)
        {
            if (from != null)
                foreach (var arg in from)
                    rpgArgs.Fill(arg.Name, arg.Value, graph);

            return rpgArgs.ToArray();
        }

        public static void Fill(this RpgArg[]? rpgArgs, string argName, object? value, RpgGraph? graph = null)
            => rpgArgs.Find(argName)?.FillValue(value, graph);

        public static RpgArg? Find(this RpgArg[]? rpgArgs, string argName)
            => rpgArgs?.FirstOrDefault(x => x.Name == argName);

        public static RpgArg[] CloneArgs(this RpgArg[]? rpgArgs)
            => rpgArgs?.Select(x => x.Clone())?.ToArray() ?? [];

        public static RpgArg[] CloneFor(this RpgArg[]? rpgArgs, params RpgArg[]? matching)
        {
            var res = new List<RpgArg>();
            if (matching != null)
            {
                foreach (var match in matching)
                {
                    var newArg = match.Clone();

                    var rpgArg = rpgArgs.Find(match.Name);
                    if (rpgArg != null)
                        newArg.SetValue(rpgArg.Value);

                    res.Add(newArg);
                }
            }

            return res.ToArray();
        }
    }
}
