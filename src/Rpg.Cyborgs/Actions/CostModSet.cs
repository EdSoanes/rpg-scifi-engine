using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time.Lifecycles;
using Rpg.ModObjects.Time.Templates;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.Cyborgs.Actions
{
    public class CostModSet : ModSet
    {
        internal string ActionPointCostProp => $"{ActionName}_{nameof(ActionPointCost)}_{ActionNo}";

        [JsonProperty] private string ActionName { get; set; }
        [JsonProperty] private int ActionNo { get; set; }

        public Dice? ActionPointCost(RpgGraph graph)
            => graph.CalculatePropValue(new PropRef(OwnerId!, ActionPointCostProp));

        [JsonConstructor] private CostModSet() { }

        public CostModSet(string ownerId, string actionName, int actionNo, string? name = null)
            : base(ownerId, new TurnLifecycle(), name)
        {
            ActionName = actionName;
            ActionNo = actionNo;
        }

        public CostModSet Cost<TInitiator>(TInitiator initiator, string name, Dice dice)
            where TInitiator : RpgEntity
                => this.Add(new TurnMod().SetName(name), initiator, ActionPointCostProp, dice);

        public CostModSet Cost<TInitiator, TSourceValue>(TInitiator initiator, Expression<Func<TInitiator, TSourceValue>> sourceExpr)
            where TInitiator : RpgEntity
                => this.Add(new TurnMod(), initiator, ActionPointCostProp, sourceExpr);

        public CostModSet Cost<TInitiator>(TInitiator initiator, string sourceProp)
            where TInitiator : RpgEntity
                => this.Add(new TurnMod(), initiator, ActionPointCostProp, sourceProp);
    }
}
