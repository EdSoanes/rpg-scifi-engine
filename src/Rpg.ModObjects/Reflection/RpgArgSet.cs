using Newtonsoft.Json;
using Rpg.ModObjects.Actions;

namespace Rpg.ModObjects.Reflection
{
    public class RpgArgSet
    {
        [JsonProperty] public Dictionary<string, RpgArg> Args { get; private set; } = new();
        [JsonProperty] public Dictionary<string, object?> ArgValues { get; private set; } = new();

        public RpgArgSet() { }

        private void ValidateArgValue(string arg, object? value)
        {
            if (!Args.ContainsKey(arg))
                throw new ArgumentException($"'{arg}' param does not exist");

            var modArg = Args[arg];
            if (!modArg.IsNullable && value == null)
                throw new ArgumentException($"'{arg}' param value is null");

            if (string.IsNullOrEmpty(modArg.TypeName))
                throw new ArgumentException($"Type name for param '{arg}' not set");

            if (value != null)
            {
                var type = RpgReflection.ScanForType(modArg.QualifiedTypeName);
                if (type == null)
                    throw new ArgumentException($"Could not find param type for param '{arg}'");

                if (value != null && !value.GetType().IsAssignableTo(type))
                    throw new ArgumentException($"'{arg}' value with type {value.GetType().Name} not assignable to method param with with type {type.Name}");
            }
        }

        public RpgArgSet Merge(RpgArgSet other)
        {
            var argSet = new RpgArgSet();

            foreach (var key in ArgValues.Keys)
            {
                var val = ArgValues[key];
                argSet.ArgValues.Add(key, val);
            }

            argSet.FillFrom(other);
            return argSet;
        }

        public void FillFrom(RpgArgSet other)
        {
            foreach (var key in other.ArgValues.Keys)
            {
                var val = other.ArgValues[key];
                if (!ArgValues.ContainsKey(key) || ArgValues[key] == null)
                    ArgValues.Add(key, val);
            }
        }

        public void Set(ActionInstance? actionInstance, RpgEntity? owner, RpgEntity? initiator, int? actionNo)
        {
            Set("actionInstance", actionInstance);
            Set("actionNo", actionNo!.Value);
            Set("initiator", initiator);
            Set("owner", owner);
        }

        public RpgArgSet Set(int idx, object? value)
        {
            if (Args.Count() < idx && idx >= 0)
            {
                var argName = Args.Keys.ToArray()[idx];
                ValidateArgValue(argName, value);
                ArgValues[argName] = value;
            }

            return this;
        }

        public RpgArgSet Set(string argName, object? value)
        {
            if (Args.ContainsKey(argName))
            {
                ValidateArgValue(argName, value);
                ArgValues[argName] = value;
            }

            return this;
        }

        public RpgArgSet SetStr(string argName, string? value)
        {
            if (Args.ContainsKey(argName))
            {
                var val = Args[argName].ToArgObject(value);
                ValidateArgValue(argName, val);
                ArgValues[argName] = value;
            }

            return this;
        }

        public void Set(Dictionary<string, string?> args)
        {
            foreach (var key in args.Keys)
                SetStr(key, args[key]);
        }

        public object?[] ToArgs()
        {
            var res = new List<object?>();
            foreach (var arg in Args.Where(x => ArgValues.ContainsKey(x.Key)))
            {
                var obj = ArgValues[arg.Key];
                if (obj != null || arg.Value.IsNullable)
                    res.Add(obj);
            }

            return res.ToArray();
        }
    }
}
