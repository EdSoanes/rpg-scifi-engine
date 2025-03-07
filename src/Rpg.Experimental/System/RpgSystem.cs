using Rpg.Experimental.System.Props;

namespace Rpg.Experimental.System
{
    public class RpgSystem : IRpgSystem
    {
        public string Identifier { get; internal set; }
        public string[]? Namespaces { get; internal set; }
        public string Name { get; internal set; }
        public string Version { get; internal set; }
        public string Description { get; internal set; }

        public MetaObject[] Objects { get; internal set; } = [];
        public MetaAction[] Actions { get; internal set; } = [];
        public MetaState[] States { get; internal set; } = [];
        public RpgPropertyAttribute[] PropertyAttributes { get; internal set; } = [];

        public MetaObject? GetMetaObject(string? archetype)
            => Objects.FirstOrDefault(x => x.Archetypes.Contains(archetype));
    }
}
