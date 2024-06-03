using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class ModTemplate
    {
        public ModBehavior Behavior { get; protected set; }
        public ITimeLifecycle Lifecycle { get; protected set; }
        public PropRef TargetPropRef { get; private set; }
        public PropRef? SourcePropRef { get; private set; }
        public Dice? SourceValue { get; private set; }
        public ModSourceValueFunction SourceValueFunc { get; private set; } = new ModSourceValueFunction();

        public static ModTemplate Init<TBehavior, TLifecycle>()
            where TBehavior : ModBehavior
            where TLifecycle : ITimeLifecycle
        {
            var behavior = Activator.CreateInstance<TBehavior>();
            var lifecycle = Activator.CreateInstance<TLifecycle>();

            var modFactory = new ModTemplate
            {
                Behavior = behavior,
                Lifecycle = lifecycle
            };

            return modFactory;
        }

        public ModTemplate SetBehavior(ModBehavior behavior)
        {
            Behavior = behavior;
            return this;
        }

        public ModTemplate SetLifecycle(ITimeLifecycle lifecycle)
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

        public ModTemplate SetProps<TTarget>(TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            TargetPropRef = Mod.CreatePropRef(target, targetProp);
            SourceValue = value;
            SourceValueFunc.Set(valueFunc);

            return this;
        }

        public ModTemplate SetProps<TTarget, TTargetVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            TargetPropRef = Mod.CreatePropRef(target, targetExpr);
            SourceValue = value;
            SourceValueFunc.Set(valueFunc);

            return this;
        }

        public ModTemplate SetProps<TTarget, TTargetVal, TSource, TSourceVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            TargetPropRef = Mod.CreatePropRef(target, targetExpr);
            SourcePropRef = Mod.CreatePropRef(source, sourceExpr);
            SourceValueFunc.Set(valueFunc);

            return this;
        }

        public ModTemplate SetProps<TTarget, TSource, TSourceVal>(TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            TargetPropRef = Mod.CreatePropRef(target, targetProp);
            SourcePropRef = Mod.CreatePropRef(source, sourceExpr);

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
    }
}
