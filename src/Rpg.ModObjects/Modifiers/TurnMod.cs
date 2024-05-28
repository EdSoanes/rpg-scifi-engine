//using Newtonsoft.Json;
//using Rpg.ModObjects.Values;
//using System.Linq.Expressions;

//namespace Rpg.ModObjects.Modifiers
//{
//    public class TurnMod : Mod
//    {
//        [JsonConstructor] private TurnMod() { }

//        public TurnMod(ModPropRef targetPropRef)
//            : this(nameof(TurnMod), targetPropRef)
//        {
//        }

//        public TurnMod(string name, ModPropRef targetPropRef)
//        {
//            Name = name;
//            ModifierType = ModType.Transient;
//            ModifierAction = ModAction.Accumulate;
//            Duration = ModDuration.Permanent();
//            EntityId = targetPropRef.EntityId;
//            Prop = targetPropRef.Prop;
//        }

//        public override void OnAdd(int turn)
//        {
//            Duration = ModDuration.OnNewTurn(turn);
//        }
//    }

//    public static class TurnModExtensions
//    {
//        public static Mod AddTurnMod<TEntity>(this TEntity entity, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
//            where TEntity : ModObject
//        {
//            var mod = Mod.Create<TurnMod, TEntity>(entity, targetProp, value, diceCalcExpr);
//            entity.AddMod(mod);

//            return mod;
//        }

//        public static Mod AddTurnMod<TEntity, T1>(this TEntity entity, Expression<Func<TEntity, T1>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
//            where TEntity : ModObject
//        {
//            var mod = Mod.Create<TurnMod, TEntity, T1>(entity, targetExpr, value, diceCalcExpr);
//            entity.AddMod(mod);

//            return mod;
//        }

//        public static Mod AddTurnMod<TEntity, T1>(this TEntity entity, string name, Expression<Func<TEntity, T1>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
//            where TEntity : ModObject
//        {
//            var mod = Mod.Create<TurnMod, TEntity, T1>(name, entity, targetExpr, value, diceCalcExpr);
//            entity.AddMod(mod);

//            return mod;
//        }
//    }
//}