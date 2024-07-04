using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActionAttribute : Attribute
    {
        public bool Required { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }

        public ActionAttribute() { }
    }
}
