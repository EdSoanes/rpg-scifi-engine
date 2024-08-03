using System.Reflection;

namespace Rpg.ModObjects.Reflection.Args
{
    public abstract class RpgObjectArgFactory : IRpgArgFactory
    {
        public virtual bool CanCreate(ParameterInfo parameterInfo)
            => parameterInfo.ParameterType.IsAssignableTo(typeof(RpgObject)) 
                || (Nullable.GetUnderlyingType(parameterInfo.ParameterType)?.IsAssignableTo(typeof(RpgObject)) ?? false);

        public virtual RpgArg Create(ParameterInfo parameterInfo)
            => new RpgObjectArg(parameterInfo);
    }
}
