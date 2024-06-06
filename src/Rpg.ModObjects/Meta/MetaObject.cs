using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    public class MetaObject
    {
        public string BaseType { get; set; }
        public string Archetype { get; set; }
        public string[] BaseTypes { get; set; }
        public MetaProperty[] Properties { get; set; }
        public MetaState[] States { get; set; }
        public MetaAction[] Actions { get; set; }
        public bool IsComponent { get; set; }

        public override string ToString()
        {
            return $"({(IsComponent ? "Component" : "Entity")}) {Archetype}";
        }
    }
}
