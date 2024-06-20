using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.States
{
    public abstract class StateModification<T> : Modification
        where T : RpgObject
    {
        [JsonProperty] private bool? ForcedOn { get; set; }
        [JsonProperty] private bool IsOn { get; set; }

        public StateModification(T owner)
        {
            AddOwner(owner);
            Lifecycle = new ConditionalLifecycle(CalculateExpiry);
            WhenOn(owner);
        }

        public bool On()
        {
            ForcedOn = true;
            return true;
        }

        public bool Off()
        {
            ForcedOn = false;
            return true;
        }

        public void Release() => ForcedOn = null;

        protected abstract bool IsOnWhen(T owner);

        protected abstract void WhenOn(T owner);

        public StateModification<T> Mod<TTargetValue>(T entity, Expression<Func<T, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        {
            var mod = new SyncedMod(OwnerId!, OwnerArchetype!)
                .SetProps(entity, targetExpr, dice, valueFunc)
                .Create();

            AddMods(mod);

            return this;
        }

        public StateModification<T> Mod<TTargetValue, TSourceValue>(T entity, Expression<Func<T, TTargetValue>> targetExpr, Expression<Func<T, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        {
            var mod = new SyncedMod(OwnerId!, OwnerArchetype!)
                .SetProps(entity, targetExpr, entity, sourceExpr, valueFunc)
                .Create();

            AddMods(mod);

            return this;
        }

        private LifecycleExpiry CalculateExpiry()
        {
            LifecycleExpiry expiry;

            if (ForcedOn == false)
                expiry = LifecycleExpiry.Expired;

            else if (ForcedOn == true)
                expiry = LifecycleExpiry.Active;

            else
            {
                var obj = Graph!.GetEntity<T>(RecipientId)!;
                expiry = IsOnWhen(obj)
                    ? LifecycleExpiry.Active
                    : LifecycleExpiry.Expired;

                //Should we do the following here? Won't the engine ensure that the mods are added later on?
                if (expiry == LifecycleExpiry.Active && !IsOn)
                {
                    IsOn = true;

                    var entity = Graph!.GetEntity<T>(OwnerId)!;
                    Graph.AddMods(Mods.ToArray());
                }
                else if (expiry != LifecycleExpiry.Active && IsOn)
                {
                    Graph!.RemoveMods(Mods.ToArray());
                }
            }

            return expiry;
        }
    }
}
