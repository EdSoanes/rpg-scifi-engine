using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class InputAttribute : Attribute
    {
        public string Param { get; set; }
        public string BindsTo { get; set; }

        public InputAttribute(string param, string bindsTo)
        {
            Param = param;
            BindsTo = bindsTo;
        }
    }
}
