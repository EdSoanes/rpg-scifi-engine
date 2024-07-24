using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Mods.Templates
{
    public class ModTemplate
    {
        public string? Name { get; protected set; }
        public BaseBehavior Behavior { get; protected set; }
        public ILifecycle Lifecycle { get; protected set; }
        public PropRef TargetPropRef { get; private set; }
        public PropRef? SourcePropRef { get; private set; }
        public Dice? SourceValue { get; private set; }
        internal RpgMethod<RpgObject, Dice>? SourceValueFunc { get; private set; }

        public virtual Mod Create(string name)
        {
            var mod = new Mod(name, this);
            return mod;
        }

        public virtual Mod Create()
            => Create(Name ?? TargetPropRef.Prop);

        public ModTemplate SetName(string name)
        {
            Name = name;
            return this;
        }

        public ModTemplate SetBehavior(BaseBehavior behavior)
        {
            Behavior = behavior;
            return this;
        }

        public ModTemplate SetScope(ModScope scope)
        {
            if (Behavior != null)
                Behavior.Scope = scope;

            return this;
        }

        public ModTemplate SetLifecycle(ILifecycle lifecycle)
        {
            Lifecycle = lifecycle;
            return this;
        }

        public ModTemplate SetProps(string targetId, string prop, Dice value)
        {
            TargetPropRef = new PropRef(targetId, prop);
            SourceValue = value;

            return this;
        }

        public ModTemplate SetProps(PropRef targetPropRef, Mod mod)
        {
            TargetPropRef = targetPropRef;
            SourcePropRef = mod.SourcePropRef != null
                ? new PropRef(mod.SourcePropRef.EntityId, mod.SourcePropRef.Prop)
                : null;

            SourceValue = mod.SourceValue;
            SourceValueFunc = mod.SourceValueFunc;

            return this;
        }

        public ModTemplate SetProps<TTarget, TSourceValue>(TTarget target, string targetProp, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            TargetPropRef = PropRef.CreatePropRef(target, targetProp);
            SourcePropRef = PropRef.CreatePropRef(target, sourceExpr);
            if (valueFunc != null)
                SourceValueFunc = RpgMethodFactory.Create<RpgObject, Dice, Dice>(valueFunc);

            return this;
        }

        public ModTemplate SetProps<TTarget>(TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            TargetPropRef = PropRef.CreatePropRef(target, targetProp);
            SourceValue = value;
            if (valueFunc != null)
                SourceValueFunc = RpgMethodFactory.Create<RpgObject, Dice, Dice>(valueFunc);

            return this;
        }

        public ModTemplate SetProps<TTarget, TTargetVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            TargetPropRef = PropRef.CreatePropRef(target, targetExpr);
            SourceValue = value;
            if (valueFunc != null)
                SourceValueFunc = RpgMethodFactory.Create<RpgObject, Dice, Dice>(valueFunc);

            return this;
        }

        public ModTemplate SetProps<TTarget, TTargetVal, TSource, TSourceVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            TargetPropRef = PropRef.CreatePropRef(target, targetExpr);
            SourcePropRef = PropRef.CreatePropRef(source, sourceExpr);
            if (valueFunc != null)
                SourceValueFunc = RpgMethodFactory.Create<RpgObject, Dice, Dice>(valueFunc);

            return this;
        }

        public ModTemplate SetProps<TTarget, TSource, TSourceVal>(TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            TargetPropRef = PropRef.CreatePropRef(target, targetProp);
            SourcePropRef = PropRef.CreatePropRef(source, sourceExpr);

            return this;
        }
    }
}
