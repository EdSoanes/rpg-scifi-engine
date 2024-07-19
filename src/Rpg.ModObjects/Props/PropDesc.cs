using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Props
{
    public class PropDesc
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string EntityArchetype { get; set; }

        public string Prop { get; set; }
        public Dice Value { get; set; }

        public List<ModDesc> Mods { get; set; } = new();
    }

    public class ModDesc
    {
        public PropDesc? SourceProp { get; set; }
        public ModType ModType { get; set; }
        public Dice? SourceValue { get; set; }
        public Dice Value { get; set; }
        public string? ValueFunction { get; set; }
    }

    public static class PropDescExtensions
    {
        public static PropDesc? Describe(this RpgObject? rpgObject, RpgGraph graph, string prop)
        {
            var exists = false;
            rpgObject?.PropertyValue(prop, out exists);

            if (rpgObject == null || !exists)
                return null;

            var propDesc = new PropDesc
            {
                EntityId = rpgObject.Id,
                EntityArchetype = rpgObject.Archetype,
                EntityName = rpgObject.Name,
                Prop = prop,
                Value = graph.CalculatePropValue(rpgObject, prop) ?? Dice.Zero
            };

            propDesc.Mods = rpgObject.GetActiveMods(prop)
                .Select(x => x.Describe(graph))
                .Where(x => x != null)
                .Cast<ModDesc>()
                .ToList();

            return propDesc;
        }

        public static ModDesc? Describe(this Mod? mod, RpgGraph graph)
        {
            if (mod == null) 
                return null;

            var value = graph.CalculateModValue(mod);
            var modDesc = new ModDesc
            {
                ModType = mod.Behavior.Type,
                Value = value,
                ValueFunction = mod.SourceValueFunc?.FullName,
                SourceValue = mod.SourceValue,
            };

            var sourceEntity = graph.GetObject(mod.SourcePropRef?.EntityId);
            if (sourceEntity != null)
                modDesc.SourceProp = sourceEntity.Describe(graph, mod.SourcePropRef!.Prop);

            return modDesc;
        }
    }
}
