using System.Linq.Expressions;
using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Mods
{
    public class Mod : Lifespan
    {
        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public ModType ModType { get; private set; } = ModType.Standard;
        [JsonProperty] public string? Name { get; internal set; }
        [JsonProperty] public string? OwnerId { get; private set; }
        [JsonProperty] public ModOwnerType OwnerType { get; private set; } = ModOwnerType.None;
        [JsonProperty] public RpgPropertyRef Target { get; private set; }
        [JsonProperty] public RpgPropertyRefValue Source { get; private set; }
        //[JsonProperty] internal RpgMethod<RpgObject, Dice>? SourceValueFunc { get; set; }

        public bool IsApplied { get; set; } = true;
        public bool IsDisabled {  get; set; }

        [JsonConstructor] protected Mod() 
        {
            Id = this.NewId();
        }

        public Mod(ModType modType)
            : this()
                => ModType = modType;

        public Mod(ModType modType, string ownerId, ModOwnerType ownerType)
            : this(modType)
        {
            OwnerId = ownerId;
            OwnerType = ownerType;
        }

        public override void Expire(TimePoint now, TimePoint expiryTime)
        {
            base.Expire(now, now);
            //if (Graph != null)
            //    foreach (var rpgObj in Graph.GetObjects())
            //        foreach (var mod in ModFilters.SyncedToOwner(rpgObj.GetMods(), Id))
            //            mod.SetExpired();
        }

        public Mod SetName(string name)
        {
            Name = name;
            return this;
        }

        public Mod Set<TTarget>(TTarget target, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
        {
            Target = target.PropertyRef(targetProp)!;
            Source = new RpgPropertyRefValue(null, dice);

            return this;
        }

        public Mod Set<TTarget, TTargetVal, TSource, TSourceVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            Target = target.PropertyRef(targetExpr)!;
            Source = source.PropertyRefValue(sourceExpr);
            
            return this;
        }

        public Mod Set<TTarget, TSourceValue>(TTarget target, string targetProp, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            Target = target.PropertyRef(targetProp)!;
            Source = target.PropertyRefValue(sourceExpr);

            return this;
        }

        public Mod Set<TTarget, TSource, TSourceValue>(TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            Target = target.PropertyRef(targetProp)!;
            Source = source.PropertyRefValue(sourceExpr);

            return this;
        }

        public Mod Set<TTarget, TSource>(TTarget target, string targetProp, TSource source, string sourceProp, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            Target = target.PropertyRef(targetProp)!;
            Source = target.PropertyRefValue(sourceProp);

            return this;
        }

        public Mod Set<TTarget, TTargetValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
        {
            Target = target.PropertyRef(targetExpr)!;
            Source = new RpgPropertyRefValue(null, dice);

            return this;
        }
    }
}
