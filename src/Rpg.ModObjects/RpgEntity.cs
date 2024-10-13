using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public abstract class RpgEntity : RpgObject
    {
        [JsonProperty] public ActionsDictionary Actions { get; private set; }

        public RpgEntity() : base() 
        {
            Actions = new ActionsDictionary();
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
                if (action != null)
                {
                    action.OnAdding(Graph!);
                    if (!Actions.ContainsKey(action.Name))
                        Actions.Add(action.Name, action!);
                }
            }
        }

        public override void OnRestoring(RpgGraph graph, RpgObject? entity)
        {
            base.OnRestoring(graph, entity);

            foreach (var action in Actions.Values)
                action.OnAdding(Graph);
        }
    }
}


