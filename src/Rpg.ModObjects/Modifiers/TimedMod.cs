//using Newtonsoft.Json;
//using Rpg.ModObjects.Values;
//using System.Linq.Expressions;

//namespace Rpg.ModObjects.Modifiers
//{
//    public class TimedMod : Mod
//    {
//        [JsonConstructor] private TimedMod() { }

//        public TimedMod(ModPropRef targetPropRef)
//            : this(nameof(TimedMod), targetPropRef)
//        {
//        }

//        public TimedMod(string name, ModPropRef targetPropRef)
//        {
//            Name = name;
//            ModifierType = ModType.Transient;
//            ModifierAction = ModAction.Sum;
//            Duration = ModDuration.Timed(1, 1);
//            EntityId = targetPropRef.EntityId;
//            Prop = targetPropRef.Prop;
//        }
//    }

//    public static class TimedModExtensions
//    {
//        public static Mod AddTimedMod<TEntity, T1, T2>(this TEntity entity, int startTurn, int duration, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<TEntity, T2>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
//            where TEntity : ModObject
//        {
//            var mod = Mod.Create<TimedMod, TEntity, T1, TEntity, T2>(entity, targetExpr, entity, sourceExpr, diceCalcExpr);
//            mod.Duration.SetDuration(startTurn, startTurn + duration - 1);
//            entity.AddMod(mod);

//            return mod;
//        }

//        public static Mod AddTimedMod<TEntity, T1>(this TEntity entity, int startTurn, int duration, Expression<Func<TEntity, T1>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
//            where TEntity : ModObject
//        {
//            var mod = Mod.Create<TimedMod, TEntity, T1>(entity, targetExpr, value, diceCalcExpr);
//            mod.Duration.SetDuration(startTurn, startTurn + duration - 1);
//            entity.AddMod(mod);

//            return mod;
//        }

//        public static Mod AddTimedMod<TEntity, T1, T2>(this TEntity entity, string name, int startTurn, int duration, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<TEntity, T2>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
//            where TEntity : ModObject
//        {
//            var mod = Mod.Create<TimedMod, TEntity, T1, TEntity, T2>(name, entity, targetExpr, entity, sourceExpr, diceCalcExpr);
//            mod.Duration.SetDuration(startTurn, startTurn + duration - 1);
//            entity.AddMod(mod);

//            return mod;
//        }

//        public static Mod AddTimedMod<TEntity, T1>(this TEntity entity, string name, int startTurn, int duration, Expression<Func<TEntity, T1>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
//            where TEntity : ModObject
//        {
//            var mod = Mod.Create<TimedMod, TEntity, T1>(name, entity, targetExpr, value, diceCalcExpr);
//            mod.Duration.SetDuration(startTurn, startTurn + duration - 1);
//            entity.AddMod(mod);

//            return mod;
//        }
//    }
//}
