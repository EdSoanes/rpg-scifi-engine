//using Newtonsoft.Json;
//using Rpg.ModObjects.Lifecycles;
//using Rpg.ModObjects.Mods;
//using Rpg.ModObjects.Mods.Mods;
//using Rpg.ModObjects.Mods.ModSets;
//using Rpg.ModObjects.Props;
//using Rpg.ModObjects.Time;
//using Rpg.ModObjects.Values;
//using System.Linq.Expressions;

//namespace Rpg.ModObjects.Actions
//{
//    public class ActionModSet : TimedModSet
//    {
//        internal string DiceRollProp => $"{ActionName}_DiceRoll_{ActionNo}";
//        internal string TargetProp => $"{ActionName}_Target_{ActionNo}";

//        [JsonProperty] private string ActionName { get; set; }
//        [JsonProperty] private int ActionNo { get; set; }

//        [JsonConstructor] private ActionModSet() { }

//        public ActionModSet(string ownerId, string actionName, int actionNo, string? name = null)
//            : base(ownerId, name ?? actionName, new SpanOfTime(0, 1))
//        {
//            ActionName = actionName;
//            ActionNo = actionNo;
//        }

//        public bool IsResolved<TInitiator>(TInitiator initiator, RpgGraph graph)
//            where TInitiator : RpgEntity
//        {
//            var target = Target(graph);
//            var diceRoll = DiceRoll(graph);

//            return (diceRoll?.IsConstant ?? true)
//                && target == null;
//        }

//        public Dice? Target(RpgGraph graph)
//            => graph.CalculatePropValue(new PropRef(OwnerId!, TargetProp));

//        public ActionModSet Target<TInitiator>(TInitiator initiator, string name, Dice dice)
//            where TInitiator : RpgEntity
//                => this.Add(new Turn().SetName(name), initiator, TargetProp, dice);

//        public ActionModSet Target<TInitiator, TSourceValue>(TInitiator initiator, Expression<Func<TInitiator, TSourceValue>> sourceExpr)
//            where TInitiator : RpgEntity
//                => this.Add(new Turn(), initiator, TargetProp, sourceExpr);

//        public ActionModSet Target<TInitiator>(TInitiator initiator, string sourceProp)
//            where TInitiator : RpgEntity
//                => this.Add(new Turn(), initiator, TargetProp, sourceProp);


//        public Dice? DiceRoll(RpgGraph graph)
//            => graph.CalculatePropValue(new PropRef(OwnerId!, DiceRollProp));

//        public ActionModSet DiceRoll<TTarget>(TTarget target, string name, Dice dice)
//            where TTarget : RpgEntity
//                => this.Add(new Turn().SetName(name), target, DiceRollProp, dice);

//        public ActionModSet DiceRoll<TTarget, TSourceValue>(TTarget target, Expression<Func<TTarget, TSourceValue>> sourceExpr)
//            where TTarget : RpgEntity
//                => this.Add(new Turn(), target, DiceRollProp, sourceExpr);

//        public ActionModSet DiceRoll<TTarget, TSource, TSourceValue>(TTarget target, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr)
//            where TTarget : RpgEntity
//            where TSource : RpgEntity
//                => this.Add(new Turn(), target, DiceRollProp, source, sourceExpr);

//        public ActionModSet DiceRoll<TTarget>(TTarget target, string sourceProp)
//            where TTarget : RpgEntity
//                => this.Add(new Turn(), target, DiceRollProp, sourceProp);
//    }
//}
