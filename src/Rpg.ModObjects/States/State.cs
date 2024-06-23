using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.States
{
    public abstract class State : ModSet
    {
        [JsonProperty] public bool? ForcedOn { get; protected set; }
        [JsonProperty] public bool ConditionallyOn { get; protected set; }
        public bool IsOn { get => (ForcedOn != null && ForcedOn.Value) || ConditionallyOn; }

        [JsonConstructor] protected State() { }

        protected State(RpgObject owner)
            : base(owner.Id, owner.Archetype)
        {
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

        protected abstract LifecycleExpiry CalculateExpiry();
    }

    public abstract class State<T> : State
        where T : RpgObject
    {
        [JsonConstructor] protected State() { }

        public State(T owner)
            : base(owner)
        {
            Lifecycle = new ConditionalLifecycle<State<T>>(Id, new RpgMethod<State<T>, LifecycleExpiry>(this, nameof(CalculateExpiry)));
            WhenOn(owner);
        }

        protected abstract bool IsOnWhen(T owner);

        protected abstract void WhenOn(T owner);

        protected override LifecycleExpiry CalculateExpiry()
        {
            LifecycleExpiry expiry;

            if (ForcedOn == false)
                expiry = LifecycleExpiry.Expired;

            else if (ForcedOn == true)
                expiry = LifecycleExpiry.Active;

            else
            {
                var obj = Graph!.GetEntity<T>(OwnerId)!;
                expiry = IsOnWhen(obj)
                    ? LifecycleExpiry.Active
                    : LifecycleExpiry.Expired;

                //Should we do the following here? Won't the engine ensure that the mods are added later on?
                if (expiry == LifecycleExpiry.Active && !ConditionallyOn)
                {
                    ConditionallyOn = true;
                }
                else if (expiry != LifecycleExpiry.Active && ConditionallyOn)
                {
                    ConditionallyOn = false;
                }
            }

            return expiry;
        }

        public State<T> Mod<TTargetValue>(T entity, Expression<Func<T, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        {
            var mod = new SyncedMod(OwnerId!)
                .SetProps(entity, targetExpr, dice, valueFunc)
                .Create();

            AddMods(mod);

            return this;
        }

        public State<T> Mod<TTargetValue, TSourceValue>(T entity, Expression<Func<T, TTargetValue>> targetExpr, Expression<Func<T, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        {
            var mod = new SyncedMod(OwnerId!)
                .SetProps(entity, targetExpr, entity, sourceExpr, valueFunc)
                .Create();

            AddMods(mod);

            return this;
        }
    }
}
