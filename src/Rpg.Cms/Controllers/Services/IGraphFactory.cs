using Rpg.ModObjects;

namespace Rpg.Cms.Controllers.Services
{
    public interface IGraphFactory
    {
        RpgGraph HydrateGraph(string systemIdentifier, RpgGraphState graphState);
        RpgGraphState CreateGraphState(string systemIdentifier, string archetype, string entityIdentifier);
    }
}