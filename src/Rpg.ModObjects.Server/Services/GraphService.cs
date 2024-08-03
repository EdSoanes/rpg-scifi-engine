using Rpg.ModObjects.Meta;

namespace Rpg.ModObjects.Server.Services
{
    public class GraphService : IGraphService
    {
        private readonly IContentFactory _contentFactory;

        public GraphService(IContentFactory contentFactory)
        {
            _contentFactory = contentFactory;
        }

        public RpgGraph CreateGraph(string systemIdentifier, string archetype, string contentId)
        {
            var entity = _contentFactory.CreateEntity(systemIdentifier, archetype, contentId);
            if (entity == null)
                throw new ArgumentException($"Content with Id {contentId} not found");

            var graph = new RpgGraph(entity);

            return graph;
        }

        public RpgGraph HydrateGraph(string systemIdentifier, RpgGraphState graphState)
        {
            var system = MetaSystems.Get(systemIdentifier);
            if (system == null)
                throw new ArgumentException($"System {system} not found");

            var graph = new RpgGraph(graphState);
            graph.Time.TriggerEvent();

            return graph;
        }

        public RpgGraphState DehydrateGraph(RpgGraph graph)
        {
            var graphState = graph.GetGraphState();
            return graphState;
        }
    }
}
