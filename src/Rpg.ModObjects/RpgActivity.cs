using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Time.Lifecycles;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects
{
    public class RpgActivity : RpgObject
    {
        private RpgEntity? _initiator;
        [JsonIgnore] public RpgEntity? Initiator
        {
            get
            {
                if (_initiator == null && Graph != null)
                    _initiator = Graph.GetObject<RpgEntity>(InitiatorId);

                return _initiator;
            }
        }

        [JsonProperty] public string InitiatorId { get; private set; }
        [JsonProperty] public TimePoint Time { get; private set; }
        [JsonProperty] public int ActivityNo { get; private set; }
        [JsonProperty] public int NextActionNo { get; private set; }
        [JsonProperty] public List<ActionInstance> ActionInstances { get; private set; } = new();
        public ActionInstance? ActionInstance { get => ActionInstances.LastOrDefault(); }

        [JsonProperty] public List<ModSet> OutputSets { get; init; } = new();
        private string OutcomeSetName { get => $"{Name}/OutcomeSet"; }
        public ModSet OutcomeSet { get => OutputSets.First(x => x.Name == OutcomeSetName); }
        private string CostSetName { get => $"{Name}/CostSet"; }
        public ModSet CostSet { get => OutputSets.First(x => x.Name == CostSetName); }

        [JsonConstructor]
        protected RpgActivity()
            : base()
        {
        }

        public RpgActivity(RpgEntity initiator, int activityNo)
            : this()
        {
            _initiator = initiator;
            InitiatorId = initiator.Id;
            ActivityNo = activityNo;
        }

        public void CreateActionInstance(RpgEntity owner, string actionName)
        {
            var action = owner.GetAction(actionName);
            if (action != null)
            {
                var instance = new ActionInstance(owner, action, NextActionNo++);
                instance.OnBeforeTime(Graph!);

                InitActionProps(instance);
                ActionInstances.Add(instance);
            }
        }

        #region Activity Mods

        public string[] GetActivityPropNames()
            => Props.Keys
                .Where(x => !x.Contains('/'))
                .ToArray();

        public string[] GetActionPropNames()
            => Props.Keys
                .Where(x => x.EndsWith($"/{ActionInstance!.ActionNo}"))
                .ToArray();

        public Dice? GetActivityProp(string prop)
            => Graph!.CalculatePropValue(this, prop);

        public RpgActivity ActivityMod(string targetProp, string modName, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        {
            var mod = new BaseMod()
                .SetName(modName)
                .SetProps(this, targetProp, dice, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }

        public RpgActivity ActivityMod<TSource, TSourceValue>(string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TSource : RpgObject
        {
            var mod = new BaseMod()
                .SetProps(this, targetProp, source, sourceExpr, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }

        public RpgActivity ActivityResultMod(string targetProp, string modName, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        {
            var mod = new OverrideMod()
                .SetName(modName)
                .SetProps(this, targetProp, dice, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }

        public RpgActivity ActivityResultMod<TSource, TSourceValue>(string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TSource : RpgObject
        {
            var mod = new OverrideMod()
                .SetProps(this, targetProp, source, sourceExpr, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }

        #endregion Activity Mods

        #region Action Mods

        public bool HasActionProp(string prop)
        {
            var propName = GetPropNameFromArg(prop, ActionInstance!.ActionNo);
            return Props.ContainsKey(propName) && Props[propName].Mods.Any();
        }

        public Dice? GetActionProp(string prop)
            => Graph!.CalculatePropValue(this, GetPropNameFromArg(prop, ActionInstance!.ActionNo));

        private void SetActionProp(string prop, object? val)
        {
            if (Dice.TryParse(val, out var dice))
            {
                if (HasActionProp(prop))
                    ActionResultMod(prop, "arg", dice);
                else
                    this.InitMod(GetPropNameFromArg(prop, ActionInstance!.ActionNo), dice);
            }
        }

        public RpgActivity ActionMod(string targetProp, string modName, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        {
            var mod = new BaseMod()
                .SetName(modName)
                .SetProps(this, GetPropNameFromArg(targetProp, ActionInstance!.ActionNo), dice, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }

        public RpgActivity ActionMod<TSource, TSourceValue>(string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TSource : RpgObject
        {
            var mod = new BaseMod()
                .SetProps(this, GetPropNameFromArg(targetProp, ActionInstance!.ActionNo), source, sourceExpr, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }

        public RpgActivity ActionResultMod(string targetProp, string modName, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        {
            var mod = new OverrideMod()
                .SetName(modName)
                .SetProps(this, GetPropNameFromArg(targetProp, ActionInstance!.ActionNo), dice, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }

        public RpgActivity ActionResultMod<TSource, TSourceValue>(string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TSource : RpgObject
        {
            var mod = new OverrideMod()
                .SetProps(this, GetPropNameFromArg(targetProp, ActionInstance!.ActionNo), source, sourceExpr, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }

        #endregion Action Mods

        public RpgActivity SetAll(string arg, object? value)
        {
            var val = ActionInstance!.SetCanAct(arg, value);
            val = ActionInstance!.SetCost(arg, value);
            val = ActionInstance!.SetAct(arg, value);
            val = ActionInstance!.SetOutcome(arg, value);

            SetActionProp(arg, val);
            return this;
        }

        public Dictionary<string, object?> GetCanActArgs()
            => CreateArgs(ActionInstance!.CanActArgs);

        public bool CanAct()
            => ActionInstance!.Action!.CanAct(GetCanActArgs());

        public RpgActivity SetCanActArg(string arg, object? value)
        {
            var val = ActionInstance!.SetCanAct(arg, value);
            SetActionProp(arg, val);

            return this;
        }

        public void SetCanActArgs(Dictionary<string, string?> argValues)
        {
            var args = ActionInstance!.Action!.OnCanAct.Args;
            foreach (var argName in argValues.Keys)
                SetCanActArg(argName, argValues[argName]);
        }

        public Dictionary<string, object?> GetCostArgs()
            => CreateArgs(ActionInstance!.CostArgs);

        public ModSet Cost()
        {
            ActionInstance!.Action!.Cost(GetCostArgs());
            return CostSet;
        }

        public RpgActivity SetCostArg(string arg, object? value)
        {
            var val = ActionInstance!.SetCost(arg, value);
            SetActionProp(arg, val);
            return this;
        }

        public void SetCostArgs(Dictionary<string, string?> argValues)
        {
            var args = ActionInstance!.Action!.OnCost.Args;
            foreach (var argName in argValues.Keys)
                SetCostArg(argName, argValues[argName]);
        }

        public Dictionary<string, object?> GetActArgs()
            => CreateArgs(ActionInstance!.ActArgs);

        public bool Act()
            => ActionInstance!.Action!.Act(GetActArgs());

        public RpgActivity SetActArg(string arg, object? value)
        {
            var val = ActionInstance!.SetAct(arg, value);
            SetActionProp(arg, val);
            return this;
        }

        public void SetActArgs(Dictionary<string, string?> argValues)
        {
            var args = ActionInstance!.Action!.OnAct.Args;
            foreach (var argName in argValues.Keys)
                SetActArg(argName, argValues[argName]);
        }

        public Dictionary<string, object?> GetOutcomeArgs()
            => CreateArgs(ActionInstance!.OutcomeArgs);

        public ModSet[] Outcome()
        {
            ActionInstance!.Action!.Outcome(GetOutcomeArgs());
            return OutputSets
                .Where(x => x.Name != CostSetName)
                .ToArray();
        }

        public RpgActivity SetOutcomeArg(string arg, object? value)
        {
            var val = ActionInstance!.SetOutcome(arg, value);
            SetActionProp(arg, val);
            return this;
        }

        public void SetOutcomeArgs(Dictionary<string, string?> argValues)
        {
            var args = ActionInstance!.Action!.OnOutcome.Args;
            foreach (var argName in argValues.Keys)
                SetOutcomeArg(argName, argValues[argName]);
        }

        public void AutoComplete()
        {
            if (!CanAct())
                throw new InvalidOperationException("Cannot AutoComplete");

            Cost();
            Graph!.AddModSets(CostSet);
            Graph!.Time.TriggerEvent();

            Act();
            Outcome();
            Complete();
        }

        private Dictionary<string, object?> CreateArgs(Dictionary<string, object?> source)
        {
            var args = new Dictionary<string, object?>(source);

            if (args.ContainsKey("initiator") && args["initiator"] == null)
                args["initiator"] = Initiator;

            if (args.ContainsKey("owner") && args["owner"] == null)
                args["owner"] = ActionInstance!.Owner;

            if (args.ContainsKey("activity") && args["activity"] == null)
                args["activity"] = this;

            foreach (var argName in GetActionPropNames())
            {
                if (args.ContainsKey(argName) && args[argName] == null)
                    args[argName] = GetActionProp(argName);
            }

            return args;
        }

        public void Complete()
        {
            Graph!.AddModSets([.. OutputSets.Where(x => x.Name != CostSetName)]);
            Graph!.Time.TriggerEvent();
        }

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);
            Time = graph.Time.Current;
            Name = $"{InitiatorId}/{Time.Type}/{Time.Tick}/{ActivityNo}";

            var outcomeSet = GetModSetByName(OutcomeSetName) as ModSet;
            if (outcomeSet == null)
            {
                outcomeSet = new ModSet(InitiatorId, new TurnLifecycle(), OutcomeSetName);
                outcomeSet.OnBeforeTime(graph);
                outcomeSet.Unapply();

                OutputSets.Add(outcomeSet);
            }

            var costSet = GetModSetByName(CostSetName) as ModSet;
            if (costSet == null)
            {
                costSet = new ModSet(InitiatorId, new TurnLifecycle(), CostSetName);
                costSet.OnBeforeTime(graph);
                costSet.Unapply();

                OutputSets.Add(costSet);
            }
        }

        private void InitActionProps(ActionInstance instance)
        {
            var props = instance.Args!.Values
                .Where(x => x.TypeName == nameof(Int32) || x.TypeName == nameof(Dice));

            foreach (var prop in props)
                GetProp(GetPropNameFromArg(prop.Name, instance.ActionNo), create: true);
        }

        private string GetPropNameFromArg(string argName, int actionNo)
            => $"{argName}/{actionNo}";

        private string GetArgNameFromProp(string propName)
            => propName.Split('/').First();
    }
}
