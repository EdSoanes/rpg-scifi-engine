using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public class RpgMethod<TOwner, TReturn> 
        where TOwner : class
    {
        [JsonProperty] public string? EntityId { get; internal set; }
        [JsonProperty] public string? ClassName { get; internal set; }
        [JsonProperty] public string MethodName { get; internal set; }
        [JsonProperty] public RpgArg[] Args { get; internal set; } = Array.Empty<RpgArg>();

        public RpgMethod() { }

        public string FullName { get => IsStatic ? $"{ClassName}.{MethodName}" : MethodName; }
        public bool IsStatic { get => !string.IsNullOrEmpty(ClassName); }

        public RpgArgSet CreateArgSet()
        {
            var res = new RpgArgSet();
            foreach (var arg in Args)
            {
                res.Args.Add(arg.Name, arg);
                res.ArgValues.Add(arg.Name, null);
            }

            return res;
        }
        public TReturn Execute(RpgArgSet argSet)
            => RpgReflection.ExecuteMethod<TReturn>(FullName, argSet.ToArgs())!;

        public TReturn Execute(TOwner owner, RpgArgSet argSet)
            => RpgReflection.ExecuteMethod<TReturn>(owner, MethodName, argSet.ToArgs())!;
    }
}
