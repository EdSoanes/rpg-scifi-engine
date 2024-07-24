using Newtonsoft.Json;
using System.Reflection;

namespace Rpg.ModObjects.Reflection.ArgFactories
{
    public class DefaultArgFactory : IRpgArgFactory
    {
        public string? ToArgString(object? value)
            => value?.ToString();

        public object? ToArgObject(string? value) 
            => null;

        public bool CanCreate(ParameterInfo parameterInfo)
            => (parameterInfo.ParameterType.IsClass || parameterInfo.ParameterType.IsInterface)
                && !parameterInfo.ParameterType.IsAssignableTo(typeof(RpgObject));

        public RpgArg Create(ParameterInfo parameterInfo)
        {
            var res = new DefaultArg(parameterInfo);
            return res;
        }
    }
}
