using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Reflection.ArgFactories
{
    public class DiceArgFactory : IRpgArgFactory
    {
        public bool CanCreate(ParameterInfo parameterInfo)
            => parameterInfo.ParameterType == typeof(Dice);

        public RpgArg Create(ParameterInfo parameterInfo)
        {
            var arg = new DiceArg(parameterInfo);
            return arg;
        }
    }
}
