using System.Reflection;

namespace Rpg.ModObjects.Reflection.Args
{
    public class ActivityFactory : RpgObjectArgFactory
    {
        public override bool CanCreate(ParameterInfo parameterInfo)
            => base.CanCreate(parameterInfo) && parameterInfo.Name == "activity" && parameterInfo.ParameterType == typeof(RpgActivity);

        public override RpgArg Create(ParameterInfo parameterInfo)
            => new RpgObjectArg(parameterInfo, "activity");
    }
}
