using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Reflection.ArgFactories
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
