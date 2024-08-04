using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Server.Ops;

namespace Rpg.ModObjects.Server.Services
{
    public interface IActivityService
    {
        Activity Act(RpgGraph graph, ActivityAct activityAct);
        Activity AutoComplete(RpgGraph graph, ActivityAutoComplete activityAutoComplete);
        Activity Complete(RpgGraph graph, ActivityComplete activityComplete);
        Activity Create(RpgGraph graph, ActivityCreate createActivity);
        Activity Outcome(RpgGraph graph, ActivityOutcome activityOutcome);
    }
}