using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rpg.ModObjects.Actions
{
    public abstract class ActionModification<TOwner> : Modification, ILifecycle
        where TOwner : RpgObject
    {
        [JsonProperty] public TimePoint? ExpiredTime { get; protected set; }
        [JsonProperty] public ModExpiry Expiry { get; protected set; }
        [JsonProperty] public RpgActionArg[] OnActivatedArgs { get; private set; } = new RpgActionArg[0];
        [JsonProperty] public RpgActionArg[] OnOutcomeArgs { get; private set; } = new RpgActionArg[0];

        public ActionModification(TOwner owner)
            : this(owner, owner)
        { }

        public ActionModification(TOwner owner, RpgObject initiator)
        {
            AddOwner(owner);
            AddInitiator(initiator);
            Name = GetType().Name;
            Lifecycle = this;

            EnsureOnActivatedMethod();
            EnsureOnOutcomeMethod();
        }

        protected abstract bool IsEnabledWhen(TOwner owner);

        public void SetExpired(TimePoint currentTime)
        {
            if (Expiry == ModExpiry.Active)
                ExpiredTime = new TimePoint(currentTime.Type, currentTime.Tick - 1);
        }

        public ModExpiry StartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
            => CalculateLifecycle();

        public ModExpiry UpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
            => CalculateLifecycle();

        private ModExpiry CalculateLifecycle()
        {
            var owner = Graph!.GetEntity<TOwner>(OwnerObjId)!;
            Expiry = IsEnabledWhen(owner)
                ? ModExpiry.Active
                : ModExpiry.Expired;

            return Expiry;
        }

        private void EnsureOnActivatedMethod()
        {
            var methodInfo = GetType().GetMethod("OnActivated");
            if (methodInfo == null)
                throw new InvalidOperationException("OnActivated() method not found on ActionModification class");

            if (!methodInfo.ReturnType.IsAssignableTo(typeof(Modification)))
                throw new InvalidOperationException("OnActivated() method does not have return type 'Modification'");

            var args = methodInfo.GetParameters()
                .Select(x => new RpgActionArg(x, null))
                .ToArray();

            OnActivatedArgs = args;
        }

        private void EnsureOnOutcomeMethod()
        {
            var methodInfo = GetType().GetMethod("OnOutcome");
            if (methodInfo == null)
                throw new InvalidOperationException("OnOutcome() method not found on ActionModification class");

            if (!methodInfo.ReturnType.IsAssignableTo(typeof(Modification)))
                throw new InvalidOperationException("OnOutcome() method does not have return type 'Modification'");

            var args = methodInfo.GetParameters()
                .Select(x => new RpgActionArg(x, null))
                .ToArray();

            OnOutcomeArgs = args;
        }
    }
}