using Rpg.Experimental.System;

namespace Rpg.Cms.Services
{
    public class SyncSessionFactory
    {
        public SyncSession CreateSession(Guid userKey, RpgSystem system)
            => new SyncSession(userKey, system);
    }
}
