using System.Reflection;

namespace Rpg.ModObjects.Reflection.Args
{
    public class IntegerArgFactory : IRpgArgFactory
    {
        public bool CanCreate(ParameterInfo parameterInfo)
            => parameterInfo.ParameterType == typeof(Int32) || Nullable.GetUnderlyingType(parameterInfo.ParameterType) == typeof(Int32);

        public RpgArg Create(ParameterInfo parameterInfo)
        {
            var arg = new IntegerArg(parameterInfo);
            return arg;
        }
    }
}
