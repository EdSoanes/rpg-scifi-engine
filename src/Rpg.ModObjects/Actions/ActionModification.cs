using Newtonsoft.Json;
using Rpg.ModObjects.Mods;

namespace Rpg.ModObjects.Actions
{
    public abstract class ActionModification<TOwner> : Modification
        where TOwner : RpgObject
    {
        //[JsonProperty] private ActionMethod<TOwner, bool> OnIsEnabledWhen { get; set; }
        [JsonProperty] private ActionMethod<TOwner, Modification> OnCost { get; set; }
        [JsonProperty] private ActionMethod<TOwner, Modification> OnAct { get; set; }
        [JsonProperty] private ActionMethod<TOwner, Modification[]> OnOutcome { get; set; }

        public ActionModification(TOwner owner)
            : base(new PermanentLifecycle())
        {
            AddOwner(owner);

            Name = GetType().Name;

            //OnIsEnabledWhen = new ActionMethod<TOwner, bool>(this, nameof(OnIsEnabledWhen));
            OnCost = new ActionMethod<TOwner, Modification>(this, nameof(OnCost));
            OnAct = new ActionMethod<TOwner, Modification>(this, nameof(OnAct));
            OnOutcome = new ActionMethod<TOwner, Modification[]>(this, nameof(OnOutcome));
        }

        public abstract bool IsEnabled(TOwner owner, RpgEntity initiator);

        //public ActionMethodArgs IsEnabledWhenArgs()
        //    => OnIsEnabledWhen.GetActionMethodArgs();

        //public bool IsEnabledWhen(TOwner owner, ActionMethodArgs args)
        //    => OnIsEnabledWhen.Execute(owner, args);
        public ActionMethodArgs CostArgs()
            => OnAct.GetActionMethodArgs();

        public Modification Cost(TOwner owner, ActionMethodArgs args)
            => OnAct.Execute(owner, args);

        public ActionMethodArgs ActArgs() 
            => OnAct.GetActionMethodArgs();

        public Modification Act(TOwner owner, ActionMethodArgs args)
            => OnAct.Execute(owner, args);

        public ActionMethodArgs OutcomeArgs() 
            => OnOutcome.GetActionMethodArgs();

        public Modification[] Outcome(TOwner owner, ActionMethodArgs args)
            => OnOutcome.Execute(owner, args);
    }
}