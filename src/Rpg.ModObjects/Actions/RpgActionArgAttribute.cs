using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Actions

{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RpgActionArgAttribute : Attribute
    {
        public string Prop {  get; set; }
        public RpgActionArgType ArgType { get; set; }

        public RpgActionArgAttribute(string prop, RpgActionArgType argType)
        {  
            Prop = prop; 
            ArgType = argType;
        }
    }
}
