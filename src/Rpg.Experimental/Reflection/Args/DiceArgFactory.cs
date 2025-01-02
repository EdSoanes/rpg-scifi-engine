using System.Reflection;

namespace Rpg.Experimental.Reflection.Args
{
    public class DiceArgFactory : IRpgArgFactory
    {
        public bool CanCreate(ParameterInfo parameterInfo)
            => parameterInfo.ParameterType == typeof(Dice) || Nullable.GetUnderlyingType(parameterInfo.ParameterType) == typeof(Dice);

        public RpgArg Create(ParameterInfo parameterInfo)
        {
            var arg = new DiceArg(parameterInfo);
            return arg;
        }
    }
}
