using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Activities
{
    internal class ActionPropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public object? Default { get; set; }
    }
}
