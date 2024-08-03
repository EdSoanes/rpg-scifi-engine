using Rpg.ModObjects.Server.Ops;

namespace Rpg.ModObjects.Server.Services
{
    public interface IActivityService
    {
        RpgActivity Act(RpgGraph graph, ActivityAct activityAct);
        RpgActivity AutoComplete(RpgGraph graph, ActivityAutoComplete activityAutoComplete);
        RpgActivity Complete(RpgGraph graph, ActivityComplete activityComplete);
        RpgActivity Create(RpgGraph graph, ActivityCreate createActivity);
        RpgActivity Outcome(RpgGraph graph, ActivityOutcome activityOutcome);
    }
}