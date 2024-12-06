using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Reflection.Args;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Activities
{
    public sealed class Action : RpgObject
    {
        private Activity? Owner { get => Graph?.GetObject<Activity>(OwnerId); }
        private RpgEntity? ActionOwner { get => Graph?.GetObject<RpgEntity>(ActionOwnerId); }
        private RpgEntity? Initiator { get => Graph?.GetObject<RpgEntity>(InitiatorId); }

        private string ActionModSetName { get; init; }
        private string CostModSetName { get; init; }
        private string PerformModSetName { get; init; }
        private string OutcomeModSetName { get; init; }

        //[JsonProperty] internal RpgMethod<ActionTemplate, bool>? OnCanPerform { get; private init; }
        [JsonProperty] internal RpgMethod<ActionTemplate, bool>? OnCost { get; private init; }
        [JsonProperty] internal RpgMethod<ActionTemplate, bool>? OnPerform { get; private init; }
        [JsonProperty] internal RpgMethod<ActionTemplate, bool> OnOutcome { get; private init; }

        [JsonProperty] public string ActionTemplateName { get; private init; }
        [JsonProperty] public int ActionNo { get; private init; }

        [JsonProperty] public string ActionOwnerId { get; private init; }
        [JsonProperty] public string InitiatorId { get; private init; }

        public RpgArg[] ActionArgs { get; private set; } = [];

        [JsonProperty] public bool IsCostDone { get; private set; }
        [JsonProperty] public bool IsPerformDone { get; private set; }
        [JsonProperty] public bool IsOutcomeDone { get; private set; }

        public ModSet CostModSet { get => GetModSetByName(CostModSetName)!; }
        public ModSet OutcomeModSet { get => GetModSetByName(OutcomeModSetName)!; }
        public List<StateRef> OutcomeStates { get; private set; } = new();
        public List<ActionRef> OutcomeActions { get; private set; } = new();

        public ActionStatus Status { get => GetActionStatus(); }

        [JsonProperty] public bool IsStarted { get; private set; }
        [JsonProperty] public bool IsComplete { get; private set; }

        [JsonConstructor] private Action() { }

        internal Action(ActionTemplate actionTemplate, RpgEntity owner, RpgEntity initiator, Activity activity)
            : base(activity.Id)
        {
            SetLifespan(new Lifespan(0, 1));
            ActionOwnerId = owner.Id;
            InitiatorId = initiator.Id;
            ActionNo = activity.NextActionNo;

            ActionTemplateName = actionTemplate.Name;
            //OnCanPerform = actionTemplate.CanPerformMethod;
            OnCost = actionTemplate.CostMethod;
            OnPerform = actionTemplate.PerformMethod;
            OnOutcome = actionTemplate.OutcomeMethod;

            ActionModSetName = $"{ActionTemplateName}/{activity.ActivityNo}/{ActionNo}/Action";
            CostModSetName = $"{ActionTemplateName}/{activity.ActivityNo}/{ActionNo}/Cost";
            PerformModSetName = $"{ActionTemplateName}/{activity.ActivityNo}/{ActionNo}/Perform";
            OutcomeModSetName = $"{ActionTemplateName}/{activity.ActivityNo}/{ActionNo}/Outcome";

            AddModSet(new ModSet(Id, ActionModSetName));

            var costModSet = new ModSet(Id, CostModSetName);
            costModSet.Unapply();
            AddModSet(costModSet);

            var performModSet = new ModSet(Id, PerformModSetName);
            performModSet.Unapply();
            AddModSet(performModSet);

            var outcomeModSet = new ModSet(Id, OutcomeModSetName);
            outcomeModSet.Unapply();
            AddModSet(outcomeModSet);

            IsCostDone = OnCost == null;
            IsPerformDone = OnPerform == null;

            ActionArgs = actionTemplate.ActionArgs
                .Select(x => x.Clone())
                .ToArray();
        }

        public Action SetOutcomeState(RpgObject obj, string stateName, Lifespan lifespan)
        {
            var stateRef = new StateRef(obj.Id, stateName, lifespan);
            OutcomeStates = OutcomeStates.Where(x => x.OwnerId != obj.Id || x.State != stateName).ToList();
            OutcomeStates.Add(stateRef);

            return this;
        }

        public Action SetOutcomeAction(RpgEntity actionOwner, string actionName, bool optional)
        {
            var template = actionOwner.GetActionTemplate(actionName);
            OutcomeActions = OutcomeActions.Where(x => x.ActionTemplateOwnerId != actionOwner.Id || x.ActionTemplateName != actionName).ToList();
            OutcomeActions.Add(new ActionRef(OwnerId!, actionOwner.Id, actionName, optional));

            return this;
        }

        public Action ResetProp(params string[] props)
        {
            foreach (var prop in props)
            {
                Graph.RemoveMods(GetProp(prop)?.Mods.ToArray() ?? []);
                GetProp(prop)?.Mods.Clear();
            }

            return this;
        }

        public Action SetProp(string targetProp, Dice? dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        {
            if  (dice != null)
            {
                var modSet = GetModSetByName(ActionModSetName)!;
                modSet.Add(new Permanent(modSet.Id), this, targetProp, dice.Value, valueCalc);
            }

            return this;
        }

        public Action SetProp<TSource, TSourceValue>(string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TSource : RpgObject
        {
            var modSet = GetModSetByName(ActionModSetName)!;
            modSet.Add(new Permanent(modSet.Id), this, targetProp, source, sourceExpr, valueFunc);

            return this;
        }

        public CostResult CreateCostResult()
        {
            var res = new CostResult { ModSet = new ModSet(Id, CostModSetName) };
            res.ModSet.OnCreating(Graph, this);
            res.ModSet.Unapply();
            return res;
        }

        public OutcomeResult CreateOutcomeResult()
        {
            var res = new OutcomeResult { ModSet = new ModSet(Id, OutcomeModSetName) };
            res.ModSet.OnCreating(Graph, this);
            res.ModSet.Unapply();
            return res;
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            InitActionArgs();
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            var expiry = base.OnStartLifecycle();
            return expiry;
        }

        //internal RpgArg[] CanPerformArgs()
        //    => OnCanPerform?.Args
        //        ?.CloneArgs()
        //        .Fill(ActionArgs) ?? [];

        //internal bool CanPerform(RpgArg[] args)
        //{
        //    ActionArgs.Fill(args);

        //    if (OnCanPerform != null)
        //        return ExecuteStep(args, OnCanPerform.Execute);

        //    return true;
        //}

        internal RpgArg[] CostArgs()
            => OnCost?.Args
                ?.CloneArgs()
                .Fill(ActionArgs) ?? [];

        internal bool Cost(RpgArg[] args)
        {
            ActionArgs.Fill(args);

            IsStarted = true;

            ResetCost();
            ResetPerform();
            if (OnCost != null)
                IsCostDone = ExecuteStep(args, OnCost.Execute);

            return IsCostDone;
        }

        private void ResetCost()
        {
            IsCostDone = OnCost == null;
            CostModSet.Reset();
        }

        internal RpgArg[] PerformArgs()
            => OnPerform?.Args
                ?.CloneArgs()
                .Fill(ActionArgs) ?? [];

        internal bool Perform(RpgArg[] args)
        {
            ActionArgs.Fill(args);

            IsStarted = true;

            ResetPerform();
            if (OnPerform != null)
                IsPerformDone = ExecuteStep(args, OnPerform.Execute);

            return IsPerformDone;
        }

        private void ResetPerform()
        {
            IsPerformDone = OnPerform == null;
        }

        internal RpgArg[] OutcomeArgs()
            => OnOutcome?.Args
                ?.CloneArgs()
                .Fill(ActionArgs) ?? [];

        internal bool Outcome(RpgArg[] args)
        {
            ActionArgs.Fill(args);

            IsStarted = true;

            IsOutcomeDone = ExecuteStep(args, OnOutcome.Execute);
            return IsOutcomeDone;
        }

        private void ResetOutcome()
        {
            if (!IsComplete)
            {
                IsOutcomeDone = false;
                OutcomeModSet.Reset();
                OutcomeStates.Clear();
                OutcomeActions.Clear();
            }
        }

        internal ActionRef[] Complete()
        {
            if (Graph != null && Status == ActionStatus.CanComplete)
            {
                CostModSet.Apply();
                OutcomeModSet.Apply();
                foreach (var stateRef in OutcomeStates)
                {
                    var stateOwner = Graph.GetObject<RpgObject>(stateRef.OwnerId);
                    var state = stateOwner?.GetState(stateRef.State);
                    state?.Activate(Id, stateRef.Lifespan);
                }

                IsComplete = true;
                Graph.Time.TriggerEvent();

                return OutcomeActions.ToArray();
            }

            return [];
        }

        public ActionRef[] AutoComplete(params (string, object?)[]? args)
        {
            if (args != null)
                foreach (var arg in args)
                {
                    ActionArgs.Set(arg.Item1, arg.Item2);
                }

            if (Status == ActionStatus.CanAutoComplete)
            {
                Cost(CostArgs());
                Perform(PerformArgs());
                Outcome(OutcomeArgs());
            }

            return Complete();
        }

        internal void Reset()
        {
            if (Graph != null)
            {
                if (IsComplete)
                {
                    CostModSet.Reset();
                    OutcomeModSet.Reset();

                    foreach (var stateRef in OutcomeStates)
                    {
                        var stateOwner = Graph.GetObject<RpgObject>(stateRef.OwnerId);
                        var state = stateOwner?.GetState(stateRef.State);
                        state?.Deactivate(Id, stateRef.Lifespan);
                    }

                    IsComplete = false;
                    Graph.Time.TriggerEvent();
                }
            }
        }

        private bool ExecuteStep(RpgArg[] args, Func<ActionTemplate, Dictionary<string, object?>, bool> runStep)
        {
            ResetOutcome();
            var template = ActionOwner!.GetActionTemplate(ActionTemplateName)!;
            var result = runStep.Invoke(template, args.ToDictionary(Graph!));

            foreach (var prop in Props.Values)
                ActionArgs.Set(prop.Name, Value(prop.Name));

            return result;
        }

        private ActionStatus GetActionStatus()
        {
            if (IsComplete)
                return ActionStatus.Completed;

            if (IsCostDone && IsPerformDone && IsOutcomeDone)
                return ActionStatus.CanComplete;

            if (IsStarted)
                return ActionStatus.Started;

            return CanAutoComplete()
                ? ActionStatus.CanAutoComplete
                : ActionStatus.NotStarted;
        }

        private bool CanAutoComplete()
        {
            if (IsComplete || IsOutcomeDone) return false;

            var setupArgs = CostArgs();
            if (!setupArgs.IsComplete()) return false;

            var performArgs = PerformArgs();
            if (!performArgs.IsComplete()) return false;

            var outcomeArgs = OutcomeArgs();
            if (!outcomeArgs.IsComplete()) return false;

            return true;
        }

        private void InitActionArgs()
        {
            foreach (var arg in ActionArgs)
            {
                var argVal = InitArgValue(arg.Name);
                var val = argVal is Mod
                    ? ((Mod)argVal).Value()
                    : argVal;

                if (val == null && Owner != null)
                {
                    for (int i = Owner.CurrentActionNo - 1; i >= 0; i--)
                    {
                        val = Owner.Actions[i].Value(arg.Name);
                        if (val != null)
                        {
                            arg.FillValue(val);
                            break;
                        }
                    }
                }

                arg.SetValue(val);
                
                if (argVal is Mod mod)
                {
                    var prop = GetProp(arg.Name, RefType.Value, true);
                    AddMods(mod);
                }
            }
        }


        private object? InitArgValue(string argName)
        {
            if (ActionArgs.Val(argName) != null)
                return ActionArgs.Val(argName);
            
            var propRef = ArgNameToPropRef(argName);
            if (propRef != null)
                return new Base().Set(new PropRef(Id, argName), propRef);

            RpgObject? entity = argName switch
            {
                "owner" => ActionOwner,
                "initiator" => Initiator,
                "activity" => Owner,
                "action" => this,
                _ => null
            };

            return entity;
        }

        private PropRef? ArgNameToPropRef(string argName)
        {
            var propParts = argName.Split('_');
            var propName = propParts[0];
            RpgObject? entity = propName switch
            {
                "owner" => ActionOwner,
                "initiator" => Initiator,
                "activity" => Owner,
                "action" => this,
                _ => null
            };

            if (entity != null && propParts.Length > 1)
            {
                var prop = string.Join('.', propParts.Skip(1));
                return PropRef.CreatePropRef(entity, prop);
            }

            return null;
        }
    }

    //THESE CAN BE CONVERTED TO SETPROPS (SEE ABOVE) AS THEY BECOME NEEDED...


    //public static class ActionExtensions
    //{

    //    public static Action2 Add<TEntity>(this Action2 action, Mod mod, TEntity target, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
    //        where TEntity : RpgObject
    //    {
    //        var modSet = action.GetModSetByName(action.ActionModSetName)!;
    //        mod.Set(target, targetProp, dice, valueCalc);
    //        modSet.AddMods(mod);

    //        return action;
    //    }

    //    public static Action2 Add<TEntity, TTargetValue, TSourceValue>(this Action2 action, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
    //        where TEntity : RpgObject
    //    {
    //        var modSet = action.GetModSetByName(action.ActionModSetName)!;
    //        modSet.Add(new Synced(modSet.Id), entity, targetExpr, entity, sourceExpr, valueCalc);

    //        return action;
    //    }

    //    public static Action2 Add<TTarget, TTargetValue, TSource, TSourceValue>(this Action2 action, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
    //        where TTarget : RpgObject
    //        where TSource : RpgObject
    //    {
    //        var modSet = action.GetModSetByName(action.ActionModSetName)!;
    //        modSet.Add(new Synced(modSet.Id), target, targetExpr, source, sourceExpr, valueCalc);

    //        return action;
    //    }

    //    public static Action2 Add<TTarget, TTargetVal, TSource, TSourceVal>(this Action2 action, Mod mod, TTarget target, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
    //        where TSource : RpgObject
    //        where TTarget : RpgObject
    //    {
    //        var modSet = action.GetModSetByName(action.ActionModSetName)!;
    //        mod.Set(target, targetExpr, source, sourceExpr, valueFunc);
    //        modSet.AddMods(mod);

    //        return action;
    //    }

    //    public static Action2 Add<TEntity, TSourceValue>(this Action2 action, TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
    //        where TEntity : RpgObject
    //    {
    //        var modSet = action.GetModSetByName(action.ActionModSetName)!;
    //        modSet.Add(new Synced(modSet.Id), entity, targetProp, sourceExpr, valueCalc);

    //        return action;
    //    }

    //    public static Action2 Add<TTarget, TSourceValue>(this Action2 action, Mod mod, TTarget target, string targetProp, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
    //        where TTarget : RpgObject
    //    {
    //        var modSet = action.GetModSetByName(action.ActionModSetName)!;
    //        mod.Set(target, targetProp, sourceExpr, valueFunc);
    //        modSet.AddMods(mod);

    //        return action;
    //    }



    //    public static Action2 Add<TTarget, TSource, TSourceValue>(this Action2 action, Mod mod, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
    //        where TTarget : RpgObject
    //        where TSource : RpgObject
    //    {
    //        var modSet = action.GetModSetByName(action.ActionModSetName)!;
    //        mod.Set(target, targetProp, source, sourceExpr, valueFunc);
    //        modSet.AddMods(mod);

    //        return action;
    //    }

    //    public static Action2 Add<TTarget, TTargetValue>(this Action2 action, Mod mod, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
    //        where TTarget : RpgObject
    //    {
    //        var modSet = action.GetModSetByName(action.ActionModSetName)!;
    //        mod.Set(target, targetExpr, dice, valueFunc);
    //        modSet.AddMods(mod);

    //        return action;
    //    }

    //    public static Action2 Add<TEntity, TTargetValue>(this Action2 action, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
    //        where TEntity : RpgObject
    //    {
    //        var modSet = action.GetModSetByName(action.ActionModSetName)!;
    //        modSet.Add(new Synced(modSet.Id), entity, targetExpr, dice, valueCalc);

    //        return action;
    //    }


    //    public static Action2 Add<TEntity, TTarget, TTargetValue, TSourceValue>(this Action2 action, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
    //        where TEntity : RpgObject
    //    {
    //        var modSet = action.GetModSetByName(action.ActionModSetName)!;
    //        modSet.Add(new Synced(modSet.Id), entity, targetExpr, entity, sourceExpr, valueCalc);

    //        return action;
    //    }
    //}
}
