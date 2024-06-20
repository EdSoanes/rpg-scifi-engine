using Rpg.ModObjects.Props;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Mods
{
    public class ModTemplate
    {
        public string? Name { get; protected set; }
        public BaseBehavior Behavior { get; protected set; }
        public ILifecycle Lifecycle { get; protected set; }
        public PropRef TargetPropRef { get; private set; }
        public PropRef? SourcePropRef { get; private set; }
        public Dice? SourceValue { get; private set; }
        public ModSourceValueFunction SourceValueFunc { get; private set; } = new ModSourceValueFunction();

        public virtual Mod Create(string name)
        {
            var mod = new Mod(name, this);
            return mod;
        }

        public virtual Mod Create()
            => Create(Name ?? TargetPropRef.Prop);

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

        public ModTemplate SetProps(PropRef targetPropRef, PropRef? sourcePropRef, Dice? value, ModSourceValueFunction? valueFunc)
        {
            TargetPropRef = new PropRef(targetPropRef.EntityId, targetPropRef.Prop);
            SourcePropRef = sourcePropRef != null
                ? new PropRef(sourcePropRef.EntityId, sourcePropRef.Prop)
                : null;

            SourceValue = value;
            SourceValueFunc.Set(valueFunc);

            return this;
        }

        public ModTemplate SetProps<TTarget, TSourceValue>(TTarget target, string targetProp, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            TargetPropRef = PropRef.CreatePropRef(target, targetProp);
            SourcePropRef = PropRef.CreatePropRef(target, sourceExpr);
            SourceValueFunc.Set(valueFunc);

            return this;
        }

        public ModTemplate SetProps<TTarget>(TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            TargetPropRef = PropRef.CreatePropRef(target, targetProp);
            SourceValue = value;
            SourceValueFunc.Set(valueFunc);

            return this;
        }

        public ModTemplate SetProps<TTarget, TTargetVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            TargetPropRef = PropRef.CreatePropRef(target, targetExpr);
            SourceValue = value;
            SourceValueFunc.Set(valueFunc);

            return this;
        }

        public ModTemplate SetProps<TTarget, TTargetVal, TSource, TSourceVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            TargetPropRef = PropRef.CreatePropRef(target, targetExpr);
            SourcePropRef = PropRef.CreatePropRef(source, sourceExpr);
            SourceValueFunc.Set(valueFunc);

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

    public class InitialMod : ModTemplate
    {
        public InitialMod()
        {
            Behavior = new Replace(ModType.Initial);
            Lifecycle = new PermanentLifecycle();
        }
    }

    public class BaseMod : ModTemplate
    {
        public BaseMod()
        {
            Behavior = new Replace(ModType.Base);
            Lifecycle = new PermanentLifecycle();
        }
    }

    public class OverrideMod : ModTemplate
    {
        public OverrideMod()
        {
            Behavior = new Replace(ModType.Override);
            Lifecycle = new PermanentLifecycle();
        }
    }

    public class SyncedMod : ModTemplate
    {
        public string SyncedToId { get; private set; }
        public string SyncedToType {  get; private set; }

        public SyncedMod(string syncedToId, string syncedToType)
        {
            SyncedToId = syncedToId;
            SyncedToType = syncedToType;
            Behavior = new Add(ModType.Standard);
            Lifecycle = new SyncedLifecycle();
        }

        public override Mod Create(string name)
        {
            var mod = new Mod(SyncedToId, SyncedToType, name, this);
            return mod;
        }
    }

    //public class StateMod : SyncedMod
    //{
    //    public StateMod(ModState state, int increment)
    //        : base(state.EntityId, nameof(ModState))
    //    {
    //        SetBehavior(new Combine(ModType.State));
    //        SetProps(state.EntityId, state.InstanceName, increment);
    //    }
    //}

    //public class ForceStateMod : PermanentMod
    //{
    //    public ForceStateMod(ModState state)
    //        : base(state.InstanceName)
    //    {
    //        SetBehavior(new Replace(ModType.ForceState));
    //        SetProps(state.EntityId, state.InstanceName, 1);
    //    }
    //}

    public class PermanentMod : ModTemplate
    {
        public PermanentMod(string name)
        {
            Name = name;
            SetLifecycle(new PermanentLifecycle());
            SetBehavior(new Add(ModType.Standard));
        }

        public PermanentMod()
        {
            SetLifecycle(new PermanentLifecycle());
            SetBehavior(new Add(ModType.Standard));
        }
    }

    public class ExpiresOnMod : ModTemplate
    {
        public ExpiresOnMod(int value) 
        {
            SetLifecycle(new PermanentLifecycle());
            SetBehavior(new ExpiresOn(value));
        }
    }

    public class ExpireOnZeroMod : ExpiresOnMod
    {
        public ExpireOnZeroMod()
            : base(0)
        { }
    }
}
