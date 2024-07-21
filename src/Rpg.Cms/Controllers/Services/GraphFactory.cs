using Rpg.ModObjects;
using System.Net;

namespace Rpg.Cms.Controllers.Services
{
    public class GraphFactory : IGraphFactory
    {
        private readonly IContentFactory _contentFactory;

        public GraphFactory(IContentFactory contentFactory)
        {
            _contentFactory = contentFactory;
        }

        public RpgGraph HydrateGraph(string systemIdentifier, RpgGraphState graphState)
        {
            var system = MetaSystems.Get(systemIdentifier);
            if (system == null)
                throw new HttpRequestException($"System {system} not found", null, HttpStatusCode.BadRequest);

            var graph = new RpgGraph(graphState);
            graph.Time.TriggerEvent();

            return graph;
        }

        public RpgGraphState CreateGraphState(string systemIdentifier, string archetype, string entityIdentifier)
        {
            var entity = _contentFactory.GetEntity(systemIdentifier, archetype, entityIdentifier);
            var graph = new RpgGraph(entity);

            var graphState = graph.GetGraphState();
            return graphState;
        }
    }
}
