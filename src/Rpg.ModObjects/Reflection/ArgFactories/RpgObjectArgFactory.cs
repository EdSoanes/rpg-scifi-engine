using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Reflection.ArgFactories
{
    public class RpgObjectArgFactory : IRpgArgFactory
    {
        public bool CanCreate(ParameterInfo parameterInfo)
            => parameterInfo.ParameterType.IsAssignableTo(typeof(RpgObject));

        public RpgArg Create(ParameterInfo parameterInfo)
        {
            var res = new RpgObjectArg(parameterInfo);
            return res;
        }
    }
}
