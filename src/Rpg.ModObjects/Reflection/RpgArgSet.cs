using Newtonsoft.Json;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public class RpgArgSet
    {
        [JsonProperty] private RpgArg[] Args { get; set; }
        [JsonProperty] private Dictionary<string, object?> ArgValues { get; set; } = new Dictionary<string, object?>();

        public object? this[string key]
        {
            get
            {
                return ArgValues.TryGetValue(key, out var val)
                    ? val
                    : null;
            }
            set
            {
                ValidateArgValue(key, value);

                if (ArgValues.ContainsKey(key))
                    ArgValues[key] = value;
                else
                    ArgValues.Add(key, value);
            }
        }

        public RpgArg? this[int index]
        {
            get
            {
                return index < Args.Length
                    ? Args[index]
                    : null;
            }
        }

        [JsonConstructor] private RpgArgSet() { }

        public RpgArgSet(ParameterInfo[] parameterInfos)
        {
            Args = parameterInfos.Select(x => new RpgArg(x)).ToArray();
        }

        public string[] ArgNames()
            => Args.Select(x => x.Name).ToArray();

        public int Count()
            => Args.Length;

        public RpgArgSet Clone()
            => new RpgArgSet
            {
                Args = Args.Select(x => x.Clone()).ToArray(),
            };

        public void ValidateArgValue(string arg, object? value)
        {
            var modArgs = Args.Where(x => x.Name == arg);
            if (modArgs.Count() != 1)
                throw new ArgumentException($"'{arg}' param does not exist");

            var modArg = modArgs.Single();
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

        public bool HasArg(string arg)
            => Args.Any(x => x.Name == arg);

        public object?[] ToArgs()
        {
            var res = new List<object?>();
            foreach (var arg in Args)
            {
                var obj = this[arg.Name];
                if (obj != null || arg.IsNullable)
                    res.Add(obj);
            }

            return res.ToArray();
        }
    }
}
