using Newtonsoft.Json;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    public class MetaObject
    {
        public string Archetype { get; set; }
        public MetaObjectType ObjectType { get; set; } = MetaObjectType.Entity;

        public string? TemplateType { get; set; }
        public MetaProperty[] Properties { get; set; }
        public MetaState[] States { get; set; }
        public MetaAction[] Actions { get; set; }

        [JsonProperty] public string[] ParentTo { get; internal set; } = Array.Empty<string>();

        public override string ToString()
        {
            return $"({ObjectType}) {Archetype}";
        }
    }
}
