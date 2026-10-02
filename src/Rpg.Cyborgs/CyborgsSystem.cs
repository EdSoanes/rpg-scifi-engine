using Rpg.Experimental;

namespace Rpg.Cyborgs
{
    public class CyborgsSystem : IRpgSystem
    {
        public string Identifier { get => "Cyborgs"; }

        public string[]? Namespaces { get; set; }

        public string Name { get => "Cyborgs & Sidearms"; }

        public string Version { get => "0.1"; }

        public string Description { get => "Cyborgs & Sidearms tabletop rpg system"; }

        public string[] TimeEvents { get => ["Sunrise", "Sunset"]; }
    }
}
