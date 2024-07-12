using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Time.Lifecycles;

namespace Rpg.ModObjects
{
    public abstract class RpgEntity : RpgObject
    {
        public Dictionary<string, State> States { get; init; }
        //[JsonProperty] internal StateStore StateStore { get; private set; }
        [JsonProperty] internal ActionStore ActionStore { get; private set; }

        [JsonConstructor] protected RpgEntity()
            : base() 
        {
            States = new Dictionary<string, State>();
            //StateStore = new StateStore(Id);
            ActionStore = new ActionStore(Id);
        }

        public RpgEntity(string name)
            : this()
        {
            Name = name;
        }

        public State? GetState(string state)
            => States.ContainsKey(state) ? States[state] : null;

        public State? GetStateById(string id)
            => States.Values.FirstOrDefault(x => x.Id == id);

        public bool IsStateOn(string state)
            => GetState(state)?.IsOn ?? false;

        public bool SetStateOn(string state)
            => GetState(state)?.On() ?? false;

        public bool SetStateOff(string state)
            => GetState(state)?.Off() ?? false;

        public ModSet[] GetActiveConditionalStateInstances(string state)
            => Graph!.GetModSets(this, (x) => x.Name == state && x.Lifecycle is SyncedLifecycle && x.Expiry == LifecycleExpiry.Active);

        public ModSet[] GetActiveManualStateInstances(string state)
            => Graph!.GetModSets(this, (x) => x.Name == state && !(x.Lifecycle is SyncedLifecycle) && x.Expiry == LifecycleExpiry.Active);

        public ModSet CreateStateInstance(string state, ILifecycle? lifecycle = null)
            => GetState(state)!.CreateInstance(lifecycle ?? new TurnLifecycle());

        public Actions.Action? GetAction(string action)
            => ActionStore[action];

        public Actions.Action[] Actions { get => ActionStore.Get(); }

        public ActionInstance? CreateActionInstance(RpgEntity initiator, string actionName, int actionNo)
        {
            var action = GetAction(actionName);
            if (action != null)
            {
                var instance = new ActionInstance(this, initiator, action, actionNo);
                instance.OnBeforeTime(Graph!);

                return instance;
            }

            return null;
        }

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);
            foreach (var state in States.Values)
                state.OnAdding(graph);

            ActionStore.OnBeforeTime(graph, entity);
        }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            OnStateStartLifecycle(graph, currentTime);
            var expiry = base.OnStartLifecycle(graph, currentTime);
            ActionStore.OnStartLifecycle(graph, currentTime);

            return expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            OnStateUpdateLifecycle(graph, currentTime);
            var expiry = base.OnUpdateLifecycle(graph, currentTime);
            ActionStore.OnUpdateLifecycle(graph, currentTime);

            return expiry;
        }

        private void OnStateStartLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            foreach (var state in States.Values)
            {
                var stateExpiry = state.Lifecycle.OnStartLifecycle(graph, currentTime);
                if (stateExpiry == LifecycleExpiry.Active)
                {
                    var stateSets = GetActiveConditionalStateInstances(state.Name);
                    if (!stateSets.Any())
                    {
                        var stateModSet = state.CreateInstance();
                        AddModSet(stateModSet);
                    }
                }
            }
        }

        public void OnStateUpdateLifecycle(RpgGraph graph, TimePoint time)
        {
            base.OnUpdateLifecycle(graph, time);

            foreach (var state in States.Values)
            {
                var stateExpiry = state.Lifecycle.OnUpdateLifecycle(graph, time);
                var stateSets = GetActiveConditionalStateInstances(state.Name);

                if (stateExpiry == LifecycleExpiry.Active)
                {
                    if (!stateSets.Any())
                    {
                        var stateModSet = state.CreateInstance();
                        AddModSet(stateModSet);
                    }
                }
                else
                {
                    foreach (var modSet in stateSets)
                        graph.RemoveModSet(modSet.Id);
                }
            }
        }
    }
}
