//using Newtonsoft.Json;
//using Rpg.ModObjects.Mods;
//using Rpg.ModObjects.States;
//using Rpg.ModObjects.Time;

//namespace Rpg.ModObjects.Lifecycles
//{
//    public class SyncedLifecycle : BaseLifecycle
//    {
//        [JsonProperty] public string OwnerId { get; private set; }

//        [JsonConstructor] private SyncedLifecycle() { }

//        public SyncedLifecycle(string ownerId)
//        {
//            OwnerId = ownerId;
//        }

//        public override LifecycleExpiry OnStartLifecycle()
//        {
//            var state = Graph.Locate<State>(OwnerId);
//            if (state != null)
//            {
//                Expiry = state.Lifecycle.OnStartLifecycle();
//                return Expiry;
//            }

//            var modSet = Graph.Locate<ModSet>(OwnerId);
//            if (modSet != null)
//            {
//                Expiry = modSet.Lifecycle.OnStartLifecycle();
//                return Expiry;
//            }

//            var ownerMod = Graph.Locate<Mod>(OwnerId);
//            if (ownerMod != null)
//            {
//                Expiry = ownerMod.OnStartLifecycle();
//                return Expiry;
//            }

//            return base.OnStartLifecycle();
//        }

//        public override LifecycleExpiry OnUpdateLifecycle()
//        {
//            var state = Graph.Locate<State>(OwnerId);
//            if (state != null)
//            {
//                Expiry = state.Lifecycle.OnUpdateLifecycle();
//                return Expiry;
//            }

//            var modSet = Graph.Locate<ModSet>(OwnerId);
//            if (modSet != null)
//            {
//                Expiry = modSet.Lifecycle.OnUpdateLifecycle();
//                return Expiry;
//            }

//            var ownerMod = Graph.Locate<Mod>(OwnerId);
//            if (ownerMod != null)
//            {
//                Expiry = ownerMod.OnUpdateLifecycle();
//                return Expiry;
//            }

//            return base.OnUpdateLifecycle();
//        }
//    }
//}
