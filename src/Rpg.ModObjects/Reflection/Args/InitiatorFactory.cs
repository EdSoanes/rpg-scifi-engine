using System.Reflection;

namespace Rpg.ModObjects.Reflection.Args
{
    public class InitiatorFactory : RpgObjectArgFactory
    {
        public override bool CanCreate(ParameterInfo parameterInfo)
            => base.CanCreate(parameterInfo) && parameterInfo.Name == "initiator";

        public override RpgArg Create(ParameterInfo parameterInfo)
            => new RpgObjectArg(parameterInfo, "initiator");
    }
}
