using System.Reflection;

namespace Rpg.ModObjects.Reflection.Args
{
    public class OwnerFactory : RpgObjectArgFactory
    {
        public override bool CanCreate(ParameterInfo parameterInfo)
            => base.CanCreate(parameterInfo) && parameterInfo.Name == "owner";

        public override RpgArg Create(ParameterInfo parameterInfo)
            => new RpgObjectArg(parameterInfo, "owner");
    }
}
