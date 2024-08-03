using Newtonsoft.Json;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects
{
    public abstract class RpgEntity : RpgObject
    {
        public Dictionary<string, Actions.Action> Actions { get; init; }

        [JsonConstructor] protected RpgEntity()
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

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            if (Graph == null)
            { 
                base.OnBeforeTime(graph, entity);

                foreach (var component in this.Traverse<RpgComponent, RpgEntity>())
                    component.SetEntityPropRef(Id, this.PathTo(component));

                foreach (var state in States.Values)
                    state.OnAdding(graph);

                foreach (var action in Actions.Values)
                    action.OnAdding(graph);
            }
        }

        public override void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeginningOfTime(graph, entity);
            var actions = ModObjects.Actions.Action.CreateOwnerActions(this);
            foreach (var action in actions)
            {
                action.OnAdding(graph);
                Actions.Add(action.Name, action);
            }
        }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            var expiry = base.OnStartLifecycle(graph, currentTime);
            return expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            var expiry = base.OnUpdateLifecycle(graph, currentTime);
            return expiry;
        }
    }
}
