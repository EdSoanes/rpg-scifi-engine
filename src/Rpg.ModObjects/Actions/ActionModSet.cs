using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time.Lifecycles;
using Rpg.ModObjects.Time.Templates;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Actions
{
    public class ActionModSet : ModSet
    {
        internal string DiceRollProp => $"{ActionName}_DiceRoll_{ActionNo}";
        internal string TargetProp => $"{ActionName}_Target_{ActionNo}";

        [JsonProperty] private string ActionName { get; set; }
        [JsonProperty] private int ActionNo { get; set; }

        [JsonConstructor] private ActionModSet() { }

        public ActionModSet(string ownerId, string actionName, int actionNo, string? name = null)
            : base(ownerId, new TurnLifecycle(), name)
        {
            ActionName = actionName;
            ActionNo = actionNo;
        }

        public bool IsResolved<TInitiator>(TInitiator initiator, RpgGraph graph)
            where TInitiator : RpgEntity
        {
            var target = Target(graph);
            var diceRoll = DiceRoll(graph);

            return (diceRoll?.IsConstant ?? true)
                && target == null;
        }

        public Dice? Target(RpgGraph graph)
            => graph.CalculatePropValue(new PropRef(OwnerId!, TargetProp));

        public ActionModSet Target<TInitiator>(TInitiator initiator, string name, Dice dice)
            where TInitiator : RpgEntity
                => this.Add(new TurnMod().SetName(name), initiator, TargetProp, dice);

        public ActionModSet Target<TInitiator, TSourceValue>(TInitiator initiator, Expression<Func<TInitiator, TSourceValue>> sourceExpr)
            where TInitiator : RpgEntity
                => this.Add(new TurnMod(), initiator, TargetProp, sourceExpr);

        public ActionModSet Target<TInitiator>(TInitiator initiator, string sourceProp)
            where TInitiator : RpgEntity
                => this.Add(new TurnMod(), initiator, TargetProp, sourceProp);


        public Dice? DiceRoll(RpgGraph graph)
            => graph.CalculatePropValue(new PropRef(OwnerId!, DiceRollProp));

        public ActionModSet DiceRoll<TInitiator>(TInitiator initiator, string name, Dice dice)
            where TInitiator : RpgEntity
                => this.Add(new TurnMod().SetName(name), initiator, DiceRollProp, dice);

        public ActionModSet DiceRoll<TInitiator, TSourceValue>(TInitiator initiator, Expression<Func<TInitiator, TSourceValue>> sourceExpr)
            where TInitiator : RpgEntity
                => this.Add(new TurnMod(), initiator, DiceRollProp, sourceExpr);

        public ActionModSet DiceRoll<TInitiator>(TInitiator initiator, string sourceProp)
            where TInitiator : RpgEntity
                => this.Add(new TurnMod(), initiator, DiceRollProp, sourceProp);
    }
}
