using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    public class MetaObj
    {
        public string Archetype { get; set; }
        public string? DisplayName { get; set; }
        public string? Icon { get; set; }
        public List<string> AllowedChildArchetypes { get; private set; } = new List<string>();
        public List<MetaProp> Props { get; set; } = new List<MetaProp>();

        public MetaObj() { }

        public MetaObj(string archetype, string? displayName = null)
        {
            Archetype = archetype;
            DisplayName = displayName ?? archetype;
        }

        public MetaObj AddProp(string prop, string type, string? displayName = null)
        {
            if (!Props.Any(x => x.Prop == prop))
                Props.Add(new MetaProp { Prop = prop, Type = type, DisplayName = displayName ?? prop });

            return this;
        }

        public MetaObj AddAllowedArchetype(string archetype)
        {
            if (!AllowedChildArchetypes.Any(x => x == archetype))
                AllowedChildArchetypes.Add(archetype);

            return this;
        }

        public MetaObj AddAllowedSelf()
        {
            if (!AllowedChildArchetypes.Any(x => x == Archetype))
                AllowedChildArchetypes.Add(Archetype);

            return this;
        }
    }
}
