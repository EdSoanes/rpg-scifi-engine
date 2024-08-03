//using Newtonsoft.Json;
//using Rpg.ModObjects.Reflection.Args;

//namespace Rpg.ModObjects.Reflection
//{
//    public class RpgArgSet
//    {
//        [JsonProperty] public Dictionary<string, RpgArg> Args { get; private set; } = new();
//        [JsonProperty] public Dictionary<string, object?> ArgValues { get; private set; } = new();

//        public RpgArgSet() { }

//        private void SetArgValue(string arg, object? value)
//        {
//            if (!Args.ContainsKey(arg))
//                return;

//            var modArg = Args[arg];
//            if (string.IsNullOrEmpty(modArg.TypeName))
//                throw new ArgumentException($"Type name for param '{arg}' not set");

//            if (modArg.IsNullable && value == null)
//            {
//                ArgValues[arg] = null;
//                return;
//            }

//            var val = modArg.ToArgValue(value);
//            if (!modArg.IsNullable && val == null)
//                return;

//            if (val != null)
//            {
//                var type = RpgTypeScan.ForType(modArg.QualifiedTypeName);
//                if (type == null)
//                    throw new ArgumentException($"Could not find param type for param '{arg}'");

//                if (value != null && !value.GetType().IsAssignableTo(type))
//                    throw new ArgumentException($"'{arg}' value with type {value.GetType().Name} not assignable to method param with with type {type.Name}");
//            }

//            ArgValues[arg] = val;
//        }

//        public RpgArgSet Merge(RpgArgSet other)
//        {
//            var argSet = new RpgArgSet();
//            foreach (var key in Args.Keys)
//                argSet.Args.Add(key, Args[key]);

//            argSet.FillFrom(this);

//            foreach (var key in other.Args.Keys)
//                if (!argSet.Args.ContainsKey(key))
//                    argSet.Args.Add(key, other.Args[key]);

//            argSet.FillFrom(other);

//            return argSet;
//        }

//        public void FillFrom(RpgArgSet other)
//        {
//            foreach (var argName in other.ArgValues.Keys)
//            {
//                var argVal = other.ArgValues[argName];
//                if (!ArgValues.ContainsKey(argName))
//                    ArgValues.Add(argName, argVal);
//                else if (ArgValues[argName] == null)
//                {
//                    if (!Args[argName].IsValid(argVal))
//                        throw new InvalidOperationException($"Mismatched arg types {Args[argName].TypeName} and {other.Args[argName].TypeName}");

//                    ArgValues[argName] = argVal;
//                }
//            }
//        }

//        public void Set(RpgActivity? activity, RpgEntity? owner, RpgEntity? initiator)
//        {
//            Set("activity", activity);
//            Set("initiator", initiator);
//            Set("owner", owner);
//        }

//        public RpgArgSet Set(int idx, object? value)
//        {
//            if (idx >= 0 && idx < Args.Count())
//            {
//                var argName = Args.Keys.ToArray()[idx];
//                SetArgValue(argName, value);
//            }

//            return this;
//        }

//        public RpgArgSet Set(string argName, object? value)
//        {
//            SetArgValue(argName, value);
//            return this;
//        }

//        public RpgArgSet SetStr(string argName, string? value)
//        {
//            if (Args.ContainsKey(argName))
//            {
//                var val = Args[argName].ToArgObject(value);
//                SetArgValue(argName, val);
//            }

//            return this;
//        }

//        public void Set(Dictionary<string, string?> args)
//        {
//            foreach (var key in args.Keys)
//                SetStr(key, args[key]);
//        }

//        public object? ArgValue(string argName, object? argValue)
//        {
//            return Args.ContainsKey(argName)
//                ? Args[argName].ToArgValue(argValue) 
//                : null;
//        }

//        public object? ArgValueFromString(string argName, string? argStr)
//        {
//            return Args.ContainsKey(argName)
//                ? Args[argName].ToArgObject(argStr)
//                : null;
//        }

//        public object?[] ToArgs()
//        {
//            var res = new List<object?>();
//            foreach (var arg in Args.Where(x => ArgValues.ContainsKey(x.Key)))
//            {
//                var obj = ArgValues[arg.Key];
//                if (obj != null || arg.Value.IsNullable)
//                    res.Add(obj);
//            }

//            return res.ToArray();
//        }
//    }
//}
