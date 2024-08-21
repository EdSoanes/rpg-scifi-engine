using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time.Templates;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Actions
{
    public class OutcomeModSet : ModSet
    {
        internal string OutcomeProp => $"{ActionName}_Outcome_{ActionNo}";

        [JsonProperty] private string ActionName { get; set; }
        [JsonProperty] private int ActionNo { get; set; }

        [JsonConstructor] private OutcomeModSet() { }

        public OutcomeModSet(string ownerId, string actionName, int actionNo, string? name = null)
            : base(ownerId, new TurnLifecycle(), name)
        {
            ActionName = actionName;
            ActionNo = actionNo;
        }

        public Dice? Outcome(RpgGraph graph)
            => graph.CalculatePropValue(new PropRef(OwnerId!, OutcomeProp));

        public OutcomeModSet Outcome<TInitiator>(TInitiator initiator, string name, Dice dice)
            where TInitiator : RpgEntity
                => this.Add(new TurnMod().SetName(name), initiator, OutcomeProp, dice);

        public OutcomeModSet Outcome<TInitiator, TSourceValue>(TInitiator initiator, Expression<Func<TInitiator, TSourceValue>> sourceExpr)
            where TInitiator : RpgEntity
                => this.Add(new TurnMod(), initiator, OutcomeProp, sourceExpr);

        public OutcomeModSet Outcome<TInitiator, TSource, TSourceValue>(TInitiator initiator, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr)
            where TSource : RpgObject
            where TInitiator : RpgEntity
                => this.Add(new TurnMod(), initiator, OutcomeProp, source, sourceExpr);

        public OutcomeModSet Outcome<TInitiator>(TInitiator initiator, string sourceProp)
            where TInitiator : RpgEntity
                => this.Add(new TurnMod(), initiator, OutcomeProp, sourceProp);
    }
}
