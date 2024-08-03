using Newtonsoft.Json;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
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
        [JsonProperty] internal RpgMethod<RpgObject, Dice>? SourceValueFunc { get; private set; }

        [JsonProperty] public BaseBehavior Behavior { get; protected set; }
        [JsonProperty] public ILifecycle Lifecycle { get; protected set; }
        [JsonIgnore] public bool IsBaseInitMod { get => Behavior.Type == ModType.Initial; }
        [JsonIgnore] public bool IsBaseOverrideMod { get => Behavior.Type == ModType.Override; }
        [JsonIgnore] public bool IsBaseMod { get => Behavior.Type == ModType.Base; }

        [JsonProperty] public bool IsApplied { get; private set; } = true;
        [JsonProperty] public bool IsDisabled { get; private set; }
        public bool IsActive { get => Expiry == LifecycleExpiry.Active && IsApplied && !IsDisabled; }

        [JsonIgnore] public LifecycleExpiry Expiry { get => (int)Lifecycle.Expiry > (int)Behavior.Expiry ? Lifecycle.Expiry : Behavior.Expiry; }

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

        public void Apply()
            => IsApplied = true;

        public void Unapply()
            => IsApplied = false;

        public void Enable()
            => IsDisabled = false;

        public void Disable()
            => IsDisabled = true;

        public void OnAdding(RpgGraph graph, Prop modProp, Time.TimePoint time)
        {
            Lifecycle.OnStartLifecycle(graph, time);
            Behavior.OnAdding(graph, modProp, this);
        }

        public void OnUpdating(RpgGraph graph, Prop modProp, Time.TimePoint time)
        {
            Lifecycle.OnUpdateLifecycle(graph, time);
            Behavior.OnUpdating(graph, modProp, this);
        }

        public void OnRemoving(RpgGraph graph, Prop modProp)
        {
            Behavior.OnRemoving(graph, modProp, this);
        }

        public override string ToString()
        {
            var src = $"{SourcePropRef}{SourceValue}";
            src = SourceValueFunc != null
                ? $"{SourceValueFunc.MethodName}({src})"
                : src;

            var mod = $"({Behavior.Type}) {EntityId}.{Prop} <= {src}";
            return mod;
        }

        public void SetSource(Dice? value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        {
            SourceValue = value;
            SourcePropRef = null;
            if (value != null && valueFunc != null)
                SourceValueFunc = RpgMethod.Create<RpgObject, Dice, Dice>(valueFunc);
        }
    }
}
