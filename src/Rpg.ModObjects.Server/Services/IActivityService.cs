using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Server.Ops;

namespace Rpg.ModObjects.Server.Services
{
    public interface IActivityService
    {
        ActionGroup[] GetActionGroups(string systemIdentifier);
        Activity Create(RpgGraph graph, ActivityCreate createActivity);
        Activity Create(string systemIdentifier, RpgGraph graph, ActivityCreateByGroup createActivity);

        Activity Act(RpgGraph graph, ActivityAct activityAct);
        Activity AutoComplete(RpgGraph graph, ActivityAutoComplete activityAutoComplete);
        Activity Complete(RpgGraph graph, ActivityComplete activityComplete);
        Activity Outcome(RpgGraph graph, ActivityOutcome activityOutcome);
    }
}