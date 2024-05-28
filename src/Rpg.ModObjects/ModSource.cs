using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects
{
    public class ModSource
    {
        [JsonProperty] public Guid? EntityId { get; protected set; }
        [JsonProperty] public string? Prop { get; protected set; }
        [JsonProperty] public Dice? Value { get; protected set; }
        public ModSourceValueFunction ValueFunc { get; } = new ModSourceValueFunction();

        [JsonConstructor] protected ModSource() { }

        public ModSource(Dice value)
            => Value = value;

        public ModSource(ModPropRef propRef)
        {
            EntityId = propRef.EntityId;
            Prop = propRef.Prop;
        }

        [JsonIgnore]
        public ModPropRef? PropRef 
        { 
            get => EntityId != null && !string.IsNullOrEmpty(Prop) 
                ? new ModPropRef(EntityId.Value, Prop) 
                : null; 
        }
    }

    public class ModSource<T, TResult> : ModSource
        where T : ModObject
    {
        public ModSource(Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr)
        {
            Value = value;
            ValueFunc.Set(diceCalcExpr);
        }

        public ModSource(T rootEntity, Expression<Func<T, TResult>> expression, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr)
        {
            var propRef = ModPropRef.CreatePropRef(rootEntity, expression);
            EntityId = propRef.EntityId;
            Prop = propRef.Prop;
            ValueFunc.Set(diceCalcExpr);
        }

        public override string ToString()
        {
            var res = $"{PropRef}{Value}";
            return ValueFunc.IsCalc
                ? $"{ValueFunc.FuncName} <= {res}"
                : res;
        }
    }
}
