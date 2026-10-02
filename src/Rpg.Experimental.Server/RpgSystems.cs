using Rpg.Experimental.Reflection;
using Rpg.Experimental.System;
using System.Reflection;

namespace Rpg.Experimental.Server
{
    /// <summary>
    /// The game systems a host knows about. A system is registered once, at start up, and its meta data is
    /// built then.
    /// </summary>
    public class RpgSystems
    {
        private readonly Dictionary<string, RpgSystem> _systems = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new();

        /// <summary>
        /// Register a game system. The assembly it is defined in is scanned for its objects, actions and
        /// states, along with any other assemblies given.
        /// </summary>
        public RpgSystem Register(IRpgSystem system, params Assembly[] additionalAssemblies)
        {
            lock (_lock)
            {
                RpgTypeUtilities.RegisterAssembly(system.GetType().Assembly);
                foreach (var assembly in additionalAssemblies)
                    RpgTypeUtilities.RegisterAssembly(assembly);

                var built = RpgSystemFactory.Build(system, additionalAssemblies);
                _systems[built.Identifier] = built;

                return built;
            }
        }

        public RpgSystem[] All()
        {
            lock (_lock)
                return _systems.Values.ToArray();
        }

        public RpgSystem? Find(string? identifier)
        {
            lock (_lock)
                return identifier != null && _systems.TryGetValue(identifier, out var system)
                    ? system
                    : null;
        }

        public RpgSystem Get(string identifier)
            => Find(identifier)
                ?? throw new RpgServerException($"System '{identifier}' not found");
    }
}
