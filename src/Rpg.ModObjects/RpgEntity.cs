using Newtonsoft.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects
{
    public abstract class RpgEntity : RpgObject
    {
        public Dictionary<string, Actions.Action> Actions { get; init; }

        [JsonConstructor]
        protected RpgEntity()
            : base()
        {
            Actions = new Dictionary<string, Actions.Action>();
        }

        public RpgEntity(string name)
            : this()
        {
            Name = name;
        }

        #region Actions

        public Actions.Action? GetAction(string action)
            => Actions.ContainsKey(action) ? Actions[action] : null;

        #endregion Actions

        #region ModSets

        #endregion ModSets

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);

            var actions = ModObjects.Actions.Action.CreateOwnerActions(this);
            foreach (var action in actions)
            {
                action.OnAdding(Graph!);
                if (!Actions.ContainsKey(action.Name))
                    Actions.Add(action.Name, action);
            }
        }

        public override void OnRestoring(RpgGraph graph)
        {
            base.OnRestoring(graph);

            foreach (var action in Actions.Values)
                action.OnAdding(Graph);
        }
    }
}


