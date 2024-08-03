using System.Reflection;

namespace Rpg.ModObjects.Reflection.Args
{
    public class DefaultObjectFactory : RpgObjectArgFactory
    {
        public override bool CanCreate(ParameterInfo parameterInfo)
            => base.CanCreate(parameterInfo);

        public override RpgArg Create(ParameterInfo parameterInfo)
            => base.Create(parameterInfo);
    }
}
