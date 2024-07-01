using Newtonsoft.Json;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Mods
{
    public class Mod : PropRef
    {
        [JsonProperty] public string Id { get; protected set; }
        [JsonProperty] public string? OwnerId { get; internal set; }
        [JsonProperty] public string Name { get; protected set; }

        [JsonProperty] public PropRef? SourcePropRef { get; protected set; }
        [JsonProperty] public Dice? SourceValue { get; protected set; }
        [JsonProperty] internal ModValueMethod SourceValueFunc { get; private set; } = new ModValueMethod();

        [JsonProperty] public BaseBehavior Behavior { get; protected set; }
        [JsonProperty] public ILifecycle Lifecycle { get; protected set; }
        [JsonProperty] public bool IsBaseInitMod { get => Behavior.Type == ModType.Initial; }
        [JsonProperty] public bool IsBaseOverrideMod { get => Behavior.Type == ModType.Override; }
        [JsonProperty] public bool IsBaseMod { get => Behavior.Type == ModType.Base; }

        public LifecycleExpiry Expiry { get => (int)Lifecycle.Expiry > (int)Behavior.Expiry ? Lifecycle.Expiry : Behavior.Expiry; }

        [JsonConstructor] protected Mod() { }

        internal Mod(string name, ModTemplate template)
        {
            Id = this.NewId();
            Name = name;
            EntityId = template.TargetPropRef.EntityId;
            Prop = template.TargetPropRef.Prop;

            Behavior = template.Behavior;
            Lifecycle = template.Lifecycle;

            SourcePropRef = template.SourcePropRef;
            SourceValue = template.SourceValue;
            SourceValueFunc = template.SourceValueFunc; 
        }

        internal Mod(string ownerId, string name, ModTemplate template)
            : this(name, template)
        {
            OwnerId = ownerId;
        }

        public void OnAdding(RpgGraph graph, Prop modProp, Time.TimePoint time)
        {
            Lifecycle.OnStartLifecycle(graph, time, this);
            Behavior.OnAdding(graph, modProp, this);
        }

        public void OnUpdating(RpgGraph graph, Prop modProp, Time.TimePoint time)
        {
            Lifecycle.OnUpdateLifecycle(graph, time, this);
            Behavior.OnUpdating(graph, modProp, this);
        }

        public void OnRemoving(RpgGraph graph, Prop modProp, Mod? mod = null)
        {
            Behavior.OnRemoving(graph, modProp, this);
        }

        public override string ToString()
        {
            var src = $"{SourcePropRef}{SourceValue}";
            src = SourceValueFunc.IsCalc
                ? $"{SourceValueFunc.MethodName}({src})"
                : src;

            var mod = $"({Behavior.Type}) {EntityId}.{Prop} <= {src}";
            return mod;
        }

        public void SetSource(Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        {
            SourceValue = value;
            SourcePropRef = null;
            SourceValueFunc.Set(valueFunc);
        }
    }
}
