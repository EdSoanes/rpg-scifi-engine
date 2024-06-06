using Newtonsoft.Json;
using Rpg.ModObjects.Actions;

namespace Rpg.ModObjects.Meta
{
    public class MetaAction
    {
        [JsonProperty] public string ActionName { get; private set; }
        [JsonProperty] public string? OutcomeAction { get; private set; }
        [JsonProperty] public string[] EnabledWhen { get; private set; }
        [JsonProperty] public string[] DisabledWhen { get; private set; }
        [JsonProperty] public MetaActionArg[] Args { get; private set; } = Array.Empty<MetaActionArg>();

        public MetaAction(string actionName, RpgActionAttribute actionAttr, MetaActionArg[]? actionArgs)
        {
            ActionName = actionName;
            OutcomeAction = actionAttr.OutcomeMethod;
            Args = actionArgs ?? Array.Empty<MetaActionArg>();
            EnabledWhen = CreateStateList(actionAttr.EnabledWhen, actionAttr.EnabledWhenAll);
            DisabledWhen = CreateStateList(actionAttr.DisabledWhen, actionAttr.DisabledWhenAll);
        }

        private string[] CreateStateList(string? state, IEnumerable<string>? states)
        {
            var res = new List<string>();
            if (!string.IsNullOrEmpty(state))
                res.Add(state);

            if (states != null)
                foreach (var item in states.Where(x => !string.IsNullOrEmpty(x)))
                    res.Add(item);

            return res.Distinct().ToArray();
        }
    }
}
