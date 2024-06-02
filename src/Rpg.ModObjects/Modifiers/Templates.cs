using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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

        public void SetProps(string targetId, string prop, Dice value)
        {
            TargetPropRef = new PropRef(targetId, prop);
            SourceValue = value;
        }

        public void SetProps<TTarget, TTargetVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            TargetPropRef = Mod.CreatePropRef(target, targetExpr);
            SourceValue = value;
            SourceValueFunc.Set(valueFunc);
        }

        public void SetProps<TTarget, TTargetVal, TSource, TSourceVal>(TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            TargetPropRef = Mod.CreatePropRef(target, targetExpr);
            SourcePropRef = Mod.CreatePropRef(source, sourceExpr);
            SourceValueFunc.Set(valueFunc);
        }

        public void SetProps<TTarget, TSource, TSourceVal>(TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
            where TTarget : RpgObject
        {
            TargetPropRef = Mod.CreatePropRef(target, targetProp);
            SourcePropRef = Mod.CreatePropRef(source, sourceExpr);
        }

        public void SetProps<TTarget>(ModBehavior behavior, TTarget target, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            TargetPropRef = Mod.CreatePropRef(target, targetProp);
            SourceValue = value;
            SourceValueFunc.Set(valueFunc);
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
}
