using Newtonsoft.Json;
using Rpg.ModObjects.Reflection;

namespace Rpg.ModObjects.Actions
{
    public class ActionInstance
    {
        private RpgGraph? Graph { get; set; }

        private RpgEntity? _owner;
        [JsonIgnore] public RpgEntity? Owner 
        { 
            get
            {
                if (_owner == null && Graph != null)
                    _owner = Graph.GetObject<RpgEntity>(OwnerId);

                return _owner;
            }
        }

        private Action? _action;
        [JsonIgnore] public Action? Action
        {
            get
            {
                if (_action == null && Graph != null)
                    _action = Owner?.GetAction(ActionName);

                return _action;
            }
        }

        [JsonProperty] public string OwnerId { get; protected set; }
        //[JsonProperty] public string InitiatorId { get; protected set; }
        [JsonProperty] public string ActionName { get; protected set; }
        [JsonProperty] public int ActionNo { get; protected set; }

        [JsonProperty] public RpgArgSet? CanActArgs {  get; protected set; }
        [JsonProperty] public RpgArgSet? CostArgs { get; protected set; }
        [JsonProperty] public RpgArgSet? ActArgs { get; protected set; }
        [JsonProperty] public RpgArgSet? OutcomeArgs { get; protected set; }
        [JsonProperty] public RpgArgSet? AutoCompleteArgs { get; protected set; }

        public ActionInstance(RpgEntity owner, Action action, int actionNo)
        {
            ActionName = action.Name;
            ActionNo = actionNo;
            OwnerId = owner.Id;

            CanActArgs = Action!.CanActArgs();
            CostArgs = Action.CostArgs();
            ActArgs = Action.ActArgs();
            OutcomeArgs = Action.OutcomeArgs();
            AutoCompleteArgs = CanActArgs
                .Merge(CostArgs)
                .Merge(ActArgs)
                .Merge(OutcomeArgs);
        }
    }
}
