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
        [JsonProperty] public int ActionNo { get; private set; }
        [JsonProperty] public List<ActionInstance> ActionInstances { get; private set; } = new();
        public ActionInstance? ActionInstance { get => ActionInstances.LastOrDefault(); }
        public List<ModSet> OutcomeSets { get; init; } = new();
        public ModSet OutcomeSet { get => OutcomeSets.First(x => x.Name == Name); }

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
            var action = owner.GetAction(actionName)!;

            var instance = new ActionInstance(owner, action, ActionNo++);
            InitInstanceArgs(instance);
            InitInstanceProps(instance);
            ActionInstances.Add(instance);
        }

        public RpgActivity AddMod(string targetProp, string modName, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        {
            var mod = new BaseMod()
                .SetName(modName)
                .SetProps(this, GetArgPropName(ActionInstance!, targetProp), dice, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }

        public RpgActivity AddResultMod(string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        {
            var mod = new OverrideMod()
                .SetProps(this, GetArgPropName(ActionInstance!, targetProp), dice, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }

        public RpgActivity AddMod<TSource, TSourceValue>(string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TSource : RpgObject
        {
            var mod = new BaseMod()
                .SetProps(this, GetArgPropName(ActionInstance!, targetProp), source, sourceExpr, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }

        public bool CanAct()
            => ActionInstance!.Action!.CanAct(ActionInstance.CanActArgs!);

        public bool Cost()
            => ActionInstance!.Action!.Cost(ActionInstance.CostArgs!);

        public bool Act()
            => ActionInstance!.Action!.Act(ActionInstance.ActArgs!);

        public bool Outcome()
            => ActionInstance!.Action!.Outcome(ActionInstance.OutcomeArgs!);

        public RpgActivity SetArgValue(string arg, object? value)
        {
            ActionInstance!.CanActArgs!.Set(arg, value);
            ActionInstance.CostArgs!.Set(arg, value);
            ActionInstance.ActArgs!.Set(arg, value);
            ActionInstance.OutcomeArgs!.Set(arg, value);
            ActionInstance.AutoCompleteArgs!.Set(arg, value);

            return this;
        }

        public void SetArgValues(Dictionary<string, string?> argValues)
        {
            ActionInstance!.CanActArgs!.Set(argValues);
            ActionInstance.CostArgs!.Set(argValues);
            ActionInstance.ActArgs!.Set(argValues);
            ActionInstance.OutcomeArgs!.Set(argValues);
            ActionInstance.AutoCompleteArgs!.Set(argValues);
        }

        public void AutoComplete()
        {
            ActionInstance!.CanActArgs!.FillFrom(ActionInstance.AutoCompleteArgs!);
            if (!CanAct())
                throw new InvalidOperationException("Cannot AutoComplete");

            ActionInstance!.CostArgs!.FillFrom(ActionInstance.AutoCompleteArgs!);
            Cost();
            Graph!.Time.TriggerEvent();

            ActionInstance!.ActArgs!.FillFrom(ActionInstance.AutoCompleteArgs!);
            Act();
            Graph!.Time.TriggerEvent();

            ActionInstance!.OutcomeArgs!.FillFrom(ActionInstance.AutoCompleteArgs!);
            Outcome();
            Graph!.Time.TriggerEvent();
        }

        public string GetArgPropName(ActionInstance actionInstance, string argName)
            => $"{actionInstance.ActionName}/{actionInstance.ActionNo}/{argName}";

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);
            Time = graph.Time.Current;
            Name = $"{InitiatorId}/{Time.Type}/{Time.Tick}/{ActivityNo}";

            var modSet = GetModSetByName(Name) as ModSet;
            if (modSet == null)
            {
                modSet = new ModSet(InitiatorId, new TurnLifecycle(), Name);
                modSet.Unapply();

                OutcomeSets.Add(modSet);
            }
        }

        private void InitInstanceProps(ActionInstance instance)
        {
            var props = instance.AutoCompleteArgs!.Args.Values
                .Where(x => x.TypeName == nameof(Int32) || x.TypeName == nameof(Dice));

            foreach (var prop in props)
                GetProp(GetArgPropName(instance, prop.Name), create: true);
        }

        public void InitInstanceArgs(ActionInstance actionInstance)
        {
            actionInstance.CanActArgs!.Set(this, actionInstance.Owner, Initiator);
            actionInstance.CostArgs!.Set(this, actionInstance.Owner, Initiator);
            actionInstance.ActArgs!.Set(this, actionInstance.Owner, Initiator);
            actionInstance.OutcomeArgs!.Set(this, actionInstance.Owner, Initiator);
            actionInstance.AutoCompleteArgs!.Set(this, actionInstance.Owner, Initiator);
        }
    }
}
