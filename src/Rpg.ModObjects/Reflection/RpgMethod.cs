using Newtonsoft.Json;
using Rpg.ModObjects.Reflection.Args;
using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public class RpgMethod<TOwner, TReturn>
        where TOwner : class
    {
        [JsonProperty] public string? EntityId { get; internal set; }
        [JsonProperty] public string? ClassName { get; internal set; }
        [JsonProperty] public string MethodName { get; internal set; }
        [JsonProperty] public string? ReturnTypeName { get; internal set; }
        [JsonProperty] public string? ReturnQualifiedTypeName { get; internal set; }
        [JsonProperty] public bool ReturnIsNullable { get; internal set; }

        [JsonProperty] public RpgArg[] Args { get; internal set; } = Array.Empty<RpgArg>();

        public RpgMethod() { }

        public string FullName { get => IsStatic ? $"{ClassName}.{MethodName}" : MethodName; }
        public bool IsStatic { get => !string.IsNullOrEmpty(ClassName); }

        public TReturn Execute(Dictionary<string, object?> args)
            => ExecuteMethod(FullName, args.Values.ToArray())!;

        public TReturn Execute(TOwner owner, Dictionary<string, object?> args)
            => ExecuteMethod(owner, MethodName, args.Values.ToArray())!;

        internal static TReturn? ExecuteMethod(string staticMethod, object?[]? args = null)
        {
            var methodInfo = RpgTypeScan.ForMethod(staticMethod);
            if (methodInfo == null)
                return default;

            var methodArgs = GetValidatedMethodArgs(methodInfo, args);
            return (TReturn?)methodInfo.Invoke(null, methodArgs);
        }

        internal static TReturn? ExecuteMethod(object obj, string methodName, object?[]? args = null)
        {
            var methodInfo = RpgTypeScan.ForMethod(obj.GetType(), methodName);
            if (methodInfo == null)
                return default;

            var methodArgs = GetValidatedMethodArgs(methodInfo, args);
            return (TReturn?)methodInfo.Invoke(obj, methodArgs);
        }

        private static object?[]? GetValidatedMethodArgs(MethodInfo methodInfo, object?[]? args)
        {
            var parameters = methodInfo.GetParameters();
            if (parameters.Length > 0 && args != null && args.Length == parameters.Length)
            {
                var updatedArgs = new List<object?>();
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parm = parameters[i];
                    var val = args[i];

                    if (parm.ParameterType == typeof(Dice) && val is int num)
                        updatedArgs.Add(new Dice(num));
                    else if (parm.ParameterType == typeof(int) && val is Dice dice)
                        updatedArgs.Add(dice.Roll());
                    else
                        updatedArgs.Add(val);
                }

                return updatedArgs.ToArray();
            }

            return args;
        }
    }
}
