using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.States
{
    public abstract class StateModification<T> : Modification, ILifecycle
        where T : RpgObject
    {
        [JsonProperty] public TimePoint? ExpiredTime { get; protected set; }
        [JsonProperty] public ModExpiry Expiry { get; protected set; }

        [JsonProperty] private bool? ForcedOn { get; set; }

        public void On() => ForcedOn = true;
        public void Off() => ForcedOn = false;
        public void Release() => ForcedOn = null;

        public StateModification()
        {
            Name = GetType().Name;
            Lifecycle = this;
        }

        protected abstract bool IsOnWhen(T recipientObjId);

        protected abstract bool WhenOn(T recipientObjId);

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
            if (ForcedOn == false)
                Expiry = ModExpiry.Expired;

            else if (ForcedOn == true)
                Expiry = ModExpiry.Active;

            else
            {
                var obj = Graph!.GetEntity<T>(RecipientObjId)!;
                Expiry = IsOnWhen(obj)
                    ? ModExpiry.Active
                    : ModExpiry.Expired;
            }

            if (Expiry != ModExpiry.Active)
            {
                Graph!.RemoveMods(Mods.ToArray());
                Mods.Clear();
            }
            else
            {
                var entity = Graph!.GetEntity<T>(RecipientObjId)!;
                WhenOn(entity);
                Graph.AddMods(Mods.ToArray());
            }

            return Expiry;
        }
    }
}
