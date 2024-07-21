using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public class RpgArgSet
    {
        private static IRpgArgType[]? _argTypes;
        private static IRpgArgType[] GetArgTypes()
        {
            if (_argTypes == null)
            {
                var types = RpgReflection.ScanForTypes<IRpgArgType>();
                _argTypes = types
                    .Where(x => !x.IsAbstract)
                    .Select(x => Activator.CreateInstance(x) as IRpgArgType)
                    .Where(x => x != null)
                    .Cast<IRpgArgType>()
                    .ToArray();
            }

            return _argTypes;
        }

        private static IRpgArgType? CreateArgType(ParameterInfo parameterInfo)
        {
            var res = GetArgTypes().FirstOrDefault(x => x.IsArgTypeFor(parameterInfo));
            return res?.Clone(parameterInfo.ParameterType);
        }

        //private static IRpgArgType? CreateArgType(Type type)
        //{
        //    var res = GetArgTypes().FirstOrDefault(x => x.GetType(). == type);
        //    return res?.Clone(parameterInfo.ParameterType);
        //}

        [JsonProperty] public Dictionary<string, IRpgArgType> Args { get; private set; } = new();
        [JsonIgnore] public Dictionary<string, object?> ArgValues { get; private set; } = new();

        [JsonConstructor] private RpgArgSet() { }

        public RpgArgSet(ParameterInfo[] parameterInfos)
        {
            foreach (var parameterInfo in parameterInfos)
            {
                var argType = CreateArgType(parameterInfo);
                if (argType != null)
                    Args.Add(parameterInfo.Name!, argType);
            }
        }

        public RpgArgSet Clone()
        {
            var argSet = new RpgArgSet();
            foreach (var arg in Args)
            {
                var argType = arg.Value.Clone();
                argSet.Args.Add(arg.Key, argType);
            }

            return argSet;
        }

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

            foreach (var key in Args.Keys)
            {
                var arg = Args[key].Clone();
                argSet.Args.Add(key, arg);
            }

            foreach (var key in other.Args.Keys)
            {
                if (!argSet.Args.ContainsKey(key))
                    argSet.Args.Add(key, other.Args[key].Clone());
            }

            return argSet;
        }

        public void FillFrom(RpgArgSet other)
        {
            foreach (var arg in other.ArgValues)
            {
                if (Args.ContainsKey(arg.Key) && (!ArgValues.ContainsKey(arg.Key) || ArgValues[arg.Key] == null))
                    ArgValues[arg.Key] = arg.Value;
            }
        }

        public void SetArgValues(ActionInstance? actionInstance, RpgEntity? owner, RpgEntity? initiator, int? actionNo)
        {
            SetArg("actionInstance", actionInstance);
            SetArg("actionNo", actionNo!.Value);
            SetArg("initiator", initiator);
            SetArg("owner", owner);
        }

        public RpgArgSet SetArg(string argName, object? value)
        {
            if (Args.ContainsKey(argName))
            {
                ValidateArgValue(argName, value);
                ArgValues[argName] = value;
            }

            return this;
        }

        public RpgArgSet SetArgValue(string argName, string? value)
        {
            if (Args.ContainsKey(argName))
            {
                var val = Args[argName].ToArgObject(value);
                ValidateArgValue(argName, val);
                ArgValues[argName] = value;
            }

            return this;
        }

        public void SetArgValues(Dictionary<string, string?> args)
        {
            foreach (var key in args.Keys)
                SetArgValue(key, args[key]);
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
