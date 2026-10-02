using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Time;
using System.Linq.Expressions;

namespace Rpg.Experimental.Mods
{
    public class Mod : RpgLifecycleObject
    {
        [JsonProperty] public ModType Type { get; private set; } = ModType.Standard;
        [JsonProperty] public ModBehavior Behavior { get; private set; } = ModBehavior.Standard;
        [JsonProperty] public int Version { get; private set; }

        [JsonProperty] public string? Name { get; internal set; }
        [JsonProperty] public RpgPropertyRef? Target { get; private set; }
        [JsonProperty] public ModSource? Source { get; private set; }

        /// <summary>
        /// A mod that lasts a number of turns starts turn tracking when it is applied outside of it. Set this
        /// to false for trivial effects that are not worth counting turns for. They are dropped instead.
        /// </summary>
        [JsonProperty] public bool StartsTurnTracking { get; private set; } = true;

        /// <summary>
        /// The mod was added by hand, not by the rules. See RpgGraph.OverrideByHand() and AdjustByHand().
        /// </summary>
        [JsonProperty] public bool IsManual { get; private set; }

        [JsonConstructor] protected Mod() 
            : base()
        { }

        public Mod(ModType modType, ModBehavior behavior)
            : base()
        {
            Type = modType;
            Behavior = behavior;
        }

        public override void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            base.OnCreating(graph, obj);
            OnCreatingVersion(graph);
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            if (Source?.PropRef?.ObjectId != null)
                graph.OnTimeEvent(Source.PropRef.ObjectId);

            base.OnTimeEvent(graph);
        }

        public Mod SetName(string name)
        {
            Name = name;
            return this;
        }

        public Mod Lifespan(int duration, bool isApplied = true)
        {
            Start = new TimePoint(TimePointType.Turn, 0);
            End = new TimePoint(TimePointType.Turn, duration);
            IsLifespanRelative = true;
            IsApplied = isApplied;

            return this;
        }

        public Mod Lifespan(int startsIn, int duration, bool isApplied = true)
        {
            Start = new TimePoint(TimePointType.Turn, startsIn);
            End = new TimePoint(TimePointType.Turn, startsIn + duration);
            IsLifespanRelative = true;
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

        /// <summary>
        /// The mod lasts until the named time event next happens, e.g. "Sunrise"
        /// </summary>
        public Mod Until(string eventName)
        {
            End = TimePoint.AtEvent(eventName);
            IsLifespanRelative = Start.Type == TimePointType.Turn && IsLifespanRelative;

            return this;
        }

        /// <summary>
        /// This mod is too trivial to start turn tracking for. See StartsTurnTracking.
        /// </summary>
        public Mod NoTurnTracking()
        {
            StartsTurnTracking = false;
            return this;
        }

        /// <summary>
        /// Mark the mod as a change made by hand
        /// </summary>
        public Mod Manual()
        {
            IsManual = true;
            return this;
        }

        public Mod SetVersion(int version)
        {
            Version = version;
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

        public Mod SetUserEnabled(bool? enabled)
        {
            IsUserEnabled = enabled;
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

        private void OnCreatingVersion(RpgGraph graph)
        {
            if (Behavior == ModBehavior.Replace)
            {
                var propData = graph.GetPropertyData<RpgPropertyDataModdable>(Target!.ObjectId, Target!.Path);
                var versions = propData?
                    .Mods
                    .Where(x => x.Type == Type)
                    .Select(x => x.Version) ?? [];

                Version = versions.Any() ? versions.Max() + 1 : 0;
            }
        }
    }
}
