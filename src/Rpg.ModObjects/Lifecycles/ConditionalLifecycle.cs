﻿//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using Rpg.ModObjects.Mods;
//using Rpg.ModObjects.Reflection;
//using Rpg.ModObjects.Time;

//namespace Rpg.ModObjects.Lifecycles
//{
//    public class ConditionalLifecycle<TOwner> : BaseLifecycle
//        where TOwner : class
//    {
//        [JsonProperty] public string OwnerId { get; private set; }
//        [JsonProperty] public RpgMethod<TOwner, LifecycleExpiry> OnStartConditional { get; private set; }
//        [JsonProperty] public RpgMethod<TOwner, LifecycleExpiry> OnUpdateConditional { get; private set; }

//        [JsonConstructor] private ConditionalLifecycle() { }

//        public ConditionalLifecycle(
//            string ownerId,
//            RpgMethod<TOwner, LifecycleExpiry> onStartConditional,
//            RpgMethod<TOwner, LifecycleExpiry> onUpdateConditional)
//        {
//            OwnerId = ownerId;
//            OnStartConditional = onStartConditional;
//            OnUpdateConditional = onUpdateConditional;
//        }

//        public ConditionalLifecycle(string entityId, RpgMethod<TOwner, LifecycleExpiry> conditional)
//            : this(entityId, conditional, conditional)
//        { }

//        public override void OnRestoring(RpgGraph graph)
//        {
//            base.OnRestoring(graph);

//        }
//        public override LifecycleExpiry OnStartLifecycle()
//        {
//            var owner = Graph.Locate<TOwner>(OwnerId);
//            if (owner == null)
//                throw new InvalidOperationException($"{nameof(ConditionalLifecycle<TOwner>)}.{nameof(GetOwner)} could not find owner");

//            var args = new Dictionary<string, object?>();
//            Expiry = OnStartConditional.Execute(owner!, args);

//            return Expiry;
//        }

//        public override LifecycleExpiry OnUpdateLifecycle()
//        {
//            var owner = Graph.Locate<TOwner>(OwnerId);
//            if (owner == null)
//                throw new InvalidOperationException($"{nameof(ConditionalLifecycle<TOwner>)}.{nameof(GetOwner)} could not find owner");

//            var args = new Dictionary<string, object?>();
//            Expiry = OnUpdateConditional.Execute(owner!, args);

//            return Expiry;
//        }

//        private TOwner GetOwner(RpgGraph graph)
//        {
//            var entity = graph.GetObject(OwnerId)!;
//            if (entity != null && entity.GetType() == typeof(TOwner))
//                return (entity as TOwner)!;

//            var state = graph.GetState(OwnerId);
//            if (state != null && state.GetType() == typeof(TOwner))
//                return (state as TOwner)!;

//            var modSet = graph.GetModSet(OwnerId);
//            if (modSet != null && modSet.GetType() == typeof(TOwner))
//                return (modSet as TOwner)!;

//            throw new InvalidOperationException($"{nameof(ConditionalLifecycle<TOwner>)}.{nameof(GetOwner)} could not find owner");
//        }
//    }
//}
