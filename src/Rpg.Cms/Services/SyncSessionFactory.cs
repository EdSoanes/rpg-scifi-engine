using Rpg.ModObjects.Meta;
using static Umbraco.Cms.Core.Constants;

namespace Rpg.Cms.Services
{
    public class SyncSessionFactory
    {
        public IMetaSystem[] GetSystems()
            => MetaGraph.DiscoverMetaSystems();

        public IMetaSystem? GetSystem(string identifier, bool build = true)
        {
            var system = MetaGraph.DiscoverMetaSystems().FirstOrDefault(x => x.Identifier == identifier);
            if (system != null && build)
            {
                var meta = new MetaGraph();
                system = meta.Build(system);
            }

            return system;
        }

        public SyncSession CreateSession(Guid userKey, IMetaSystem system)
        {
            var meta = new MetaGraph();
            system = meta.Build(system);
            if (system != null)
            {
                var session = new SyncSession(userKey, system);
                return session;
            }

            throw new InvalidOperationException("Could not create session");
        }

        public SyncSession CreateSession(Guid userKey)
        {
            var meta = new MetaGraph();
            var system = meta.Build();
            if (system != null)
            {
                var session = new SyncSession(userKey, system);
                return session;
            }

            throw new InvalidOperationException("Could not create session");
        }
    }
}
