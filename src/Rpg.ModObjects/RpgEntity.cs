using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;
using System.Collections.Specialized;

namespace Rpg.ModObjects
{
    public class RpgEntity : RpgObject, INotifyCollectionChanged
    {
        [JsonProperty] internal StateStore StateStore { get; private set; }
        [JsonProperty] internal ActionStore ActionStore { get; private set; }

        public RpgEntity()
            : base() 
        {
            StateStore = new StateStore(Id);
            ActionStore = new ActionStore(Id);
        }

        public RpgEntity(string name)
            : this()
        {
            Name = name;
        }

        public bool IsStateOn(string state)
            => (StateStore[state]?.Expiry ?? LifecycleExpiry.Expired) == LifecycleExpiry.Active;

        public bool SetStateOn(string state)
            => StateStore[state]?.On() ?? false;

        public bool SetStateOff(string state)
            => StateStore[state]?.Off() ?? false;

        public State? GetState(string state)
            => StateStore[state];

        public State[] GetStates()
            => StateStore.Get();


        public bool IsActionEnabled<TOwner>(string action, RpgEntity initiator)
            where TOwner : RpgEntity
                => GetAction(action)?.IsEnabled<TOwner>((this as TOwner)!, initiator) ?? false;

        public Actions.Action? GetAction(string action)
            => ActionStore[action];

        public Actions.Action[] GetActions()
            => ActionStore.Get();

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);

            ActionStore.OnBeforeTime(graph, entity);
            StateStore.OnBeforeTime(graph, entity);
        }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mods.Mod? mod = null)
        {
            StateStore.OnStartLifecycle(graph, currentTime);
            var expiry = base.OnStartLifecycle(graph, currentTime, mod);
            ActionStore.OnStartLifecycle(graph, currentTime);

            return expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mods.Mod? mod = null)
        {
            StateStore.OnUpdateLifecycle(graph, currentTime);
            var expiry = base.OnUpdateLifecycle(graph, currentTime, mod);
            ActionStore.OnUpdateLifecycle(graph, currentTime);

            return expiry;
        }
    }
}
