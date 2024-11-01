using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.Reflection.Args;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Actions
{
    public class Activity : RpgObject
    {
        private RpgEntity? _initiator;

        [JsonIgnore]
        public RpgEntity? Initiator
        {
            get
            {
                if (_initiator == null && Graph != null)
                    _initiator = Graph.GetObject<RpgEntity>(InitiatorId);

                return _initiator;
            }
        }

        [JsonProperty] public string InitiatorId { get; protected set; }
        [JsonProperty] public PointInTime Time { get; protected set; }
        [JsonProperty] public int ActivityNo { get; protected set; }
        [JsonProperty] public int NextActionNo { get; protected set; }
        [JsonProperty] public List<ActionInstance> ActionInstances { get; protected set; } = new();
        public ActionInstance? ActionInstance { get; protected set; }

        [JsonProperty] public List<ModSet> OutputSets { get; init; } = new();
        private string OutcomeSetName { get => $"{Name}/OutcomeSet"; }
        public ModSet OutcomeSet { get => OutputSets.First(x => x.Name == OutcomeSetName); }
        private string CostSetName { get => $"{Name}/CostSet"; }
        public ModSet CostSet { get => OutputSets.First(x => x.Name == CostSetName); }

        [JsonConstructor]
        protected Activity()
            : base()
        {
        }

        internal Activity(RpgEntity initiator, RpgEntity owner, string actionName, int activityNo)
        {
            _initiator = initiator;
            InitiatorId = initiator.Id;
            ActivityNo = activityNo;
            Name = $"{actionName}/{activityNo}";
            ActionInstances.Add(new ActionInstance(owner, owner.GetAction(actionName)!, activityNo));
        }

        internal Activity(RpgEntity initiator, ActivityTemplate actionGroup, int activityNo)
        {
            _initiator = initiator;
            InitiatorId = initiator.Id;
            ActivityNo = activityNo;
            Name = $"{actionGroup.Name}/{activityNo}";

            foreach (var item in actionGroup.Items)
            {
                var owner = Graph!.GetObjects<RpgEntity>()
                    .First(x => x.Archetypes.Contains(item.OwnerArchetype) && x.Actions.Values.Any(a => a.Name == item.ActionName));

                ActionInstances.Add(new ActionInstance(owner, owner.GetAction(item.ActionName)!, activityNo));
            }
        }

        public void SetActionInstance(string actionName)
        {
            var next = ActionInstances.FirstOrDefault(x => x.ActionName == actionName);
            if (next != null)
                ActionInstance = next;
        }

        #region Activity Mods

        public string[] GetActivityPropNames()
            => Props.Keys
                .Where(x => !x.Contains('/'))
                .ToArray();

        private void SetActivityProp(string prop, object? val)
        {
            if (Dice.TryParse(val, out var dice))
            {
                if (HasActivityProp(prop))
                {
                    var existing = GetActivityProp(prop);
                    if (existing != dice)
                        ActivityResultMod(prop, "arg", dice);
                }
                else
                    AddMods(new Initial(Id, prop, dice));
            }
        }

        public bool HasActivityProp(string prop)
            => Props.ContainsKey(prop) && Props[prop].Mods.Any();

        public Dice? GetActivityProp(string prop)
            => Graph!.CalculatePropValue(this, prop);

        public Activity ActivityMod(string targetProp, string modName, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        {
            var mod = new Base()
                .SetName(modName)
                .Set(this, targetProp, dice, valueCalc);

            AddMods(mod);

            return this;
        }

        public Activity ActivityMod<TSource, TSourceValue>(string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TSource : RpgObject
        {
            var mod = new Base()
                .Set(this, targetProp, source, sourceExpr, valueCalc);

            AddMods(mod);

            return this;
        }

        public Activity ActivityResultMod(string targetProp, string modName, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        {
            var mod = new Override()
                .SetName(modName)
                .Set(this, targetProp, dice, valueCalc);

            AddMods(mod);

            return this;
        }

        public Activity ActivityResultMod<TSource, TSourceValue>(string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TSource : RpgObject
        {
            var mod = new Override()
                .Set(this, targetProp, source, sourceExpr, valueCalc);

            AddMods(mod);

            return this;
        }

        #endregion Activity Mods

        public Activity SetAll(string arg, object? value)
        {
            var val = ActionInstance!.SetCanAct(arg, value);
            val = ActionInstance!.SetCost(arg, value);
            val = ActionInstance!.SetAct(arg, value);
            val = ActionInstance!.SetOutcome(arg, value);

            SetActivityProp(arg, val);
            return this;
        }

        public ActionArgs GetCanActArgs()
            => new ActionArgs(ActionInstance!.Action!.OnCanAct.Args, CreateArgs(ActionInstance!.Action!.OnCanAct.Args, ActionInstance!.CanActArgs, true));

        public bool CanAct()
            => ActionInstance!.Action!.CanAct(CreateArgs(ActionInstance!.Action!.OnCanAct.Args, ActionInstance!.CanActArgs, false));

        public Activity SetCanActArg(string arg, object? value)
        {
            var val = ActionInstance!.SetCanAct(arg, value);
            SetActivityProp(arg, val);

            return this;
        }

        public void SetCanActArgs(Dictionary<string, string?> argValues)
        {
            var args = ActionInstance!.Action!.OnCanAct.Args;
            foreach (var argName in argValues.Keys)
                SetCanActArg(argName, argValues[argName]);
        }

        public ActionArgs GetCostArgs()
            => new ActionArgs(ActionInstance!.Action!.OnCost.Args, CreateArgs(ActionInstance!.Action!.OnCost.Args, ActionInstance!.CostArgs, true));

        public ModSet Cost()
        {
            ActionInstance!.Action!.Cost(CreateArgs(ActionInstance!.Action!.OnCost.Args, ActionInstance!.CostArgs, false));
            return CostSet;
        }

        public Activity SetCostArg(string arg, object? value)
        {
            var val = ActionInstance!.SetCost(arg, value);
            SetActivityProp(arg, val);
            return this;
        }

        public void SetCostArgs(Dictionary<string, string?> argValues)
        {
            var args = ActionInstance!.Action!.OnCost.Args;
            foreach (var argName in argValues.Keys)
                SetCostArg(argName, argValues[argName]);
        }

        public ActionArgs GetActArgs()
            => new ActionArgs(ActionInstance!.Action!.OnAct.Args, CreateArgs(ActionInstance!.Action!.OnAct.Args, ActionInstance!.ActArgs, true));

        public bool Act()
            => ActionInstance!.Action!.Act(CreateArgs(ActionInstance!.Action!.OnAct.Args, ActionInstance!.ActArgs, false));

        public Activity SetActArg(string arg, object? value)
        {
            var val = ActionInstance!.SetAct(arg, value);
            SetActivityProp(arg, val);
            return this;
        }

        public void SetActArgs(Dictionary<string, string?> argValues)
        {
            var args = ActionInstance!.Action!.OnAct.Args;
            foreach (var argName in argValues.Keys)
                SetActArg(argName, argValues[argName]);
        }

        public void SetActArgs(Dictionary<string, object?> argValues)
        {
            var args = ActionInstance!.Action!.OnAct.Args;
            foreach (var argName in argValues.Keys)
                SetActArg(argName, argValues[argName]);
        }

        public ActionArgs GetOutcomeArgs()
            => new ActionArgs(ActionInstance!.Action!.OnOutcome.Args, CreateArgs(ActionInstance!.Action!.OnOutcome.Args, ActionInstance!.OutcomeArgs, true));

        public ModSet[] Outcome()
        {
            ActionInstance!.Action!.Outcome(CreateArgs(ActionInstance!.Action!.OnOutcome.Args, ActionInstance!.OutcomeArgs, false));
            return OutputSets
                .Where(x => x.Name != CostSetName)
                .ToArray();
        }

        public Activity SetOutcomeArg(string arg, object? value)
        {
            var val = ActionInstance!.SetOutcome(arg, value);
            SetActivityProp(arg, val);
            return this;
        }

        public void SetOutcomeArgs(Dictionary<string, string?> argValues)
        {
            var args = ActionInstance!.Action!.OnOutcome.Args;
            foreach (var argName in argValues.Keys)
                SetOutcomeArg(argName, argValues[argName]);
        }

        public void SetOutcomeArgs(Dictionary<string, object?> argValues)
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

        private Dictionary<string, object?> CreateArgs(RpgArg[] args, Dictionary<string, object?> source, bool toOutput)
        {
            var target = new Dictionary<string, object?>(source);

            if (target.ContainsKey("initiator") && target["initiator"] == null)
                target["initiator"] = toOutput ? Initiator?.Id : Initiator;

            if (target.ContainsKey("owner") && target["owner"] == null)
                target["owner"] = toOutput ? ActionInstance!.Owner?.Id : ActionInstance!.Owner;

            if (target.ContainsKey("activity") && target["activity"] == null)
                target["activity"] = toOutput ? Id : this;

            foreach (var argName in GetActivityPropNames())
            {
                if (target.ContainsKey(argName) && target[argName] == null)
                {
                    var arg = args.First(x => x.Name == argName);
                    target[argName] = toOutput
                        ? arg.ToOutput(Graph!, GetActivityProp(argName))
                        : arg.FromInput(Graph!, GetActivityProp(argName));
                }
            }

            return target;
        }

        public void Complete()
        {
            Graph!.AddModSets([.. OutputSets.Where(x => x.Name != CostSetName)]);
            Graph!.Time.TriggerEvent();
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            foreach (var actionInstance in ActionInstances)
                actionInstance.OnBeforeTime(graph);

            Time = graph.Time.Now;

            var outcomeSet = GetModSetByName(OutcomeSetName) as ModSet;
            if (outcomeSet == null)
            {
                outcomeSet = new TurnModSet(InitiatorId, OutcomeSetName);
                outcomeSet.OnCreating(graph);
                outcomeSet.Unapply();

                OutputSets.Add(outcomeSet);
            }

            var costSet = GetModSetByName(CostSetName) as ModSet;
            if (costSet == null)
            {
                costSet = new TurnModSet(InitiatorId, CostSetName);
                costSet.OnCreating(graph);
                costSet.Unapply();

                OutputSets.Add(costSet);
            }
        }

        private void InitActionProps(ActionInstance instance)
        {
            var props = instance.Args!.Values
                .Where(x => x.TypeName == nameof(Int32) || x.TypeName == nameof(Dice));

            foreach (var prop in props)
                GetProp(prop.Name, create: true);
        }
    }
}
