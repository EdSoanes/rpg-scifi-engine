using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Server.Ops;

namespace Rpg.ModObjects.Server
{
    public interface IRpgSessionlessServer
    {
        RpgContent[] ListEntities(string system);
        RpgResponse<string> CreateGraphState(string system, string archetype, string id);

        ActivityTemplate[] ActivityTemplates(string system);
        RpgResponse<Activity> ActivityCreate(string system, RpgRequest<ActivityCreate> request);
        RpgResponse<Activity> ActivityCreate(string system, RpgRequest<ActivityCreateByTemplate> request);

        RpgResponse<Activity> ActivityAct(string system, RpgRequest<ActivityAct> request);
        RpgResponse<Activity> ActivityAutoComplete(string system, RpgRequest<ActivityAutoComplete> request);
        RpgResponse<Activity> ActivityComplete(string system, RpgRequest<ActivityComplete> request);
        RpgResponse<Activity> ActivityOutcome(string system, RpgRequest<ActivityOutcome> request);
        RpgResponse<bool> ApplyModSet(string system, RpgRequest<ModSet> request);
        RpgResponse<PropDesc> Describe(string system, RpgRequest<Describe> request);
        RpgResponse<string> SetState(string system, RpgRequest<SetState> request);
    }
}