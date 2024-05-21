using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Actions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModCmdAttribute : Attribute
    {
        public string OutcomeMethod { get; set; }
    }
}
