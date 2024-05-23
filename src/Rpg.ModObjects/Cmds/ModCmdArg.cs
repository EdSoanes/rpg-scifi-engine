using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Actions
{
    public class ModCmdArg
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public ModCmdArgType ArgType { get; set; }

        internal static ModCmdArg Create(ParameterInfo parameterInfo)
        {
            var arg = new ModCmdArg
            {
                Name = parameterInfo.Name!,
                DataType = parameterInfo.ParameterType.FullName!,
                IsNullable = Nullable.GetUnderlyingType(parameterInfo.ParameterType) != null
            };

            return arg;
        }

        public bool IsOfType(object? obj)
        {
            if (IsNullable && obj == null)
                return true;

            if (obj != null && obj.GetType().FullName == DataType)
                return true;

            return false;
        }
    }
}
