//using Newtonsoft.Json;
//using Rpg.ModObjects.Values;
//using System.Linq.Expressions;

//namespace Rpg.ModObjects.Modifiers
//{
//    public class ExternalMod : Mod
//    {
//        [JsonConstructor] private ExternalMod() { }

//        public ExternalMod(ModPropRef targetPropRef)
//            : this(nameof(ExternalMod), targetPropRef)
//        {
//        }

//        public ExternalMod(string name, ModPropRef targetPropRef)
//        {
//            Name = name;
//            ModifierType = ModType.Permanent;
//            ModifierAction = ModAction.Accumulate;
//            Duration = ModDuration.External();
//            EntityId = targetPropRef.EntityId;
//            Prop = targetPropRef.Prop;
//        }
//    }
//}
