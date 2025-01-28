using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Reflection.Args;

namespace Rpg.Experimental
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

            var args = RpgArg.CreateDictionary(graph, Args);
            var result = _method?.Execute(_action!, args) ?? true;

            IsDone = true;

            return result;
        }

        public void Reset(RpgActivityAction activityAction)
        {
            IsDone = _method == null;
            //Reset relevant mod sets here...
        }

        public void OnCreating(RpgGraph graph, RpgAction? action, RpgMethod<RpgAction, bool>? method)
        {
            _action = action;
            _method = method;
            IsDone = _method == null;
            Args = RpgArg.CreateArgs(graph, _method);
        }

        public void OnRestoring(RpgGraph graph, RpgAction? action, RpgMethod<RpgAction, bool>? method)
        {
            _action = action;
            _method = method;
        }
    }
}
