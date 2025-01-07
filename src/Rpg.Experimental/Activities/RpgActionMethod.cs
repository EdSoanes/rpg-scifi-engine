using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Reflection.Args;

namespace Rpg.Experimental.Activities
{
    public sealed class RpgActionMethod
    {
        private RpgAction? _action;
        private RpgMethod<RpgAction, bool>? _method;

        [JsonProperty] public RpgArg[] Args { get; private set; } = [];
        [JsonProperty] public bool CanAutoComplete { get => _action != null && !IsDone && Args.IsComplete(); }
        [JsonProperty] public bool IsDone { get; private set; }

        public bool Execute(RpgGraph graph)
        {
            if (!CanAutoComplete) return false;

            var args = Args.ToDictionary(graph);
            var result = _method?.Execute(_action!, args) ?? true;

            IsDone = true;

            return result;
        }

        public void Reset(RpgActivityAction activityAction)
        {
            IsDone = false;
            //Reset relevant mod sets here...
        }

        public void OnCreating(RpgActivityAction activityAction, string methodName)
        {
            _action = activityAction.GetAction();
            _method = methodName switch
            {
                MethodNames.Cost => _action?.CostMethod,
                MethodNames.Perform => _action?.PerformMethod,
                MethodNames.Outcome => _action?.OutcomeMethod,
                _ => null
            };

            Args = _action?.ActionArgs.CloneArgs(methodName) ?? [];
        }

        public void OnRestoring(RpgActivityAction activityAction)
        {
            _action = activityAction.GetAction();
        }
    }
}
