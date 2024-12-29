using System.Linq.Expressions;
using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods.Behaviors;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Mods
{
    public class Mod : Lifespan
    {
        [JsonProperty] public ModType ModType { get; private set; } = ModType.Standard;
        [JsonProperty] public IModBehavior ModBehavior { get; private set; } = new Standard();
        [JsonProperty] public string? Name { get; internal set; }
        [JsonProperty] public RpgPropertyRef Target { get; private set; }
        [JsonProperty] public RpgPropertyRefValue Source { get; internal set; }
        //[JsonProperty] internal RpgMethod<RpgObject, Dice>? SourceValueFunc { get; set; }

        public bool IsApplied { get; set; } = true;
        public bool IsDisabled {  get; set; }

        [JsonConstructor] protected Mod() 
            : base()
        { }

        public Mod(ModType modType)
            : base()
                => ModType = modType;

        public Mod SetName(string name)
        {
            Name = name;
            return this;
        }

        public Mod Behavior(IModBehavior behavior)
        {
            ModBehavior = behavior;
            return this;
        }

        public Mod Lifespan(int duration)
        {
            Start = new TimePoint(TimePointType.Turn, 0);
            End = new TimePoint(TimePointType.Turn, duration);

            return this;
        }

        public Mod Lifespan(int startsIn, int duration)
        {
            Start = new TimePoint(TimePointType.Turn, startsIn);
            End = new TimePoint(TimePointType.Turn, startsIn + duration);

            return this;
        }

        public Mod Lifespan(TimePoint start, TimePoint end)
        {
            Start = start;
            End = end;

            return this;
        }

        public Mod SetOwner(string ownerId, bool syncToOwner)
        {
            OwnerId = ownerId;
            SyncToOwner = syncToOwner;
            return this;
        }

        public Mod SetApply(bool apply)
        {
            IsApplied = apply;
            return this;
        }

        public Mod SetDisabled(bool disabled)
        {
            IsDisabled = disabled;
            return this;
        }

        public Mod SetTarget(RpgPropertyRef target)
        {
            Target = target;
            return this;
        }

        public Mod SetTarget<TTarget>(TTarget target, string targetProp)
            where TTarget : RpgObject
        {
            Target = target.PropertyRef(targetProp)!;
            return this;
        }

        public Mod SetTarget<TTarget, TTargetVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr)
            where TTarget : RpgObject
        {
            Target = target.PropertyRef(targetExpr)!;
            return this;
        }

        public Mod SetSource(Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        {
            Source = new RpgPropertyRefValue(null, dice);
            return this;
        }

        public Mod SetSource(RpgPropertyRefValue source, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        {
            Source = source;
            
            return this;
        }

        public Mod SetSource<TSource, TSourceVal>(TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
        {
            Source = source.PropertyRefValue(sourceExpr);
            return this;
        }
    }
}
