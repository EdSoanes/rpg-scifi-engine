using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Cmds
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ModCmdArgAttribute : Attribute
    {
        public string Prop {  get; set; }
        public ModCmdArgType ArgType { get; set; }

        public ModCmdArgAttribute(string prop, ModCmdArgType argType)
        {  
            Prop = prop; 
            ArgType = argType;
        }
    }
}
