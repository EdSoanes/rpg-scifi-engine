using System.Reflection;

namespace Rpg.ModObjects.Reflection.Args
{
    public class DefaultArgFactory : IRpgArgFactory
    {
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
