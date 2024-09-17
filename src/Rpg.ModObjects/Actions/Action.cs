using Rpg.ModObjects.Reflection;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Actions
{
    public abstract class Action
    {
        protected RpgGraph Graph { get; private set; }

        [JsonInclude] public string Id { get; protected set; }
        [JsonInclude] public string Name { get; protected set; }
        [JsonInclude] public string OwnerId { get; protected set; }
        [JsonInclude] public string OwnerArchetype { get; protected set; }

        [JsonInclude] public RpgMethod<Action, bool> OnCanAct { get; protected set; }
        [JsonInclude] public RpgMethod<Action, bool> OnCost { get; protected set; }
        [JsonInclude] public RpgMethod<Action, bool> OnAct { get; protected set; }
        [JsonInclude] public RpgMethod<Action, bool> OnOutcome { get; protected set; }

        [JsonInclude] public string[] CanPerformAfter { get; protected set; }

        [JsonConstructor] protected Action()
        {
            var type = GetType();

            Id = this.NewId();
            Name = type.Name;
            OwnerArchetype = GetOwnerArchetype(type) ?? nameof(RpgEntity);
        }

        public Action(RpgEntity owner)
            : this()
        {
            OwnerId = owner.Id;
            Name = GetType().Name;
            OnCanAct = RpgMethod.Create<Action, bool>(this, nameof(OnCanAct))!;
            OnCost = RpgMethod.Create<Action, bool>(this, nameof(OnCost))!;
            OnAct = RpgMethod.Create<Action, bool>(this, nameof(OnAct))!;
            OnOutcome = RpgMethod.Create<Action, bool>(this, nameof(OnOutcome))!;
        }

        public void OnAdding(RpgGraph graph)
        {
            Graph = graph;
        }

        //public RpgArgSet CanActArgs()
        //    => OnCanAct.CreateArgSet();

        //public RpgArgSet CostArgs()
        //    => OnCost.CreateArgSet();

        //public bool CanAct(RpgArgSet argSet)
        //    => OnCanAct.Execute(this, argSet);

        public bool CanAct(Dictionary<string, object?> args)
            => OnCanAct.Execute(this, args);

        public bool Cost(Dictionary<string, object?> args)
            => OnCost.Execute(this, args);

        //public RpgArgSet ActArgs() 
        //    => OnAct.CreateArgSet();

        //public bool Act(RpgArgSet args)
        //    => OnAct.Execute(this, args);

        public bool Act(Dictionary<string, object?> args)
            => OnAct.Execute(this, args);

        //public RpgArgSet OutcomeArgs() 
        //    => OnOutcome.CreateArgSet();

        //public bool Outcome(RpgArgSet args)
        //    => OnOutcome.Execute(this, args);

        public bool Outcome(Dictionary<string, object?> args)
            => OnOutcome.Execute(this, args);

        public static Action[] CreateOwnerActions(RpgObject entity)
        {
            var actions = new List<Action>();

            var types = RpgTypeScan.ForTypes<Action>()
                .Where(x => IsOwnerActionType(entity, x));

            foreach (var type in types)
            {
                var action = (Action)Activator.CreateInstance(type, [entity])!;
                if (entity.IsA(action.OwnerArchetype!))
                    actions.Add(action);
            }

            return actions.ToArray();
        }

        private static bool IsOwnerActionType(RpgObject entity, Type? actionType)
        {
            while (actionType != null)
            {
                if (actionType.IsGenericType)
                {
                    var genericTypes = actionType.GetGenericArguments();
                    if (genericTypes.Length == 1 && entity.GetType().IsAssignableTo(genericTypes[0]))
                        return true;
                }

                actionType = actionType.BaseType;
            }

            return false;
        }

        private static string? GetOwnerArchetype(Type? actionType)
        {
            if (actionType != null && actionType.IsAssignableTo(typeof(Actions.Action)))
            {
                while (actionType != null)
                {
                    if (actionType.IsGenericType)
                    {
                        var genericTypes = actionType.GetGenericArguments();
                        if (genericTypes.Length == 1)
                            return genericTypes[0].Name;
                    }

                    actionType = actionType.BaseType;
                }
            }

            return null;
        }

    }

    public abstract class Action<TOwner> : Action
        where TOwner : RpgEntity
    {
        [JsonConstructor] protected Action() { }

        public Action(TOwner owner)
            : base(owner)
        { }
    }
}