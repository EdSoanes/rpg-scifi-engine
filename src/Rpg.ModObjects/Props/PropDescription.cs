//using Rpg.ModObjects.Mods;
//using Rpg.ModObjects.Values;

//namespace Rpg.ModObjects.Props
//{
//    public class PropDescription
//    {
//        public string RootEntityId { get; set; }
//        public string RootEntityName { get; set; }  
//        public string RootEntityArchetype { get; set; }
//        public string RootProp { get; set; }

//        public string EntityId { get; set; }
//        public string EntityName { get; set; }
//        public string EntityArchetype { get; set; }

//        public string Prop { get; set; }
//        public Dice Value { get; set; }
//        public Dice BaseValue { get; set; }

//        public List<ModDescription> Mods { get; set; } = new();

//        public override string ToString()
//        {
//            return $"{Value} <= {RootEntityArchetype}.{RootProp}";
//        }
//    }

//    public class ModDescription
//    {
//        public PropDescription? SourceProp { get; set; }
//        public string ModType { get; set; }
//        public string Behavior { get; set; }
//        public Dice? SourceValue { get; set; }
//        public Dice Value { get; set; }
//        public string? ValueFunction { get; set; }

//        public override string ToString()
//        {
//            var src = SourceProp != null
//                ? $"{SourceProp.RootEntityName}.{SourceProp.RootProp}"
//                : SourceValue?.ToString();

//            var type = Behavior == "Threshold" ? Behavior : ModType;
//            return $"{Value} <= ({type}) {src}";
//        }
//    }

//    public static class PropDescExtensions
//    {
//        //public static PropDesc? Describe(this RpgObject? rootObject, RpgGraph graph, string propPath)
//        //{
//        //    var exists = false;
//        //    var (entity, prop) = rootObject.FromPath(propPath);
//        //    if (entity == null || prop == null)
//        //        return null;

//        //    var propVal = entity?.PropertyValue(prop, out exists);


//        //    var propDesc = new PropDesc
//        //    {
//        //        RootEntityId = rootObject!.Id,
//        //        RootEntityArchetype = rootObject.Archetype,
//        //        RootEntityName = rootObject.Name,
//        //        RootProp = propPath,

//        //        EntityId = entity!.Id,
//        //        EntityArchetype = entity.Archetype,
//        //        EntityName = entity.Name,
//        //        Prop = prop,

//        //        Value = graph.CalculatePropValue(entity, prop) ?? Dice.Zero,
//        //        BaseValue = graph.CalculateBasePropValue(entity, prop) ?? Dice.Zero
//        //    };

//        //    propDesc.Mods = entity.GetActiveMods(prop)
//        //        .Select(x => x.Describe(graph))
//        //        .Where(x => x != null)
//        //        .Cast<ModDesc>()
//        //        .ToList();

//        //    return propDesc;
//        //}

//        //public static ModDescription? Describe(this Mod? mod, RpgGraph graph)
//        //{
//        //    if (mod == null) 
//        //        return null;

//        //    var value = graph.CalculateModValue(mod);
//        //    var modDesc = new ModDescription
//        //    {
//        //        ModType = mod.GetType().Name,
//        //        Behavior = mod.Behavior.GetType().Name,
//        //        Value = value ?? Dice.Zero,
//        //        ValueFunction = mod.SourceValueFunc?.FullName,
//        //        SourceValue = mod.SourceValue,
//        //    };

//        //    var sourceEntity = graph.GetObject(mod.Source?.EntityId);
//        //    if (sourceEntity != null)
//        //        modDesc.SourceProp = sourceEntity.Describe(mod.Source!.Prop);

//        //    return modDesc;
//        //}
//    }
//}
