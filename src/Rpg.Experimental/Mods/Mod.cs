using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods.Behaviors;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Time;
using System.Linq.Expressions;

namespace Rpg.Experimental.Mods
{
    public class Mod : Lifespan
    {
        [JsonProperty] public ModType ModType { get; private set; } = ModType.Standard;
        [JsonProperty] public IModBehavior ModBehavior { get; private set; } = new Standard();
        [JsonProperty] public string? Name { get; internal set; }
        [JsonProperty] public RpgPropertyRef? Target { get; private set; }
        [JsonProperty] public ModSource? Source { get; private set; }

        [JsonConstructor] protected Mod() 
            : base()
        { }

        public Mod(ModType modType)
            : base()
        {
            ModType = modType;
            if (modType == ModType.Initial || modType == ModType.Base)
                ModBehavior = new Replace();
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            if (Source?.PropRef?.ObjectId != null)
                graph.OnTimeEvent(Source.PropRef.ObjectId);

            ModBehavior.OnBeforeTimeEvent(this, graph);
            base.OnTimeEvent(graph);
            ModBehavior.OnAfterTimeEvent(this, graph);
        }

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

        public Mod Lifespan(int duration, bool isApplied = true)
        {
            Start = new TimePoint(TimePointType.Turn, 0);
            End = new TimePoint(TimePointType.Turn, duration);
            IsApplied = isApplied;

            return this;
        }

        public Mod Lifespan(int startsIn, int duration, bool isApplied = true)
        {
            Start = new TimePoint(TimePointType.Turn, startsIn);
            End = new TimePoint(TimePointType.Turn, startsIn + duration);
            IsApplied = isApplied;

            return this;
        }

        public Mod Lifespan(TimePoint start, TimePoint end, bool isApplied = true)
        {
            Start = start;
            End = end;
            IsApplied = isApplied;

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

        public Mod SetTarget(string objectId, string prop)
            => SetTarget(new RpgPropertyRef(objectId, prop));

        public Mod SetTarget(RpgPropertyRef? target)
        {
            Target = target;
            return this;
        }

        public Mod SetTarget<TTarget>(TTarget target, string targetProp)
            where TTarget : RpgObject
        {
            Target = new RpgPropertyRef(target.Id, targetProp);
            return this;
        }

        public Mod SetTarget<TTarget, TTargetVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr)
            where TTarget : RpgObject
        {
            Target = new RpgPropertyRef(target.Id, RpgMemberUtilities.ExpressionToPath(targetExpr));
            return this;
        }

        public Mod SetSource(Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            => SetSource(new ModSource(dice, valueCalc));

        public Mod SetSource(RpgPropertyRef propRef, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            => SetSource(new ModSource(propRef, valueCalc));

        public Mod SetSource(ModSource? source)
        {
            Source = source;   
            return this;
        }

        public Mod SetSource<TSource, TSourceVal>(TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            => SetSource(new RpgPropertyRef(source.Id, RpgMemberUtilities.ExpressionToPath(sourceExpr)), valueFunc);

        public override string ToString()
        {
            return $"{base.ToString()} = {Source?.ToString()}";
        }
    }
}
