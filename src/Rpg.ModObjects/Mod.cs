using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects
{
    //TODO: Improved modifier lifecycle. Try to encode Start/End turns, Permanent, Conditional, etc into a more intuitive model.

    public abstract class Mod : ModPropRef
    {
        [JsonConstructor] protected Mod() { }

        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public ModSource Source { get; protected set; }
        [JsonProperty] public ModType ModifierType { get; protected set; }
        [JsonProperty] public ModAction ModifierAction { get; protected set; } = ModAction.Accumulate;
        [JsonProperty] public ModDuration Duration { get; protected set; } = new ModDuration();

        public void SetSource(Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            => Source = new ModSource<ModObject, Dice>(value, diceCalcExpr);
        
        protected static TMod Create<TMod, TTarget, TTargetVal, TSourceVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Mod
            where TTarget : ModObject
        {
            var mod = Activator.CreateInstance<TMod>();

            //Set target
            var targetPropRef = CreatePropRef(target, targetExpr);
            mod.EntityId = targetPropRef.EntityId;
            mod.Prop = targetPropRef.Prop;

            //Set source
            mod.Source = new ModSource<TTarget, TSourceVal>(value, diceCalcExpr);

            return mod;
        }

        protected static TMod Create<TMod, TTarget, TTargetVal, TSource, TSourceVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Mod
            where TSource : ModObject
            where TTarget : ModObject
        {
            var mod = Activator.CreateInstance<TMod>();

            //Set target
            var targetPropRef = CreatePropRef(target, targetExpr);
            mod.EntityId = targetPropRef.EntityId;
            mod.Prop = targetPropRef.Prop;

            //Set source
            mod.Source = new ModSource<TSource, TSourceVal>(source, sourceExpr, diceCalcExpr);

            return mod;
        }

        protected static TMod Create<TMod, TTarget>(TTarget target, string targetPropPath, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TMod : Mod
            where TTarget : ModObject
        {
            var mod = Activator.CreateInstance<TMod>();

            //Set target
            var targetPropRef = CreatePropRef(target, targetPropPath);
            mod.EntityId = targetPropRef.EntityId;
            mod.Prop = targetPropRef.Prop;

            //Set source
            mod.Source = new ModSource<TTarget, Dice>(value, diceCalcExpr);

            return mod;
        }

        public virtual void OnAdd(int turn) { }
        public virtual void OnUpdate(int turn) { }

        public override string ToString()
        {
            var mod = new
            {
                Id = Id,
                Name = Name,
                EntityId = EntityId,
                Prop = Prop,
                Type = ModifierType.ToString(),
                Action = ModifierAction.ToString(),
                Duration = Duration.ToString(),
                Source = Source.ToString()
            };

            return JsonConvert.SerializeObject(mod);
        }
    }
}
