using Rpg.ModObjects.Meta;

namespace Rpg.Cms.Services
{
    public class SyncSessionFactory
    {
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
