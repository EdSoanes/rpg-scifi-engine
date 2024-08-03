namespace Rpg.ModObjects.Server.Services
{
    public interface IGraphService
    {
        RpgGraph CreateGraph(string systemIdentifier, string archetype, string entityIdentifier);
        RpgGraph HydrateGraph(string systemIdentifier, RpgGraphState graphState);
        RpgGraphState DehydrateGraph(RpgGraph graph);
    }
}