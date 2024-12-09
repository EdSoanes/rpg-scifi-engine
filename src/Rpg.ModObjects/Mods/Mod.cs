using Newtonsoft.Json;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Mods
{
    public abstract class Mod : RpgLifecycleObject
    {
        [JsonProperty] public string Id { get; init; }
        [JsonProperty] public string? OwnerId { get; internal set; }
        [JsonIgnore] public string EntityId { get => Target.EntityId; }
        [JsonIgnore] public string Prop { get => Target.Prop; }
        [JsonProperty] public string Name { get; internal set; }
        [JsonProperty] public BaseBehavior Behavior { get; protected set; }

        [JsonProperty] public PropRef Target { get; protected set; }

        [JsonProperty] public PropRef? Source { get; internal set; }
        [JsonProperty] public Dice? SourceValue { get; internal set; }
        [JsonProperty] internal RpgMethod<RpgObject, Dice>? SourceValueFunc { get; set; }

        [JsonConstructor] protected Mod() 
        {
            Id = this.NewId();
            Behavior = new Add();
        }

        protected Mod(string name)
            : this()
                => Name = name;

        public override void SetExpired()
        {
            base.SetExpired();
            if (Graph != null)
                foreach (var rpgObj in Graph.GetObjects())
                    foreach (var mod in ModFilters.SyncedToOwner(rpgObj.GetMods(), Id))
                        mod.SetExpired();
        }

        public void Apply()
            => IsApplied = true;

        public void Unapply()
            => IsApplied = false;

        public void Enable()
            => IsDisabled = false;

        public void Disable()
            => IsDisabled = true;

        public Dice? Value()
        {
            if (Graph == null)
                return null;

            Dice? value = SourceValue ?? Graph.GetObject(Source!.EntityId)?.Value(Source.Prop);

            if (value != null && SourceValueFunc != null)
            {
                var args = new Dictionary<string, object?>();
                args.Add(SourceValueFunc.Args.First().Name, value);

                var entity = Graph.GetObject(SourceValueFunc.EntityId);
                value = entity != null
                    ? SourceValueFunc.Execute(entity, args)
                    : SourceValueFunc.Execute(args);
            }

            return value;
        }

        //public ModDescription? Describe()
        //{
        //    var value = Value();
        //    var modDesc = new ModDescription
        //    {
        //        ModType = GetType().Name,
        //        Behavior = Behavior.GetType().Name,
        //        Value = value ?? Dice.Zero,
        //        ValueFunction = SourceValueFunc?.FullName,
        //        SourceValue = SourceValue,
        //    };

        //    var sourceEntity = Graph.GetObject(Source?.EntityId);
        //    if (sourceEntity != null)
        //        modDesc.SourceProp = sourceEntity.Describe(Source!.Prop);

        //    return modDesc;
        //}

        public override LifecycleExpiry OnStartLifecycle()
        {
            var oldExpiry = Expiry;
            base.OnStartLifecycle();
            if (oldExpiry != Expiry)
                Graph.OnPropUpdated(Target);

            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            var oldExpiry = Expiry;
            base.OnUpdateLifecycle();
            Behavior.OnUpdating(Graph, this);
            if (oldExpiry != Expiry)
                Graph.OnPropUpdated(Target);

            return Expiry;
        }

        public override string ToString()
        {
            var src = $"{Source}{SourceValue}";
            src = SourceValueFunc != null
                ? $"{SourceValueFunc.MethodName}({src})"
                : src;

            var mod = $"({this.GetType().Name}) {EntityId}.{Prop} <= {src}";
            return mod;
        }

        public Mod SetName(string name)
        {
            Name = name;
            return this;
        }

        public Mod Set(Dice? value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        {
            SourceValue = value;
            Source = null;
            if (value != null && valueFunc != null)
                SourceValueFunc = RpgMethodFactory.Create<RpgObject, Dice, Dice>(valueFunc);

            return this;
        }

        public Mod Set(PropRef targetPropRef, PropRef sourcePropRef, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        {
            Target = targetPropRef;
            Source = sourcePropRef;
            if (valueFunc != null)
                SourceValueFunc = RpgMethodFactory.Create<RpgObject, Dice, Dice>(valueFunc);

            if (string.IsNullOrEmpty(Name))
                Name = targetPropRef.Prop;

            return this;
        }

        public Mod Set(PropRef targetPropRef, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
        {
            Target = targetPropRef;
            SourceValue = dice;
            if (valueFunc != null)
                SourceValueFunc = RpgMethodFactory.Create<RpgObject, Dice, Dice>(valueFunc);

            if (string.IsNullOrEmpty(Name))
                Name = targetPropRef.Prop;

            return this;
        }

        public Mod Set(PropRef targetPropRef, Mod mod)
        {
            Target = targetPropRef;
            SourceValue = mod.SourceValue;
            Source = mod.Source;
            SourceValueFunc = mod.SourceValueFunc;
            Name = mod.Name;
            return this;
        }

        public Mod Set<TTarget>(TTarget target, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
        {
            var targetPropRef = PropRef.CreatePropRef(target, targetProp);
            return Set(targetPropRef, dice, valueCalc);
        }

        public Mod Set<TTarget, TTargetVal, TSource, TSourceVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            var targetPropRef = PropRef.CreatePropRef(target, targetExpr);
            var sourcePropRef = PropRef.CreatePropRef(source, sourceExpr);
            return Set(targetPropRef, sourcePropRef, valueFunc);
        }

        public Mod Set<TTarget, TSourceValue>(TTarget target, string targetProp, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var targetPropRef = PropRef.CreatePropRef(target, targetProp);
            var sourcePropRef = PropRef.CreatePropRef(target, sourceExpr);
            return Set(targetPropRef, sourcePropRef, valueFunc);
        }

        public Mod Set<TTarget, TSource, TSourceValue>(TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var targetPropRef = PropRef.CreatePropRef(target, targetProp);
            var sourcePropRef = PropRef.CreatePropRef(source, sourceExpr);
            return Set(targetPropRef, sourcePropRef, valueFunc);
        }

        public Mod Set<TTarget, TSource>(TTarget target, string targetProp, TSource source, string sourceProp, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var targetPropRef = PropRef.CreatePropRef(target, targetProp);
            var sourcePropRef = PropRef.CreatePropRef(source, sourceProp);
            return Set(targetPropRef, sourcePropRef, valueFunc);
        }

        public Mod Set<TTarget, TTargetValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
        {
            var targetPropRef = PropRef.CreatePropRef(target, targetExpr);
            return Set(targetPropRef, dice, valueCalc);
        }
    }
}
