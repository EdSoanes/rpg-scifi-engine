using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rpg.Experimental.Graph;
using System.Reflection;

namespace Rpg.Experimental.Reflection.Args
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

        public static RpgArg[] CreateArgs(RpgGraph graph, RpgArg[] existing, params RpgArg[]?[] argsList)
        {
            var additions = new List<RpgArg>();
            foreach (var args in argsList.Where(x => x != null))
            {
                var cloned = args.CloneArgs();
                foreach (var arg in cloned)
                    if (!existing.Any(x => x.Name == arg.Name) && !additions.Any(x => x.Name == arg.Name))
                        additions.Add(arg);
            }

            var res = existing.ToList();
            res.AddRange(additions);


            var allArgs = res.ToArray();
            foreach (var args in argsList.Where(x => x != null))
                SetValues(graph, allArgs, args!);

            return allArgs;
        }

        public static Dictionary<string, object?> CreateDictionary(RpgGraph graph, RpgArg[]? rpgArgs)
        {
            var res = new Dictionary<string, object?>();
            if (rpgArgs != null)
            {
                foreach (var arg in rpgArgs)
                {
                    if (arg is RpgObjectArg)
                    {
                        var rpgObj = graph.GetObject(arg.Value?.ToString());
                        if (rpgObj != null)
                            graph.OnSyncProperties(rpgObj.Id);

                        res.Add(arg.Name, rpgObj);
                    }
                    else
                        res.Add(arg.Name, arg.Value);
                }
            }
            return res;
        }

        public static void SetValues(RpgGraph graph, RpgArg[]? rpgArgs, RpgObject obj)
        {
            if (rpgArgs != null)
            {
                foreach (var arg in rpgArgs)
                {
                    var argObj = obj.ResolvePropertyNameToObject(graph, arg.Name);
                    if (argObj != null)
                        arg.SetValue(argObj);
                    else
                    {
                        var val = graph.GetPropertyData(obj.Id, arg.Name)?.GetValue<object?>(graph);
                        if (val != null)
                            arg.SetValue(val);
                    }
                }
            }
        }

        public static void SetValues(RpgGraph graph, RpgArg[] rpgArgs, RpgArg[]? from)
        {
            if (from != null)
                foreach (var arg in from)
                    SetValue(graph, rpgArgs, arg.Name, arg.Value);
        }

        public static void SetValue(RpgGraph graph, RpgArg[] rpgArgs, string argName, object? value)
            => rpgArgs.Find(argName)?.SetValue(value, graph);

        public static void SetValue(RpgGraph graph, RpgArg[] rpgArgs, (string, object?)[]? from)
        {
            if (from != null)
                foreach (var arg in from)
                    SetValue(graph, rpgArgs, arg.Item1, arg.Item2);
        }
    }

    public static class RpgArgExtensions
    {
        public static bool IsComplete(this RpgArg[]? rpgArgs, string? group = null)
        {
            if (rpgArgs == null) return false;
            
            foreach (var arg in rpgArgs.Where(x => group == null || x.Groups.Contains(group)))
            {
                if (!arg.IsNullable && arg.Value == null)
                    return false;
            }

            return true;
        }





        public static RpgArg[] Fill(this RpgArg[] rpgArgs, RpgArg[]? from, RpgGraph? graph = null)
        {
            if (from != null)
                foreach (var arg in from)
                    rpgArgs.Find(arg.Name)?.FillValue(arg.Value, graph);

            return rpgArgs.ToArray();
        }







        public static RpgArg? Find(this RpgArg[]? rpgArgs, string argName)
            => rpgArgs?.FirstOrDefault(x => x.Name == argName);

        public static RpgArg[] CloneArgs(this RpgArg[]? rpgArgs, string? group = null)
            => rpgArgs
                ?.Where(x => group == null || x.Groups.Contains(group))
                .Select(x => x.Clone())
                .ToArray() ?? [];
    }
}
