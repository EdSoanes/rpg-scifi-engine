namespace Rpg.Experimental.Tests.Models
{
    internal class TestSystem : IRpgSystem
    {
        public string Identifier { get => "Test"; }

        public string[]? Namespaces { get; set; }

        public string Name { get => "Test System"; }

        public string Version { get => "0.1"; }

        public string Description { get => "Test system for unit testing"; }
    }
}
