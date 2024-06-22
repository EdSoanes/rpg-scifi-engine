using Newtonsoft.Json;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public class RpgArgSet
    {
        private Type? _type;

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
                if (ArgValues.ContainsKey(key) && IsValid(key, value))
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
                _type = _type
            };

        public bool IsValid(string arg, object? value)
        {
            var modArgs = Args.Where(x => x.Name == arg);
            if (modArgs.Count() != 1)
                return false;

            var modArg = modArgs.Single();
            if (!modArg.IsNullable && value == null)
                return false;

            if (string.IsNullOrEmpty(modArg.TypeName))
                return false;

            if (value == null)
                return true;

            if (_type == null)
             _type = RpgReflection.ScanForType(modArg.QualifiedTypeName);

            return _type!.IsAssignableTo(value!.GetType());
        }

        public object?[] ToArgs()
        {
            var res = new List<object?>();
            foreach (var arg in Args)
            {
                if (ArgValues.ContainsKey(arg.Name))
                {
                    var obj = ArgValues[arg.Name];
                    res.Add(obj);
                }
                else
                    res.Add(null);
            }

            return res.ToArray();
        }
    }
}
