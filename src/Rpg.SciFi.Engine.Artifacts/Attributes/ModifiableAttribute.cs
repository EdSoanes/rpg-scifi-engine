using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ModifiableAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public ModifiableAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
